//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DREditor.Dialogues;
using DREditor.EventObjects;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Video;
using DREditor.Characters;
using System.Diagnostics;
using System;
using System.Linq;
using Debug = UnityEngine.Debug;
using DREditor.Dialogues.Events;
using DREditor.Gates;
using DREditor.Camera;
using DREditor.FPC;
using static UnityEngine.InputSystem.InputAction;

public class DialogueAnimConfig : MonoBehaviour
{
    //[SerializeField] AudioClip showUISound = null;
    //[SerializeField] AudioClip hideUISound = null;
    // FMod versions of above
    [Header("Debugging")]
    [SerializeField] bool debugBody = false;
    [Header("Dialogue UI Mode Main Objects")]
    [SerializeField] RawImage charSpriteOne = null;
    [SerializeField] RawImage charSpriteTwo = null;
    [SerializeField] RectTransform charSpeakingObject = null;
    [SerializeField] RectTransform charSpeakingObjectT = null;

    [Header("Dialogue UI Mode Protag Objects")]
    [SerializeField] RawImage protagSpriteOne = null;
    [SerializeField] RawImage protagSpriteTwo = null;
    [SerializeField] RectTransform protagSpeakingObject = null;
    [SerializeField] RectTransform protagSpeakingObjectT = null;

    [Header("Camera's and Animators")]
    [SerializeField] Camera mainCamera = null;
    [SerializeField] Camera dialogueCamera = null;
    [SerializeField] Camera blurCamera = null;
    [SerializeField] Camera protagCamera = null;
    [SerializeField] Animator uiAnimator = null;
    public Animator reticleAnimator = null;
    public Animator mainAnimator = null;
    public static DialogueAnimConfig instance = null;

    [Header("Dialogue Animation")]
    //[SerializeField] VFXDatabase VFXDB = null;
    [SerializeField] Animator protagUI = null;
    public bool protagUIIsShown = false;
    public Canvas dialogueCanvas = null;
    public Canvas mainCanvas = null;
    public Canvas ReticleCanvas = null;
    public Canvas FreezeCanvas = null;
    public RawImage freezeImage = null;
    public RawImage freezeBack = null;
    public RawImage protagFreezeImage = null;
    public BoolWithEvent inDialogue = null;
    public BoolWithEvent inTPFD = null;
    [SerializeField] int focusLayer = 8;
    [SerializeField] int dialogueLayer = 6;
    [SerializeField] Volume blurProfile = null;
    bool blurOn = true;
    [SerializeField] Image FlashWhiteImage = null;
    

    [Header("Time Floats for Animation Lengths")]
    [SerializeField] float uISpriteChangeTime = 0.185f;
    [SerializeField] float shakeDuration = 0.5f;
    [SerializeField] float lookAtPlayerTime = 1;
    [SerializeField] float showUIWaitTime = 1;
    //[SerializeField] float reticleAnimTime = 1;
    public float characterPanTime = 0.3f;
    public float spriteChangeTime = 0.185f;
    public float uiSpriteYPos = -298;
    public float uiSpriteXPos = -71;
    Quaternion ogCameraLook;
    public RenderTexture rt;
    public RenderTexture prt;
    [Header("Audio")]
    [SerializeField] AudioClip showUISound = null;
    [SerializeField] AudioClip hideUISound = null;
    //[SerializeField][EventRef] string showInstantUISound = "";
    [SerializeField] AudioClip investigationSound = null;
    Actor startActor = null;
    /// <remark>
    /// Added below because when saving and loading in the middle of dialogue player camera would snap
    /// due to smoothmouselook script snapping to the loaded value 
    /// </remark>
    bool skipOGCamLook = false;
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        dialogueCanvas.enabled = false;
    }
    public void ToggleDebugBody(CallbackContext ctx)
    {
        debugBody = !debugBody;
    }
    private void Start()
    {
        UIHandler.ToTitle += ToTitle;
        DialogueEventSystem.StartListening("ToggleBlur", ToggleBlur);
        DialogueEventSystem.StartListening("CheckFocus", CheckFocus);
        DialogueEventSystem.StartListening("FaintAnim", Faint);
        DialogueEventSystem.StartListening("CharacterLeave", CharacterLeave);
        DialogueEventSystem.StartListening("ChangeLast", ChangeLast);
        if (!mainCamera)
        {
            try
            {
                mainCamera = PlayerManager.instance.mainCamera;
                dialogueCamera = PlayerManager.instance.dialogueCamera;
                blurCamera = PlayerManager.instance.blurCamera;
            }
            catch
            {
                Debug.LogWarning("An Error occured trying to set the cameras for " +
                    "DialogueAnimConfig. Please ensure you have a PlayerSystem in the scene");
            }
        }
        //Application.targetFrameRate = 30;
    }
    private void OnDisable()
    {
        UIHandler.ToTitle -= ToTitle;
    }
    private void Update()
    {
        /* Below causes capture main texture to be visible through
         * letterbox when doing Alt+Enter to full screen the game.
         */
        //if (rt != null) { Graphics.Blit(rt, (RenderTexture)null); }
        //if (prt != null) { Graphics.Blit(prt, (RenderTexture)null); }
        if (debugBody)
        {
            dialogueCamera.transform.position = DialogueAssetReader.instance.currentActor.body.position;
        }
    }
    void ToTitle()
    {
        skipOGCamLook = false;
        mainCamera.enabled = true;
        if (inDialogue.Value && GameManager.instance.currentMode != GameManager.Mode.Trial)
        {
            Debug.LogWarning("Stopping dialogue");
            Reset = true;
            StopDialogue();
        }
        fadedActors.Clear();
        freezeImage.texture = null;
        freezeImage.DOKill();
        freezeImage.color = new Color(1, 1, 1, 1);
        protagUI.Rebind();
        
    }
    /// <summary>
    /// This function disables multiple canvases and cameras
    /// </summary>
    public void DisableVisuals()
    {
        rt = null;
        prt = null;
        FreezeCanvas.enabled = false;
        Debug.LogWarning("Called Disable Visuals");
        //freezeImage.DOFade(0, 0);

        if (protagUIIsShown)
        {
            protagUI.Play("HideProtag");
            protagUIIsShown = false;
            MoveBackDialogue();
        }


        dialogueCamera.enabled = false;
        blurCamera.enabled = false;
        HideDialogueUI();
        dialogueCanvas.enabled = false;

        inDialogue.Value = false;

    }
    public void EnableCanvases(bool to)
    {
        if (dialogueCamera != null)
        {
            dialogueCamera.enabled = to;
            blurCamera.enabled = to;
            dialogueCanvas.enabled = to;
            mainCanvas.enabled = to;
            FreezeCanvas.enabled = to;
        }
        //Debug.LogWarning("CG Set Canvases to " + to);
    }
    public void EnableFreezeCanvas(bool to)
    {
        FreezeCanvas.enabled = to;
    }
    public void EnableReticleCanvas(bool to)
    {
        //Debug.LogWarning("Reticle Canvas to: " + to);
        ReticleCanvas.enabled = to;
    }
    Stopwatch timer = new Stopwatch();
    public void StartDialogue(Actor actor, Dialogue[] dialogues, ItemActor item = null, bool inleaving = false)
    {
        if (dialogues.Length == 0 || !dialogues[0])
        {
            StopDialogue();
            return;
        }
        GameManager.SetControls("dialogue");
        Debug.Log("Start Dialogue is: " + dialogues[0].name);
        if (!inleaving && DialogueAssetReader.instance.autoMode)
        {
            DialogueAssetReader.instance.autoIcon.gameObject.SetActive(true);
            DialogueAssetReader.instance.autoIcon.TurnOn();
        }
        if (dialogues[0].IsInstant)
            InstantDialogue(dialogues,inleaving);
        else
            StartCoroutine(BeginDialogue(actor, dialogues, item));
    }
    IEnumerator BeginDialogue(Actor actor, Dialogue[] dialogues, ItemActor item = null)
    {
        
        DialogueAssetReader.CallStart();
        timer.Reset();
        timer.Start();
        GameManager.instance.cantBeInMenu = true;
        DialogueAssetReader.SaveDialogueName = dialogues[0].name; // Might need to put here if gamesave.loadingfile is false
        if (inDialogue.Value != true)
            inDialogue.Value = true;

        DialogueAssetReader.instance.lastActor = null;
        startActor = actor;
        //Camera.main.targetTexture = null;
        if (!GameSaver.LoadingFile)
        {
            dialogueCamera.transform.position = mainCamera.transform.position;
            dialogueCamera.transform.rotation = mainCamera.transform.rotation;
        }
        else
            skipOGCamLook = true;

        ogCameraLook = mainCamera.transform.rotation;
        
        EvaluateNameplate(dialogues);
        if (!(GameManager.instance.currentMode == GameManager.Mode.TPFD))
        {
            if (item != null)
                LookAtHead(item);
            else if (actor != null)
                LookAtHead(actor);
        }

        if (!GameSaver.LoadingFile)
        {

            yield return new WaitForSeconds(lookAtPlayerTime);
        }
        else
        {
            //AudioListener.volume = 0;
            // TO-DO: FMod version of above
        }

        freezeImage.DOFade(1, 0);
        
        yield return new WaitForEndOfFrame();
        
        CaptureMain();

        reticleAnimator.SetBool("ItemIcon", false);
        reticleAnimator.SetBool("SpeakIcon", false);
        //mainCamera.enabled = false;
        yield return new WaitForSeconds(0.5f);
        //At this point we want to have a freeze frame of whats on the main camera
        //Turn off the quad and turn on the sprite renderer
        if (item == null && actor != null)
        {
            ChangeRender(actor);
        }


        ReticleCanvas.enabled = false;
        mainCamera.enabled = false;

        if (item != null)
        {
            //Debug.LogWarning("Triggered Look at Item Body");
            if (item.body != null)
                LookDialogue(item);
            else
                Debug.LogWarning("ITEM DOESN'T HAVE A BODY");
        }
        else if(actor != null)
        {
            //Debug.LogWarning("Triggered Look at Actor Body");
            if(!GameSaver.LoadingFile)
                LookDialogue(actor);
            FocusActor(actor);
            DialogueAssetReader.instance.lastActor = actor;
        }

        dialogueCanvas.enabled = true;
        ShowDialogueUI();
        if (!GlobalFade.instance.IsDark)
        {
            SoundManager.instance.PlaySFX(showUISound); //Fmod
        }
        
        if (!GameSaver.LoadingFile)
        {
            yield return new WaitForSeconds(showUIWaitTime);
            ReticleCanvas.enabled = false;
        }
        else
        {
            //ShowMainUI();
        }
        
        Debug.Log("At Start of Dialogue Loading File is: " + GameSaver.LoadingFile);
        if (GameSaver.LoadingFile)
            DialogueAssetReader.instance.LoadThrough(dialogues[0], 0, 0);
        else
            DialogueAssetReader.instance.DialoguePlay(dialogues);
        yield return null;
    }
    public void CaptureMain()
    {
        Debug.LogWarning("InInstant: " + inInstant + "  Canvas on: " + FreezeCanvas.enabled);
        if (inInstant || FreezeCanvas.enabled)
        {
            Debug.LogWarning("Tried Capturing Main");
            return;
        }
        Debug.LogWarning("Captured Main, Freeze Image on");
        if (!mainCamera.enabled)
        {
            Debug.LogWarning("Main Camera wasn't on before capturing.");
            mainCamera.enabled = true;
        }
        
        rt = new RenderTexture(Screen.width, Screen.height, 32);
        mainCamera.targetTexture = rt;
        freezeImage.texture = rt;
        Camera.main.Render();
        Camera.main.targetTexture = null;
        FreezeCanvas.enabled = true;
        Vector2 v = freezeImage.rectTransform.parent.GetComponent<RectTransform>().sizeDelta;
        //Debug.LogWarning(v.x + " " + v.y + " " + freezeImage.rectTransform.parent.GetComponent<RectTransform>().gameObject.name);

        freezeImage.rectTransform.sizeDelta = v;

        //Debug.LogWarning("Freeze Canvas Should be On");

        //freezeImage.DOKill();
        freezeImage.DOColor(new Color(1, 1, 1, 1), 0);
        mainCamera.enabled = false;
    }
    public void CaptureProtag()
    {
        if (!protagCamera.enabled)
        {
            Debug.LogWarning("Protag Camera wasn't on before capturing.");
            protagCamera.enabled = true;
        }
        prt = new RenderTexture(Screen.width, Screen.height, 32);
        protagCamera.targetTexture = prt;
        protagFreezeImage.texture = prt;
        protagCamera.Render();
        protagCamera.targetTexture = null;
        protagCamera.enabled = false;
    }
    void EvaluateNameplate(Dialogue[] dialogues)
    {
        foreach(Dialogue d in dialogues)
        {
            foreach(Line line in d.Lines)
            {
                Character c = DialogueAssetReader.instance.FindChar(line.Speaker.FirstName);
                if (c != null)
                {
                    DialogueAssetReader.instance.namePlate.texture = c.Nameplate;
                    if (line.AliasNumber > 0)
                        DialogueAssetReader.instance.namePlate.texture = c.Aliases[line.AliasNumber-1].Nameplate;
                    return;
                }
            }
        }
    }
    public bool inInstant = false;
    public void InstantDialogue(Dialogue[] dialogues, bool inleaving = false)
    {
        if (DialogueAssetReader.instance.ffMode)
            DialogueAssetReader.instance.ffMode = false;
        //Debug.LogWarning("Instant Dialogue Started");
        if (GameSaver.LoadingFile)
            ShowMainUI();
        inDialogue.SetValue(true);
        DialogueAssetReader.CallStart();
        inInstant = true;
        dialogueCanvas.enabled = true;
        ReticleCanvas.enabled = false;
        uiAnimator.Play("ShowInstant");
        SoundManager.instance.PlayInstantSound();
        if (inleaving)
        {
            DialogueAssetReader.instance.autoIcon.gameObject.SetActive(false);
        }
        StartCoroutine(PlayInstant(dialogues, inleaving));
    }
    IEnumerator PlayInstant(Dialogue[] dialogues, bool inleaving = false)
    {
        yield return new WaitForSeconds(1);
        DialogueAssetReader.SaveDialogueName = dialogues[0].name;

        DialogueAssetReader.instance.DialoguePlay(dialogues, inleaving);
        yield break;
    }
    public void StopDialogue() => StartCoroutine(EndDialogue());
    public static Gate MoveAtEnd = null;
    public static bool Reset = false; // true when going to main menu from pause menu
    // Below 3 things were for muting sound because when going to the trial prep you'd hear
    // the dialogue UI close So this combined with whats on TrialPrepUI.cs Fixes that
    public static bool MuteEndDialogue = false;
    public delegate void voidDel();
    public static event voidDel OnFinishedDialogue;
    IEnumerator EndDialogue()
    {
        Debug.LogWarning("Called End Dialogue");
        if (CGPlayer.inCG)
        {
            DialogueEventSystem.TriggerEvent("HideCG");
            yield return new WaitUntil(() => !CGPlayer.inCG);
        }
        if (CGPlayer.inVid)
        {
            DialogueEventSystem.TriggerEvent("HideVideo", new SVTuple());
            yield return new WaitUntil(() => !CGPlayer.inVid);
        }
        if (inInstant)
        {
            inInstant = false;
            uiAnimator.Play("HideInstant");
            yield return new WaitForSeconds(1.5f);
            reticleAnimator.SetBool("ConfirmSelect", false);
            dialogueCanvas.enabled = false;
            if (!Door.inLeaveProcess)
            {
                //Debug.LogWarning("Showing Reticle");
                ReticleCanvas.enabled = true;
            }
            
            DialogueAssetReader.CallEnd();
            inDialogue.Value = false;
            GameManager.instance.cantBeInMenu = false;
            DialogueAssetReader.instance.diaAutoIcon.gameObject.SetActive(true);
            DialogueAssetReader.instance.cgAutoIcon.gameObject.SetActive(true);
            DialogueAssetReader.CallEnd();
            Debug.Log("Ended instant dialogue");
            if (GameManager.instance.currentMode == GameManager.Mode.TPFD)
                GameManager.SetControls("tpfd");
            if (GameManager.instance.currentMode == GameManager.Mode.ThreeD)
                GameManager.SetControls("threed");
            yield break;
        }
        if (protagUIIsShown)
        {
            protagUI.Play("HideProtag");
            protagUIIsShown = false;
            MoveBackDialogue();
            yield return new WaitForSeconds(uiAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        
        HideDialogueUI();
        if (!GlobalFade.instance.IsDark && !MuteEndDialogue && !Reset)
        {
            SoundManager.instance.PlaySFX(hideUISound); //FMod
        }
        if (!skipOGCamLook)
            mainCamera.transform.DORotateQuaternion(ogCameraLook, lookAtPlayerTime);
        else
            skipOGCamLook = false;
        dialogueCamera.enabled = false;
        blurCamera.enabled = false;
        if (DialogueAssetReader.instance.lastActor != null && !DialogueAssetReader.instance.lastActor.character.enabled)
        {
            UnFocusActor(DialogueAssetReader.instance.lastActor);
            ChangeRender(DialogueAssetReader.instance.lastActor);
        }
        if (startActor != null && !startActor.character.enabled)
        {
            UnFocusActor(startActor);
            ChangeRender(startActor);
            startActor = null;
        }
        DialogueAssetReader.CallEndEvent();
        CheckFocus(null); // if this causes problems for whatever reason I could just remove this
        mainCamera.enabled = true;
        Debug.LogWarning("Fading out freezeimage");
        freezeImage.DOFade(0, 0.5f);
        if(!Reset)
            yield return new WaitForSecondsRealtime(0.5f);
        FreezeCanvas.enabled = false;
        Debug.LogWarning("Disabled Freeze Canvas");
        if (!Reset)
            yield return new WaitForSecondsRealtime(uiAnimator.GetCurrentAnimatorStateInfo(0).length);

        

        if (!CGPlayer.inEndVideo)
        {
            ReticleCanvas.enabled = true;
            dialogueCanvas.enabled = false;
        }

        

        
        DialogueTextConfig.instance.ClearText();
        FinishDialogue();
        yield break;
    }
    void FinishDialogue()
    {
        
        if (GameManager.instance.currentMode != GameManager.Mode.Trial)
            GameManager.instance.cantBeInMenu = false;

        DialogueAssetReader.CallEnd();
        Debug.Log("Finish Dialogue Called");
        inDialogue.Value = false;
        

        timer.Stop();
        TimeSpan timeTaken = timer.Elapsed;
        string foo = "Time taken to finish dialogue: " + timeTaken.ToString(@"m\:ss\.fff");
        UnityEngine.Debug.Log(foo);

        if (!Door.inLeaveProcess)
        {
            reticleAnimator.SetBool("ConfirmSelect", false);
            reticleAnimator.SetBool("Appear", true);
        }

        if (MoveAtEnd != null && !Door.inLeaveProcess)
        {
            Door.inLeaveProcess = true;
            PlayerManager.instance.EnableScripts(false);
            PlayerManager.instance.EnableControlMono(false);
            RoomLoader.instance.ChangeRoom(MoveAtEnd);
            MoveAtEnd = null;
        }
        CheckFocus(null); // if this causes problems for whatever reason I could just remove this
        Reset = false;
        RaycastReticle.canSelectOverride = true;
        for (int i = 0; i < fadedActors.Count; i++)
            if (fadedActors[i] != null && fadedActors[i].sprite.color.a == 0)
                fadedActors[i].sprite.DOFade(1, 0);
        fadedActors.Clear();
        DialogueAssetReader.instance.inLeaving = false;
        if(DialogueAssetReader.instance.diaAutoIcon != null)
            DialogueAssetReader.instance.diaAutoIcon.gameObject.SetActive(true);
        if(DialogueAssetReader.instance.cgAutoIcon != null)
            DialogueAssetReader.instance.cgAutoIcon.gameObject.SetActive(true);
        if (GameManager.instance.currentMode == GameManager.Mode.TPFD)
            GameManager.SetControls("tpfd");
        if (GameManager.instance.currentMode == GameManager.Mode.ThreeD)
            GameManager.SetControls("threed");
        OnFinishedDialogue?.Invoke();
    }
    /// <summary>
    /// This is for when you go to a tpfd scene while in the middle of dialogue
    /// to set the main camera at the correct tpfd place
    /// </summary>
    public void SetCamTPFD()
    {
        //Debug.Log("SETCAMTPFD CALLED");
        //if (!inTPFD.Value)
            //inTPFD.Value = true;
        TPFDManager tPFDManager = FindObjectOfType<TPFDManager>();
        tPFDManager.StartEarly();
        tPFDManager.Set();
        PnCCamera pn = FindObjectOfType<PnCCamera>();
        mainCamera.transform.rotation = pn.GetCalculatedRotation();
        mainCamera.transform.position = pn.GetCalculatedPosition();
        dialogueCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);
        CaptureMain();
        ogCameraLook = mainCamera.transform.rotation;
    }
    public void ShowMainUI(bool showing)
    {
        if (showing)
            mainAnimator.Play("ShowMainUI");
        else
            mainAnimator.Play("HideMainUI");
    }
    public void ChangeRender(Actor actor)
    {
        if (actor == null || actor.sprite == null)
            return;
        if (!actor.sprite.enabled)
        {
            actor.gameObject.GetComponent<MeshRenderer>().enabled = false;
            actor.characterb.enabled = false;
            actor.sprite.enabled = true;
            actor.spriteb.enabled = true;
        }
        else
        {
            actor.gameObject.GetComponent<MeshRenderer>().enabled = true;
            actor.characterb.enabled = true;
            actor.sprite.enabled = false;
            actor.spriteb.enabled = false;
        }
    }

    #region Dialogue UI Animations
    public void ShowDialogueUI()
    {
        dialogueCamera.enabled = true;
        blurCamera.enabled = true;
        uiAnimator.SetBool("ShowDUI", true);
    }

    public void HideDialogueUI() => uiAnimator.SetBool("ShowDUI", false);
    public void MoveDialogue() => uiAnimator.SetBool("MoveDUI", true);
    public void MoveBackDialogue() => uiAnimator.SetBool("MoveDUI", false);
    public void HideDialogueBox(bool to) => uiAnimator.SetBool("HideBox", to);
    public void ShowProtag()
    {
        protagCamera.enabled = true;
        CaptureProtag();
        protagUI.Play("ShowProtag");
        protagUIIsShown = true;
        MoveDialogue();
    }
    public void HideProtag()
    {
        protagUI.Play("HideProtag");
        protagUIIsShown = false;
        MoveBackDialogue();
        protagCamera.enabled = false;
    }
    public void ShowMainUI() => mainAnimator.Play("ShowMainUI");
    public void HideMainUI() => mainAnimator.Play("HideMainUI");

    #endregion

    #region Camera Animations
    public void LookAtHead(Actor actor)
    {
        if(actor.head != null)
            mainCamera.transform.DOLookAt(actor.head.position, lookAtPlayerTime);
        // Put a look at middle of the character THEN the Looking at characters eyes

    }
    public void LookDialogue(Actor actor)
    {
        if (!actor)
        {
            Debug.LogWarning("NOTIFY: LOOK DIALOGUE WAS CALLED BUT THE ACTOR WAS NULL");
            // This has been found when pan to char actor was null called through the asset reader
            return;
        }
        if (!blurOn)
            ToggleBlur(true);
        if (!actor.body)
            Debug.LogWarning(actor.GetComponentInParent<Transform>().gameObject.name + " doesn't have a body");
        else if (DialogueAssetReader.instance.ffMode || GameSaver.LoadingFile)
        {
            /* DOKill is here because during testing, PostTrialDialogue, Kai's : That's uh.. that's not
             * Loading to that line caused the camera to pan to tozu because there were so many calls to it
             * from load to line. Just noting this in case I forget.
             */
            //Debug.LogWarning("Dialogue Camera Was Killed");
            dialogueCamera.transform.DOKill();
            dialogueCamera.transform.SetPositionAndRotation(actor.body.position, actor.body.rotation);
            //Debug.LogWarning("Looking at: " + actor.displayName);
        }
        else
        {
            dialogueCamera.transform.DOKill();
            //Debug.LogWarning("Looking at: " + actor.displayName);
            //Debug.LogWarning("Dialogue Camera Was Killed");
            DiaCamEvents.t = dialogueCamera.transform.DOMove(actor.body.position, characterPanTime);
            dialogueCamera.transform.DORotateQuaternion(actor.body.rotation, characterPanTime);
        }
    }
    public void LookAtHead(ItemActor actor)
    {
        // Put a look at middle of the character THEN the Looking at characters eyes
        if (!actor.head)
            Debug.LogWarning(actor.GetComponentInParent<Transform>().gameObject.name + " doesn't have a head");
        else
            mainCamera.transform.DOLookAt(actor.head.position, lookAtPlayerTime);
        //Debug.LogWarning("Triggered Look at Item Head");
    }

    public void LookDialogue(ItemActor actor)
    {
        if (blurOn)
            ToggleBlur(false);
        dialogueCamera.transform.DOMove(actor.body.position, characterPanTime);
        dialogueCamera.transform.DORotateQuaternion(actor.body.rotation, characterPanTime);
    }
    public void ChangeFocus(Actor newActor, Actor oldActor)
    {
        if(!GameSaver.LoadingFile)
            LookDialogue(newActor);
        if (newActor != oldActor)
        {
            CheckFocus(null);
            ChangeRender(newActor);
            if (oldActor != null && oldActor.sprite.enabled)
                ChangeRender(oldActor);
            
            FocusActor(newActor);
            UnFocusActor(oldActor);
        }
        else if (!newActor.sprite.enabled)
        {
            ChangeRender(newActor);
            FocusActor(newActor);
        }
        if (newActor.sprite.gameObject.layer != focusLayer)
            FocusActor(newActor);
    }
    public void FocusActor(Actor actor)
    {
        if (actor == null || actor.sprite == null)
            return;
        actor.gameObject.layer = focusLayer;
        actor.characterb.gameObject.layer = focusLayer;
        actor.sprite.gameObject.layer = focusLayer;
        actor.spriteb.gameObject.layer = focusLayer;
    }
    public void UnFocusActor(Actor actor)
    {
        if (actor == null || actor.characterb == null)
            return;
        actor.gameObject.layer = dialogueLayer;
        actor.characterb.gameObject.layer = dialogueLayer;
        actor.sprite.gameObject.layer = dialogueLayer;
        actor.spriteb.gameObject.layer = dialogueLayer;
    }
    public void ShakeCamera(float intensity, bool protagSpeaking)
    {
        if (protagSpeaking)
        {
            protagCamera.DOShakePosition(shakeDuration,intensity, 15 ,0,true);
        }
        else
        {
            dialogueCamera.DOShakePosition(shakeDuration, intensity, 15, 0, true);
        }
    }
    public void MoveCamera(string objectName)
    {
        // Maybe make a Singleton in the room that we can take/reference from so we don't have to use .Find?
        GameObject o = GameObject.Find(objectName);
        if(o != null)
        {
            dialogueCamera.transform.DOMove(o.transform.position, 1);
            dialogueCamera.transform.DOLocalRotateQuaternion(o.transform.rotation, 1);
        }
    }
    #endregion

    #region FindActor
    public Actor FindActor(string actorName)
    {
        var actors = FindObjectsOfType<Actor>().Where(n => n.gameObject.transform.parent.name == actorName);
        if (actors.Count() > 0)
        {
            //Debug.LogWarning("Found Actor: " + actorName);
            return actors.ElementAt(0);
        }
        else
        {
            //Debug.Log("Couldn't find actor: " + actorName);
        }
        return null;
    }
    #endregion

    // Do a static bool and then do wait until in asset reader
    public static bool SpriteChanged = false;

    #region Dialogue UI Mode Change Sprite
    // Use this Function to use the Dialogue UI if we're in a situation where there aren't any actors 
    // This function is mostly used for protag changes, but is set up in case no 3D actors
    public void UISpriteChange(Line currentLine, bool damonSpeak) => StartCoroutine(UIChangeSprite(currentLine, damonSpeak));
    IEnumerator UIChangeSprite(Line currentLine, bool damonSpeak)
    {
        RawImage spriteOne = null;
        RawImage spriteTwo = null;
        RectTransform spriteOneObject = null;
        RectTransform spriteTwoObject = null;
        Material currentExpression = currentLine.Expression.Sprite;
        if (currentExpression != null && !damonSpeak)
        {
            spriteOne = charSpriteOne;
            spriteTwo = charSpriteTwo;
            spriteOneObject = charSpeakingObject;
            spriteTwoObject = charSpeakingObjectT;
        }
        else if (currentExpression != null && damonSpeak)
        {
            spriteOne = protagSpriteOne;
            spriteTwo = protagSpriteTwo;
            spriteOneObject = protagSpeakingObject;
            spriteTwoObject = protagSpeakingObjectT;
        }
        if (currentExpression != null && DialogueAssetReader.instance.ffMode)
        {
            Texture tex = currentExpression.mainTexture;
            if (damonSpeak)
                spriteOneObject.GetComponent<RectTransform>().localPosition = new Vector3(uiSpriteXPos, uiSpriteYPos, 0);//new Vector3(0, -200, 0);
            else
                spriteOneObject.GetComponent<RectTransform>().localPosition = new Vector3(0, -513, 0);
            
            spriteTwo.texture = tex;
            spriteTwoObject.sizeDelta = new Vector2(tex.width, tex.height);
            spriteOne.texture = tex;
            spriteOneObject.sizeDelta = new Vector2(tex.width, tex.height);
            yield break;
        }
        if (spriteOne != null)
        {

            Texture tex = currentExpression.mainTexture;
            //spriteOne.DOFade(0, 0);
            //spriteTwo.DOFade(0, 0);
            yield return new WaitForSeconds(0);
            if (damonSpeak)
                spriteOneObject.GetComponent<RectTransform>().localPosition = new Vector3(uiSpriteXPos, uiSpriteYPos, 0);//new Vector3(0, -200, 0);
            else
                spriteOneObject.GetComponent<RectTransform>().localPosition = new Vector3(0, -513, 0);

            spriteTwo.texture = tex;
            spriteTwoObject.sizeDelta = new Vector2(tex.width, tex.height);

            spriteTwo.DOFade(1, uISpriteChangeTime)
                .SetEase(Ease.Linear);// Bottom Sprite fade in
            spriteOne.DOFade(0, uISpriteChangeTime)
                .SetEase(Ease.Linear);  //Top Sprite fade out


            //yield return new WaitForSeconds(0.285f);
            spriteOneObject.sizeDelta = new Vector2(tex.width, tex.height);
            //spriteOneObject.GetComponent<RectTransform>().SetPositionAndRotation(new Vector3(960, spriteOneObject.transform.position.y - tex.height , 0), new Quaternion());
            //yield return new WaitForSeconds(0);
            //spriteTwo.DOFade(0, 0f);
            //yield return new WaitForSeconds(0);
            
            yield return new WaitForSeconds(uISpriteChangeTime);
            
            //spriteTwoObject.GetComponent<RectTransform>().SetPositionAndRotation(new Vector3(0, -163, 0), new Quaternion());
            yield return new WaitForSeconds(0);
            spriteOne.texture = tex;
            spriteOne.DOFade(1, 0);
            spriteTwo.DOFade(0, 0);
        }
        yield return null;
    }
    #endregion

    #region World Space Change Sprite
    
    public void WorldSpriteChange(Line currentLine,  Actor actor) => StartCoroutine(WorldChangeSprite(currentLine, actor));
    IEnumerator WorldChangeSprite(Line currentLine,  Actor currentActor)
    {
        if (currentLine.Expression.Sprite == null)
        {
            Debug.LogWarning("Sprite was Null");
            SpriteChanged = true;
            yield break;
        }

        //Debug.LogWarning(currentActor);
        //Debug.LogWarning(currentActor.displayName);
        if(currentActor == null)
        {
            Debug.LogWarning("ACTOR WAS NULL for character: " + currentLine.Speaker.FirstName);
            SpriteChanged = true;
            yield break;
        }
        SpriteRenderer spriteOne = currentActor.sprite;
        SpriteRenderer spriteTwo = currentActor.spriteb;
        
        //Material currentExpression = currentLine.Expression.Sprite;
        Sprite mySprite = currentLine.Speaker.Sprites[currentLine.ExpressionNumber - 1].Sprite;
        
        if (currentLine.Expression.Sprite != null && currentActor.sprite.sprite.name.Contains(mySprite.name))
        {
            //Debug.Log(currentActor.sprite.sprite.name);
            //Debug.Log(mySprite.name);
            //Debug.LogWarning("Was the same");
            SpriteChanged = true;
            yield break;
        }
        if (DialogueAssetReader.instance.ffMode)
        {
            //currentActor.sprite.DOKill();
            //currentActor.sprite.color = new Color(255, 255, 255, 255);
            /* The above line causes a visual kinda bug where pressing fast forward, a sprite changes, then 
             * going out of fast forward, will incorrectly fade the sprite transition. But this line seems 
             * needed for fixing the bug of CharLeave overpowering sprite change.
             */
            currentActor.character.material = currentLine.Expression.Sprite;
            currentActor.characterb.material.mainTexture = currentActor.character.material.mainTexture;
            spriteOne.sprite = mySprite;
            spriteTwo.sprite = mySprite;
            spriteOne.DOFade(1, 0);
            SpriteChanged = true;
            yield break;
        }
        //currentActor.sprite.DOKill();
        if (currentLine.Expression.Sprite != null)
        {
            currentActor.character.material = currentLine.Expression.Sprite;
            currentActor.characterb.material.mainTexture = currentActor.character.material.mainTexture;
            

            spriteTwo.sprite = mySprite;
            spriteTwo.DOFade(1, 0.185f)
                .SetEase(Ease.Linear);  // Bottom Sprite fade in
            spriteOne.DOFade(0, 0.185f)
                .SetEase(Ease.Linear);  //Top Sprite fade out
            
            yield return new WaitForSeconds(spriteChangeTime);

            spriteOne.sprite = mySprite;
            spriteOne.DOFade(1, 0);
            spriteTwo.DOFade(0, 0);
            
            yield return new WaitForSeconds(0);
        }
        SpriteChanged = true;
        yield break;
    }
    #endregion

    #region Change Last Actor's Sprite
    private void ChangeLast(object o = null)
    {
        LASTuple t = (LASTuple)o;
        if (t.spriteNum == 0)
        {
            Debug.LogWarning("ChangeLast Detected you changed it to \"No Sprite\" ");
            return;
        }
        Actor actor = FindActor(DialogueAssetReader.instance.database.Characters[t.charNum].FirstName);
        if (GameSaver.LoadingFile)
        {
            ChangeFocus(actor, null);
            return;
        }
        StartCoroutine(ChangeLastActorSprite(t, actor));

    }
    IEnumerator ChangeLastActorSprite(LASTuple t, Actor currentActor)
    {
        Character c = DialogueAssetReader.instance.database.Characters[t.charNum];


        SpriteRenderer spriteOne = currentActor.sprite;
        SpriteRenderer spriteTwo = currentActor.spriteb;

        //Material currentExpression = currentLine.Expression.Sprite;
        Sprite mySprite = c.Sprites[t.spriteNum - 1].Sprite;

        if (c.Expressions[t.spriteNum - 1].Sprite != null && currentActor.sprite.sprite.name.Contains(mySprite.name))
        {
            Debug.Log(currentActor.sprite.sprite.name);
            Debug.Log(mySprite.name);
            Debug.LogWarning("Was the same");
            SpriteChanged = true;
            yield break;
        }
        if (DialogueAssetReader.instance.ffMode)
        {

            currentActor.character.material = c.Expressions[t.spriteNum - 1].Sprite;
            currentActor.characterb.material.mainTexture = currentActor.character.material.mainTexture;
            spriteOne.sprite = mySprite;
            spriteTwo.sprite = mySprite;
            SpriteChanged = true;
            yield break;
        }
        if (c.Expressions[t.spriteNum - 1].Sprite != null)
        {
            currentActor.character.material = c.Expressions[t.spriteNum - 1].Sprite;
            currentActor.characterb.material.mainTexture = currentActor.character.material.mainTexture;


            spriteTwo.sprite = mySprite;
            spriteTwo.DOFade(1, 0.185f)
                .SetEase(Ease.Linear);  // Bottom Sprite fade in
            spriteOne.DOFade(0, 0.185f)
                .SetEase(Ease.Linear);  //Top Sprite fade out

            yield return new WaitForSeconds(spriteChangeTime);

            spriteOne.sprite = mySprite;
            spriteOne.DOFade(1, 0);
            spriteTwo.DOFade(0, 0);

            yield return new WaitForSeconds(0);
        }
        SpriteChanged = true;
        yield break;
    }
    #endregion

    public static bool fading = false;
    List<Actor> fadedActors = new List<Actor>();
    private void CharacterLeave(object o = null)
    {
        fading = true;
        CLTuple t = (CLTuple)o;
        //Debug.LogWarning(" Leave Setting is: " + t.current + " " + !DialogueAssetReader.instance.protagSpeaking
            //+ " " + DialogueAssetReader.instance.currentActor.displayName + " " + t.appear);
        
        if (t.appear)
            if (!t.current)
                ActorAppear(DialogueAssetReader.instance.lastActor, t.exit);
            else
                ActorAppear(DialogueAssetReader.instance.currentActor, t.exit);

        else if(t.current && !DialogueAssetReader.instance.protagSpeaking)
            ActorLeft(DialogueAssetReader.instance.currentActor, t.exit);
        else
            ActorLeft(DialogueAssetReader.instance.lastActor, t.exit);

    }
    public void ActorLeft(Actor actor, bool exit) // Character leave option
    {
        fadedActors.Add(actor);
        fading = true;
        StartCoroutine(Fading(actor, 0, exit));
        
        if (exit)
        {
            if (!DialogueAssetReader.OnLastLine)
            {
                if (GameSaver.LoadingFile && DialogueAssetReader.LoadThroughNLine)
                {
                    Debug.LogWarning("Loading File Skip Leaving: " + actor.displayName);
                    DialogueAssetReader.ExitChar(actor);
                }
                else
                {
                    System.EventHandler handler = null;
                    handler = (s, e) =>
                    {
                        DialogueAssetReader.ExitChar(actor);
                        DialogueAssetReader.OnLineStart -= handler;
                        DialogueAssetReader.delegates.Remove(handler);
                    };
                    DialogueAssetReader.delegates.Add(handler);
                    DialogueAssetReader.OnLineStart += handler;
                }
            }
            else
            {
                Debug.LogWarning("Adding to the end of the line");
                if (GameSaver.LoadingFile && DialogueAssetReader.LoadThroughNLine)
                {
                    DialogueAssetReader.ExitChar(actor);
                }
                else
                {
                    System.EventHandler handler = null;
                    handler = (s, e) =>
                    {
                        DialogueAssetReader.ExitChar(actor);
                        DialogueAssetReader.OnDialogueEndEvent -= handler;
                        DialogueAssetReader.delegates.Remove(handler);
                    };
                    DialogueAssetReader.delegates.Add(handler);
                    DialogueAssetReader.OnDialogueEndEvent += handler;
                    
                }
            }
        }
    }
    private void ActorAppear(Actor actor, bool exit)
    {
        StartCoroutine(Fading(actor, 1, exit));
    }
    IEnumerator Fading(Actor actor, float to, bool exit)
    {
        //Debug.LogWarning("Fading Called with float " + to);
        actor.sprite.DOFade(to, 1);
        yield return new WaitForSeconds(1);
        fading = false;
    }

    void ToggleBlur(object to)
    {
        blurProfile.enabled = (bool)to;
        blurOn = (bool)to;
    }
    /// <summary>
    /// Check's if a character is incorrectly focused and fixes it.
    /// </summary>
    /// <param name="o"></param>
    void CheckFocus(object o)
    {
        //if (CGPlayer.inCG || CGPlayer.inVid)
        //return;
        var actors = FindObjectsOfType<Actor>().Where(n => n.sprite.enabled);
        foreach(Actor a in actors)
        {
            //Debug.Log("Check Focus is Logging an Actor: " + a.displayName);
            ChangeRender(a);
            UnFocusActor(a);
        }
        
    }

    #region Blur Vision Animation
    
    [HideInInspector] public Tweener blurTween;
    VolumeProfile prof;
    float dofValue;
    public void BlurVision()
    {
        prof = blurProfile.profile;
        UniversalAdditionalCameraData data = dialogueCamera.gameObject.GetComponent<UniversalAdditionalCameraData>();
        //dialogueCamera.clearFlags = CameraClearFlags.Nothing;
        data.renderPostProcessing = true;
        DepthOfField dof = GetProfile();
        dofValue = dof.focalLength.value;
        dof.focalLength.value -= 50;
        blurTween = DOTween.To(() => dof.focalLength.value, x => dof.focalLength.value = x, 300, 1.75f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InFlash);
    }
    public void StopBlur()
    {
        blurTween.Kill();
        DepthOfField dof = GetProfile();
        DOTween.To(() => dof.focalLength.value, x => dof.focalLength.value = x, 150, 1);
        dof.focalLength.value = dofValue;
        UniversalAdditionalCameraData data = dialogueCamera.gameObject.GetComponent<UniversalAdditionalCameraData>();
        data.renderPostProcessing = false;
        blurTween = null;
        blurProfile.profile = prof;
    }
    DepthOfField GetProfile()
    {
        if (blurProfile.profile.TryGet(out DepthOfField tmp))
            return tmp;
        else
            return null;
    }
    #endregion

    public void FlashWhite()
    {
        FlashWhiteImage.DOFade(0.15f, 0.1f)
            .SetLoops(2, LoopType.Yoyo);
    }

    #region Fainting Animation
    public bool isFainting = false;
    public void Faint(object o)
    {
        FATuple data = (FATuple)o;
        if (data.faint)
            StartCoroutine(FaintAnim());
        else
            StartCoroutine(UnFaintAnim(data.to));
    }
    //void UnFaint(object o) => StartCoroutine(UnFaintAnim());
    IEnumerator FaintAnim()
    {
        Actor actor = DialogueAssetReader.instance.currentActor;
        isFainting = true;
        Transform t = actor.transform.parent.gameObject.transform;
        t.DOMoveY(t.localPosition.y - (actor.transform.localPosition.y / 2), 1.5f)
            .SetEase(Ease.InExpo);
        yield return new WaitForSeconds(1f);
        actor.sprite.DOFade(0, 0.5f);
        yield return new WaitForSeconds(0.3f);
        ShakeCamera(0.5f, false);
        yield return new WaitForSeconds(0.3f);
        t.DOMoveY(t.localPosition.y - 5, 0);
        isFainting = false;
        yield return null;
    }
    IEnumerator UnFaintAnim(float to)
    {
        Actor actor = DialogueAssetReader.instance.currentActor;
        isFainting = true;
        actor.sprite.DOFade(0, 0);
        Transform t = actor.transform.parent.gameObject.transform;
        //t.DOMoveY(t.localPosition.y - 5, 0);
        t.DOMoveY(to, 1.5f)
            .SetEase(Ease.OutExpo);
        
        yield return new WaitForSeconds(1f);
        actor.sprite.DOFade(1, 0.5f);
        yield return new WaitForSeconds(0.5f);
        isFainting = false;
        yield return null;
    }
    #endregion

    #region Start Investigation
    /// <summary>
    /// The Goal is to End dialogue normally,
    /// except at the very end, it doesn't give the player back control,
    /// we instead set up Investigation and then give player control.
    /// </summary>
    public void StartInvestigation(StartInvestigation start) => StartCoroutine(ToInvestigation(start));
    IEnumerator ToInvestigation(StartInvestigation start)
    {
        if (CGPlayer.inCG)
        {
            DialogueEventSystem.TriggerEvent("HideCG");
            yield return new WaitUntil(() => !CGPlayer.inCG);
        }
        if (inInstant)
        {
            inInstant = false;
            uiAnimator.Play("HideInstant");
            reticleAnimator.SetBool("ConfirmSelect", false);
            dialogueCanvas.enabled = false;
            if (!Door.inLeaveProcess)
            {
                ReticleCanvas.enabled = true;
            }
            //DialogueAssetReader.instance.FFModeOff(new UnityEngine.InputSystem.InputAction.CallbackContext());
            DialogueAssetReader.CallEnd();
            inDialogue.Value = false;
            GameManager.instance.cantBeInMenu = false;
            Debug.Log("Ended instant dialogue");
            yield break;
        }
        if (protagUIIsShown)
        {
            protagUI.Play("HideProtag");
            protagUIIsShown = false;
            MoveBackDialogue();
            yield return new WaitForSeconds(uiAnimator.GetCurrentAnimatorStateInfo(0).length);
        }
        PlayerManager.instance.EnableMonoScripts(false);
        reticleAnimator.SetBool("ConfirmSelect", true);
        HideDialogueUI();
        mainCamera.transform.DORotateQuaternion(ogCameraLook, lookAtPlayerTime);
        dialogueCamera.enabled = false;
        blurCamera.enabled = false;
        if (DialogueAssetReader.instance.lastActor != null && !DialogueAssetReader.instance.lastActor.character.enabled)
        {
            UnFocusActor(DialogueAssetReader.instance.lastActor);
            ChangeRender(DialogueAssetReader.instance.lastActor);
        }
        if (startActor != null && !startActor.character.enabled)
        {
            UnFocusActor(startActor);
            ChangeRender(startActor);
            startActor = null;
        }

        mainCamera.enabled = true;
        freezeImage.DOFade(0, 0.5f);
        yield return new WaitForSeconds(0.5f);
        FreezeCanvas.enabled = false;
        Debug.LogWarning("Disabled Freeze Canvas");

        yield return new WaitForSeconds(uiAnimator.GetCurrentAnimatorStateInfo(0).length);
        

        if (!CGPlayer.inEndVideo)
        {
            ReticleCanvas.enabled = true;
            dialogueCanvas.enabled = false;
        }
        
        DialogueTextConfig.instance.ClearText();




        // Visual has Ended here, Starting Investigation

        HideMainUI();

        GlobalFade.instance.FadeTo(0.5f);
        yield return new WaitForSeconds(0.5f);
        GameManager.instance.addState = AddState.Investigation;
        if (start.NextObjective)
        {
            ProgressionManager.instance.ChangeObjective();
            yield return new WaitForEndOfFrame();
            RoomManager.instance.UnLoadActors(false);
            RoomManager.UnloadEvents();
            RoomManager.instance.LoadRoomInstance(ProgressionManager.instance.CurrentObjective, null);
            yield return new WaitUntil(() => RoomManager.RoomLoaded);
            
        }
        if (GameManager.instance.currentState != GameManager.State.Deadly)
            GameManager.instance.ChangeState(GameManager.State.Deadly);

        GlobalFade.instance.FadeOut(0.5f);
        yield return new WaitForSeconds(0.5f);

        // Room has Loaded Next Objective Room if used and faded out of black screen
        // and now the Start Investigation animation must show
        if (InvestigationHandler.Instance != null)
        {
            InvestigationHandler.Instance.InvestigationStart();
            if(investigationSound != null)
                SoundManager.instance.PlaySFX(investigationSound);
            yield return new WaitForEndOfFrame();
            if (InvestigationHandler.Instance.GetStartLength() == float.PositiveInfinity)
            {
                yield return new WaitUntil(() => InvestigationHandler.Instance.GetStartLength() != float.PositiveInfinity);
            }
            yield return new WaitForSeconds(InvestigationHandler.Instance.GetStartLength());
            InvestigationHandler.Instance.TurnOffStartCanvas();
        }
        else
            Debug.LogWarning("There is no Investigation Handler!");

        // At this point the Investigation Start Animation has Finished and you can give the player control 
        // and show the main UI + Reticle

        ShowMainUI();

        if (start.NewDialogue != null)
        {
            Dialogue[] d = new Dialogue[1];
            d[0] = start.NewDialogue;
            StartDialogue(null, d);
        }
        else
        {
            reticleAnimator.SetBool("ConfirmSelect", false);
            FinishDialogue();
        }


        
        yield break;
    }

    #endregion
}
