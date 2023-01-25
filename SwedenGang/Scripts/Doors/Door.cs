//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DG.Tweening;
using DREditor.Dialogues;
using DREditor.Gates;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Linq;
using static UnityEngine.InputSystem.InputAction;

public class Door : MonoBehaviour, IDialogueHolder
{
    // Resources.load area database
    // set area by checking database for current area based on scene put in
    // set the doors destination from areas list of doors to
    // Add locks to the door
    // Global events are next with things like dialogue instance triggers and the like
    public static GateDatabase ADB => (GateDatabase)Resources.Load("Gates/GateDatabase");
    public static Gate CurrentArea => ADB.GetArea(SceneManager.GetActiveScene().name);
    //[SerializeField] string displayName;
    [Header("Gate to where the door leads to")]
    public Gate setGateway = null;
    //public Vector3 positionFromCurrent;
    //public bool lockedDialogue = false;
    [SerializeField] bool identified = true;
    [SerializeField] DoorData data = new DoorData();
    //public Transform comeFromPosition;
    [Header("If this room is TPFD, only one can be checked per room")]
    [SerializeField] bool isTPFD = false;
    [Header("If this Door is activated by walking into it and not selection")]
    [SerializeField] bool isHitbox = false;


    public static bool inLeaveProcess = false;
    public delegate void DoorDelegate();
    public static event DoorDelegate GlobalLockLifted;
    public static event DoorDelegate OnLeaveAsk;
    public static event DoorDelegate OnDidntLeave;
    DRControls _controls;
    
    private void Awake()
    {
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
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
    private void Start()
    {
        if (isTPFD)
        {
            RoomLoader.PreEndLoad += DoorControl;
        }
    }
    void DoorControl()
    {
        RoomLoader.PreEndLoad -= DoorControl;
        Debug.LogWarning("IsTPFD DOOR CONTROL HAS BEEN CALLED");
        _controls.Player.Leave.performed += EvaluateLock;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isHitbox && !inLeaveProcess)
            LockCheck();
    }
    void DisplayName() // Called from RaycastReticle.cs
    {
        if (identified)
            ItemDisplayer.instance.DisplayName(data.displayName);
        else
            ItemDisplayer.instance.DisplayName("???");
    }
    public static void InvokeGLL()
    {
        GlobalLockLifted?.Invoke();
    }
    void CallLock() => LockCheck(); // Called from raycast reticle
    void EvaluateLock(CallbackContext context) // Called from raycast reticle
    {
        if (!inLeaveProcess && !ObserveManager.ChangingObserve)
            LockCheck();
        //else
            //Debug.LogWarning("In Leave Process");
    }
    void LockCheck()
    {
        if (GameManager.instance.InMenu.Value || GameManager.instance.InDialogue.Value) // Denies Evaluation
            return;
        //Debug.LogWarning("LockCheck Called");
        inLeaveProcess = true;
        OnLeaveAsk?.Invoke();
        /*if (ProgressionManager.instance.CurrentObjective.ProgressionGate) // Progression Gate Scenario
        {
            if (NotProgressionGate())
            {
                DoorLocked(ProgressionManager.instance.LockedDialogue);
                return;
            }
        }*/
        CheckLocks();
    }
    void CheckLocks()
    {
        // if Locked and has lock dialogue
        //Debug.Log("Bool 1 for checking the lock is: " + (data.lockData.locked) + " " + 
            //(!data.lockData.IsUnlocked() && data.lockData.lockDialogue.Count > 0));
        if (data.lockData.locked || !data.lockData.IsUnlocked())
        {
            Debug.Log("Door is Locked and has Lock Dialogue");
            Dialogue d = LocalDialogue.EvaluateDialogue(data.lockData.lockDialogue);
            if (d != null)
                DoorLocked(d);
            else
            {
                Debug.LogWarning("Lock Dialogue is null!");
                PassedLock();
            }
        }
        else
        {
            PassedLock();
        }
        //Debug.Log("Bool 2 for checking the lock is: " + data.lockData.IsUnlocked());
        
    }
    void PassedLock()
    {
        if (isTPFD)
        {
            Debug.Log("TPFD Room Unlocked");
            //OnLeaveAsk?.Invoke();
            GameManager.instance.cantBeInMenu = true;
            Dialogue[] d = new Dialogue[1];
            if (!ProgressionManager.instance.LeaveAsk)
                Debug.LogError("Leave Ask has not been added! please add it to the progression manager");
            d[0] = ProgressionManager.instance.LeaveAsk;
            DialogueAssetReader.instance.choice1 = ChangeRoomTPFD;
            DialogueAssetReader.instance.choice2 = DummyNoOption;
            DialogueAnimConfig.instance.StartDialogue(null, d, null, true);

        }
        else
        {
            if (data.lockData.unlockedDialogue.Count > 0)
            {
                Dialogue[] d = new Dialogue[1];
                d[0] = LocalDialogue.EvaluateDialogue(data.lockData.unlockedDialogue);
                if (d[0])
                {
                    DialogueAnimConfig.instance.StartDialogue(null, d);
                    DialogueAssetReader.OnDialogueEnd += ChangeRoomAtEnd;
                }
                else
                {
                    Debug.LogWarning("Unlock Dialogue was Null! So just changing room.");
                    ChangeRoom();
                }

            }
            else
            {
                Debug.Log("Changing Room");
                ChangeRoom();
            }
        }
    }
    void ChangeRoomAtEnd()
    {
        Debug.Log("Changing Room");
        
        ChangeRoom();
        DialogueAssetReader.OnDialogueEnd -= ChangeRoomAtEnd;
    }
    void DoorLocked(Dialogue dia)
    {
        GameManager.instance.cantBeInMenu = true;
        Dialogue[] d = new Dialogue[1];
        d[0] = dia;
        if (dia != null)
            DialogueAnimConfig.instance.StartDialogue(null, d);
        else
            Debug.LogWarning("Door Locked Dialogue is null");
        inLeaveProcess = false;
    }
    void ChangeRoomTPFD()
    {
        // Incase someone wanted something special here
        ChangeRoom();
    }
    void ChangeRoom()
    {
        //ItemDisplayer.instance.HideName();
        //PlayerManager.instance.
        PlayerManager.instance.EnableControlMono(false);
        PlayerManager.instance.EnableScripts(false);
        PlayerManager.instance.TPFDControllable(false);
        if (!identified)
        {
            identified = true;
            data.identified = true;
        }
        GameObject.Find("Main Camera").GetComponent<DREditor.Camera.SmoothMouseLook>().enabled = false;
        // TO-DO: Wait for this to finish then load the level? Because it kinda freezes when doing this
        //DialogueAnimConfig.instance.ReticleCanvas.enabled = false;
        
        RoomLoader.instance.ChangeRoom(setGateway, data.options);
    }
    void DummyNoOption()
    {
        EventSystem.current.SetSelectedGameObject(null);
        DialogueAnimConfig.instance.ReticleCanvas.enabled = true;
        GameManager.SetControls("tpfd");
        inLeaveProcess = false;
        OnDidntLeave?.Invoke();
    }
    
    public DoorData Save()
    {
        data.doorName = gameObject.name;
        DoorData d = (DoorData)data.Clone();
        //d.lockData = (LockData)d.lockData.Clone();
        return d;
    }
    public void Load(DoorData data)
    {
        this.data = data;
        identified = data.identified;
    }
    public static void CallDidnt()
    {
        OnDidntLeave?.Invoke();
    }
    public void ClearLockData()
    {
        identified = true;
        data.ClearData();
    }
    private void OnDestroy()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Player.Leave.performed -= EvaluateLock;
        _controls.UI.Cancel.performed -= EvaluateLock;
#endif
    }

    public void CheckIfSaveDialogue()
    {
        if(data.lockData != null)
        {
            foreach (LocalDialogue d in data.lockData.lockDialogue)
            {
                DialogueData.CheckDialogue(d.dialogue);
                DialogueData.CheckDialogue(d.dialogueIfFlagNotMet);
            }
        }
    }
}

[Serializable]
public class DoorData
{
    public string doorName;
    public string displayName;
    public LoadRoomOptions options;
    public LockData lockData;
    public bool identified;
    public void ClearData()
    {
        lockData.ClearData();
    }
    public static DoorData MergeInstance(DoorData baseD, DoorData ins)
    {
        DoorData copy = (DoorData)baseD.Clone();
        copy.identified = ins.identified;
        
        return copy;
    }
    public object Clone()
    {
        DoorData copy = (DoorData)MemberwiseClone();
        DoorData data = new DoorData();
        data.doorName = copy.doorName;
        data.displayName = copy.displayName;
        data.options = (LoadRoomOptions)options.Clone();
        data.lockData = (LockData)lockData.Clone();
        data.identified = copy.identified;
        return data;
    }
}
[Serializable]
public class LockData
{
    public List<LocalDialogue> unlockedDialogue = new List<LocalDialogue>();
    public List<string> flagList = new List<string>();
    [Tooltip("Set to true if this door isn't intended to be ever unlocked for the current room save.")]
    public bool locked = false;
    public List<LocalDialogue> lockDialogue = new List<LocalDialogue>();
    public bool IsUnlocked() // returns true if all flags are triggered
    {
        if (flagList.Count == 0 && lockDialogue != null)
            return false;
        var list = flagList.ToArray().Where(n => ProgressionManager.instance.CheckFlag(n));
        return list.Count() == flagList.Count;
    }
    public void ClearData()
    {
        unlockedDialogue.Clear();
        flagList.Clear();
        lockDialogue.Clear();
        locked = false;
    }
    public object Clone()
    {
        LockData copy = (LockData)MemberwiseClone();
        LockData c = new LockData();
        c.unlockedDialogue = new List<LocalDialogue>();
        foreach (LocalDialogue l in copy.unlockedDialogue)
        {
            c.unlockedDialogue.Add((LocalDialogue)l.Clone());
        }
        c.lockDialogue = new List<LocalDialogue>();
        foreach (LocalDialogue l in copy.lockDialogue)
        {
            c.lockDialogue.Add((LocalDialogue)l.Clone());
        }
        c.flagList = new List<string>();
        foreach (string s in copy.flagList)
            c.flagList.Add((string)s.Clone());
        c.locked = copy.locked;
        return c;
    }
}
