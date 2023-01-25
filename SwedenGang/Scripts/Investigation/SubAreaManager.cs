//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DREditor.Dialogues;
using DREditor.EventObjects;
using DREditor.FPC;
using DREditor.Camera;
using DG.Tweening;
using UnityEngine.EventSystems;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(BoxCollider))]
public class SubAreaManager : MonoBehaviour, IDialogueHolder
{
    // This should be on the layer of dialogueobject
    
    [Header("Everything Showing in Data is only for debug viewing purposes")]
    public SubAreaData Data = new SubAreaData();
    [Header("Fill out Settings")]
    [SerializeField] string codeName = "";
    [SerializeField] string displayName;
    [SerializeField] float panToBodyTime = 1;
    [SerializeField] LockData leaveData;
    [SerializeField] Dialogue startDialogue = null;
    [SerializeField] Dialogue autoStartDialogue = null;
    [SerializeField] ItemActor focusedActor = null;
    //[SerializeField] TPFDManager manager = null; Might need this in case we have to call setting manually
    [SerializeField] List<ItemActor> items = new List<ItemActor>();
    [SerializeField] List<Collider> disableList = new List<Collider>();
    [Header("Required Objects")]
    [SerializeField] BoolWithEvent inDialogue = null;
    [SerializeField] BoolWithEvent inTPFD = null;
    [SerializeField] BoolWithEvent inMenu = null;
    [SerializeField] GameObject CamArrows = null;
    private GameObject ArrowRef;
#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif
    ControlMonobehaviours playerSystem;
    Camera cam;
    [SerializeField] BoxCollider box;
    private void Awake()
    {
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
        
        if (!inTPFD || !inDialogue)
            Debug.LogError("You need to assign inTPFD or inDialogue in the CorpseManager!");
    }
    private void Start()
    {
        playerSystem = GameObject.Find("PlayerSystem").GetComponent<ControlMonobehaviours>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        RoomLoader.OnLoad += CheckLoad;
        //Application.targetFrameRate = 15;
        //box = GetComponent<BoxCollider>();
    }
    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Enable();
#endif
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Disable();
#endif
    }
    public void DisplayName() // Called from RaycastReticle.cs
    {
        ItemDisplayer.instance.DisplayName(displayName);
    }
    public void HitByRay() // Called by RaycastReticle.cs
    {
        if (Data.leaveData.locked)
        {
            Dialogue[] d = new Dialogue[1];
            d[0] = LocalDialogue.EvaluateDialogue(leaveData.lockDialogue);
            if (d[0] != null)
            {
                DialogueAnimConfig.instance.InstantDialogue(d);
            }
            return;
        }
        if (startDialogue != null && !Data.finishedStart)
        {
            ItemDisplayer.instance.HideName();
            Dialogue[] d = new Dialogue[1];
            d[0] = startDialogue;
            DialogueAnimConfig.instance.StartDialogue(null, d, focusedActor);
            Data.isWaiting = true;
            DialogueAssetReader.OnDialogueEnd += StartUp;
        }
        else
            Activate();
    }
    void StartUp()
    {
        //RaycastReticle.Instance.FadeOut();
        if (autoStartDialogue && !Data.finishedAuto)
        {
            //RaycastReticle.Instance.FadeOut();
            RaycastReticle.Instance.ResetReticleVisual();
            RaycastReticle.Instance.EnableCanvas(false);
            inDialogue.Value = true;
        }
        Data.isWaiting = false;
        DialogueAssetReader.OnDialogueEnd -= StartUp;
        Data.finishedStart = true;
        Activate();
        
    }
    void Activate(bool to = true)
    {
        box.enabled = false;
        RaycastReticle r = (RaycastReticle)playerSystem.RaycastScript;
        r.UnSelectVisual();
        ItemDisplayer.instance.HideName();
        if (!GameSaver.LoadingFile)
        {
            Data.returnPosition = cam.transform.position;
            Data.returnRotation = cam.transform.eulerAngles;
            SmoothMouseLook mouse = cam.GetComponent<SmoothMouseLook>();
            Data.returnAbs = mouse.GetAbsolute();
            Data.returnSmooth = mouse.GetSmooth();
        }
        GameManager.instance.cantBeInMenu = true;
        //playerSystem.SaveBool();
        playerSystem.Enable(false);
        playerSystem.enabled = false;
        if(to)
            StartCoroutine(Activation());
        else
        {
            Debug.LogWarning("Cam moved");
            StartCoroutine(SLActivation());
        }
        ArrowRef = Instantiate(CamArrows);
    }
    IEnumerator SLActivation()
    {
        yield return new WaitUntil(() => !GameSaver.LoadingFile);
        PnCCamera pnc = (PnCCamera)playerSystem.TPFDScript;
        pnc.ClearPosition();
        cam.transform.DORotate(Vector3.zero, 0);
        cam.transform.DOMove(pnc.GetCalculatedPositionZ(), 0);
        yield break;
    }
    IEnumerator Activation()
    {
        if(autoStartDialogue && !Data.finishedAuto)
            yield return new WaitUntil(() => GameManager.instance.cantBeInMenu == false);
        GameManager.instance.cantBeInMenu = true;
        PnCCamera pnc = (PnCCamera)playerSystem.TPFDScript;
        pnc.ClearPosition();
        if (GameSaver.LoadingFile)
        {
            //Debug.LogWarning("Instant Set");
            cam.transform.DORotate(Vector3.zero, 0);
            cam.transform.DOMove(pnc.GetCalculatedPositionZ(), 0);
        }
        else
        {
            //Debug.LogWarning("Animation Set");
            cam.transform.DORotate(Vector3.zero, panToBodyTime);
            cam.transform.DOMove(pnc.GetCalculatedPositionZ(), panToBodyTime);
            yield return new WaitForSeconds(panToBodyTime);
        }


        if (autoStartDialogue && !Data.finishedAuto)
        {
            
            //Debug.LogWarning("Auto Start Dialogue Begun");
            Dialogue[] d = new Dialogue[1];
            d[0] = autoStartDialogue;
            DialogueAnimConfig.instance.StartDialogue(null, d, focusedActor);
            Data.isWaitingAuto = true;
            DialogueAssetReader.OnDialogueEnd += EndOfActivation;
            //yield return new WaitUntil(() => !inDialogue.Value);
        }
        else
            EndOfActivation();


        
        yield break;
    }
    /// <summary>
    /// End point of activation, sets the mode to a safe state
    /// </summary>
    void EndOfActivation()
    {
        Debug.LogWarning("End of Activation Started");
        if (autoStartDialogue && Data.isWaitingAuto)
        {
            DialogueAssetReader.OnDialogueEnd -= EndOfActivation;
            Data.isWaitingAuto = false;
            Data.finishedAuto = true;
            Debug.LogWarning("Finished End of activation");
        }
        
        inDialogue.Value = false;
        inTPFD.Value = true;
        playerSystem.TPFDScript.enabled = true;
        PnCCamera p = (PnCCamera)playerSystem.TPFDScript;
        p.Controllable = true;
        //inTPFD.Invoke();
        playerSystem.RaycastScript.enabled = true;
        if (!RaycastReticle.canSelect)
            RaycastReticle.canSelect = true;
        Data.activated = true;
        _controls.Player.Leave.performed += Release;
        EnableItems(true);
        GameManager.instance.cantBeInMenu = false;
    }
    /// <summary>
    /// Called when trying to leave the SubArea Mode
    /// </summary>
    /// <param name="context"></param>
    void Release(CallbackContext context)
    {
        if (!Door.inLeaveProcess && !inMenu.Value && !inDialogue.Value)
            CheckRelease();
    }

    void CheckRelease()
    {
        //OnLeaveAsk?.Invoke();
        Door.inLeaveProcess = true;
        playerSystem.TPFDScript.enabled = false;
        GameManager.instance.cantBeInMenu = true;

        if (!leaveData.IsUnlocked())
        {
            if (!LocalDialogue.EvaluateDialogue(leaveData.lockDialogue))
                Debug.LogWarning("Lock Dialogue has not been set in the SubAreaManager!");
            Dialogue[] d = new Dialogue[1];
            d[0] = LocalDialogue.EvaluateDialogue(leaveData.lockDialogue);
            if (d[0] != null)
            {
                DialogueAnimConfig.instance.InstantDialogue(d);
                DialogueAssetReader.OnDialogueEnd += AfterLock;
            }
            else
                CheckLeave();
        }
        else
            CheckLeave();
    }
    void AfterLock()
    {
        DialogueAssetReader.OnDialogueEnd -= AfterLock;
        Door.inLeaveProcess = false;
        DialogueAnimConfig.instance.ReticleCanvas.enabled = true;
        playerSystem.TPFDScript.enabled = true;
        GameManager.instance.cantBeInMenu = false;
    }
    void CheckLeave()
    {
        Dialogue[] d = new Dialogue[1];
        if (!ProgressionManager.instance.LeaveAsk)
            Debug.LogWarning("Leave Ask has not been added! please add it to the progression manager");
        d[0] = ProgressionManager.instance.LeaveAsk;
        DialogueAssetReader.instance.choice1 = Deactivate;
        DialogueAssetReader.instance.choice2 = DummyNoOption;
        DialogueAnimConfig.instance.InstantDialogue(d,true);
    }
    void DummyNoOption()
    {
        EventSystem.current.SetSelectedGameObject(null);
        DialogueAnimConfig.instance.ReticleCanvas.enabled = true;
        Door.inLeaveProcess = false;
        Door.CallDidnt();
        playerSystem.TPFDScript.enabled = true;
        GameManager.instance.cantBeInMenu = false;
    }
    void Deactivate()
    {
        _controls.Player.Leave.performed -= Release;
        playerSystem.Enable(false);
        playerSystem.enabled = false;
        inTPFD.Value = false;

        StartCoroutine(Deactivation());
    }
    IEnumerator Deactivation()
    {
        //RaycastReticle r = (RaycastReticle)playerSystem.RaycastScript;
        //r.UnSelectVisual();
        //Debug.LogWarning("Started Moving Camera " + Data.returnPosition);
        
        SmoothMouseLook mouse = cam.GetComponent<SmoothMouseLook>();
        //mouse.targetCharacterDirection = Vector2.zero;
        mouse.SetCorePos(Data.returnAbs, new Vector2(1.401298e-45f, 1.401298e-45f));
        // above smooth was new Vector2(1.401298e-45f, 1.401298e-45f)
        cam.transform.DORotate(Data.returnRotation, panToBodyTime);
        cam.transform.DOMove(Data.returnPosition, panToBodyTime);
        yield return new WaitForSeconds(panToBodyTime + 0.1f);

        //Debug.LogWarning("Ended Moving Camera");
        //playerSystem.LoadBool();
        playerSystem.enabled = true;
        playerSystem.Enable(true);

        RaycastReticle r = (RaycastReticle)playerSystem.RaycastScript;
        r.CenterReticle();
        ItemDisplayer.instance.HideName();
        EnableItems(false);
        box.enabled = true;
        Data.activated = false;
        Door.inLeaveProcess = false;
        Debug.LogWarning("Called Deactivation");
        inDialogue.Value = false;
        DialogueAnimConfig.instance.ReticleCanvas.enabled = true;
        Destroy(ArrowRef);
        yield break;
    }

    private void EnableItems(bool to)
    {
        foreach (ItemActor i in items)
        {
            i.box.enabled = to;
            i.SetSelectable(to);
        }
        foreach (Collider c in disableList)
            c.enabled = !to;
    }
    public SubAreaData Save()
    {
        Data.leaveData = leaveData;
        Data.codeName = codeName;
        return (SubAreaData)Data.Clone();
    }
    public void Load(SubAreaData areaData)
    {
        SubAreaData copy = (SubAreaData)areaData.Clone();
        Data = copy;
        leaveData = Data.leaveData;
        codeName = Data.codeName;
        if (Data.activated)
        {
            inTPFD.Value = true;
        }
        //SmoothMouseLook mouse = cam.GetComponent<SmoothMouseLook>();
        //mouse.targetCharacterDirection = Vector2.zero;
    }

    void CheckLoad() // when loaded sets up the script from where you left off
    {
        if (Data.activated)
            HitByRay();
        if (Data.isWaiting)
            DialogueAssetReader.OnDialogueEnd += StartUp;
        if (Data.isWaitingAuto)
        {
            //Debug.Log("Is Waiting Auto Was True");
            DialogueAssetReader.OnDialogueEnd += EndOfActivation;
            //DialogueAssetReader.OnDialogueEnd += StartUp;
            Activate(false);
        }
    }

    private void OnDestroy()
    {
        if (Data.isWaiting)
            DialogueAssetReader.OnDialogueEnd -= StartUp;
        if (Data.isWaitingAuto)
            DialogueAssetReader.OnDialogueEnd -= EndOfActivation;
        RoomLoader.OnLoad -= CheckLoad;
    }

    public void CheckIfSaveDialogue()
    {
        if (startDialogue)
            DialogueData.CheckDialogue(startDialogue);

        if (autoStartDialogue)
            DialogueData.CheckDialogue(autoStartDialogue);
    }
}

[Serializable]
public class SubAreaData
{
    public string codeName = "";
    public bool activated = false;
    public bool finishedStart = false;
    public bool finishedAuto = false;
    public bool isWaiting = false;
    public bool isWaitingAuto = false;
    // Position to return to when leaving
    public Vector3 returnPosition;
    public Vector3 returnRotation;
    public Vector2 returnAbs;
    public Vector2 returnSmooth;
    public LockData leaveData;
    public static SubAreaData MergeInstance(SubAreaData baseD, SubAreaData ins)
    {
        SubAreaData s = (SubAreaData)ins.Clone();
        s.leaveData = (LockData)baseD.leaveData.Clone();
        /*
        s.activated = ins.activated;
        s.finishedStart = ins.finishedStart;
        s.finishedAuto = ins.finishedAuto;
        s.isWaiting = ins.isWaiting;
        s.isWaitingAuto = ins.isWaitingAuto;
        s.returnPosition = ins.returnPosition;
        s.returnRotation*/
        return s;
    }
    public object Clone()
    {
        SubAreaData s = new SubAreaData();
        s.codeName = codeName;
        s.activated = activated;
        s.finishedStart = finishedStart;
        s.finishedAuto = finishedAuto;
        s.isWaiting = isWaiting;
        s.isWaitingAuto = isWaitingAuto;
        s.returnPosition = returnPosition;
        s.returnRotation = returnRotation;
        s.returnAbs = returnAbs;
        s.returnSmooth = returnSmooth;
        s.leaveData = (LockData)leaveData.Clone();
        return s;
    }
}
