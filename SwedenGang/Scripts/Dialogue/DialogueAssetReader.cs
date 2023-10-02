//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DREditor.Characters;
using DREditor.Dialogues;
using System.Linq;
using UnityEngine.EventSystems;
using DREditor.PlayerInfo;
using UnityEngine.InputSystem;
using DREditor.EventObjects;
using static UnityEngine.InputSystem.InputAction;
using DREditor.Dialogues.Events;
using System;

/// <summary>
/// Singleton that reads and uses assets with DialogueAnimConfig
/// Requires: GameManager, PlayerInfo, Progression Manager, GateDatabase
/// </summary>
public class DialogueAssetReader : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] bool EnableDebugBodyToggle = false;
    [SerializeField] float ffModeSpeed = 2;
    [Header("Dialogue Effects")]
    [SerializeField] string protagFirstName = "";
    [SerializeField] BoolWithEvent InMenu = null;
    public CharacterDatabase database = null;
    public bool trigger = false; //trigger unlocks an event when it's finished
    [Header("Dialogue UI")]
    [SerializeField] DialogueIcon diaMouseIcon = null;
    //[SerializeField] RawImage diaMouseImage = null;
    public DialogueIcon diaAutoIcon = null;
    //[SerializeField] Image diaffIcon = null;

    [Header("Icon Debugging")]
    public bool autoMode = false;
    public bool ffMode = false;

    [Header("CG Icons")]
    [SerializeField] DialogueIcon cgMouseIcon = null;
    //[SerializeField] RawImage cgMouseImage = null;
    public DialogueIcon cgAutoIcon = null;
    //[SerializeField] Image cgffIcon = null;

    DialogueIcon mouseIcon = null;
    //RawImage mouseImage = null;
    [HideInInspector] public DialogueIcon autoIcon = null;
    //Image ffIcon = null;

    [Header("Nameplate")]
    [SerializeField] RawImage dialogueNamePlate = null;
    [SerializeField] RawImage cgNamePlate = null;
    public Texture2D transparentImage = null;
    [HideInInspector] public RawImage namePlate = null;

    public static DialogueAssetReader instance = null;

    [Header("Choice and FTE")]
    [SerializeField] MenuGroup ChoiceGroup = null;
    [SerializeField] AudioClip choiceConfirm;
    [SerializeField] List<TextMeshProUGUI> ChoiceTextObjects = new List<TextMeshProUGUI>();
    [SerializeField] List<Selectable> ChoiceOptions = new List<Selectable>();

    public delegate void Choice();
    public Choice choice1;
    public Choice choice2;

    //GameObject currentCharObject;
    Character currentChar;

    public Actor currentActor;
    public Actor lastActor;
    public bool protagSpeaking = false;

    public delegate void VoidDel();
    public static event VoidDel OnDialogueStart;
    public static event VoidDel OnDialogueEnd;
    public static event VoidDel OnLineEndEvent;

    public static event System.EventHandler OnLineStart;
    public static event System.EventHandler OnLineEnd;
    public static event System.EventHandler OnDialogueEndEvent;
    public static bool OnLastLine = false;
    public static void CallStart() => OnDialogueStart?.Invoke();
    public static void CallEnd() => OnDialogueEnd?.Invoke();
    public static void CallEndEvent() => OnDialogueEndEvent?.Invoke(null, null);
    public static ArrayList delegates = new ArrayList();


    public void RemoveAllEvents()
    {
        foreach (System.EventHandler eh in delegates)
        {
            OnLineStart -= eh;
            OnLineEnd -= eh;
            OnDialogueEndEvent -= eh;
        }
        delegates.Clear();
    }
#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        foreach (TextMeshProUGUI text in ChoiceTextObjects)
        {
            text.text = "";
        }

        namePlate = dialogueNamePlate;
        mouseIcon = diaMouseIcon;
        //mouseImage = diaMouseImage;
        autoIcon = diaAutoIcon;
        //ffIcon = diaffIcon;
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
        DontDestroyOnLoad(this);
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
        UIHandler.ToTitle -= ResetReader;
        //OnDialogueEnd -= ResetReader;
    }
    private void Start()
    {
        UIHandler.ToTitle += ResetReader;
        //OnDialogueEnd += ResetReader;
        DialogueEventSystem.StartListening("ChangeItemSelect", ChangeItemSelect);
        if (EnableDebugBodyToggle)
            _controls.Player.Crouch.started += DialogueAnimConfig.instance.ToggleDebugBody;
        //Application.targetFrameRate = 30;
    }
    void ResetReader() // ToTitle Function
    {
        SoundManager.instance.StopEnv();
        RemoveAllEvents();
        cantToggle = false;
        inLeaving = false;
        OnLastLine = false;
        OnLoadCG = false;
        OnLoadVid = false;
        _controls.UI.FastForward.started -= FFModeOn;
        _controls.UI.FastForward.canceled -= FFModeOff;
    }
    private void Update()
    {
        if (DialogueAnimConfig.instance.inDialogue.Value
            && _controls.UI.Auto.triggered)
        {
            ToggleAutoMode();
        }
    }
    public void SetNameTransparent()
    {
        if (transparentImage != null)
            namePlate.texture = transparentImage;
    }
    public void EnableCGNameplate(bool to)
    {
        cgNamePlate.enabled = to;
    }
    #region Icons
    public void EnableIcons(bool to)
    {
        //mouseImage.enabled = to;
        autoIcon.enabled = to;
        //ffIcon.enabled = to;
    }
    public void SwapIcons(bool to) // For swapping Icons going to and from CG Mode
    {
        if (diaMouseIcon == null || diaAutoIcon == null || cgAutoIcon == null || cgMouseIcon == null)
            return;
        mouseIcon = to ? cgMouseIcon : diaMouseIcon;
        //mouseImage = mouseImage == diaMouseImage ? cgMouseImage : diaMouseImage;
        autoIcon = to ? cgAutoIcon : diaAutoIcon;
        //ffIcon = ffIcon == diaffIcon ? cgffIcon : diaffIcon;
        if (autoMode)
        {
            mouseIcon.TurnOff();
            autoIcon.TurnOn();
        }
        else
        {
            //Debug.LogWarning("Swap Icons turned off");
            autoIcon.TurnOff();
        }
        /*
        else if (ffMode)
        {
            mouseIcon.enabled = false;
            if (ffIcon.color.a == 0)
            {
                mouseImage.DOFade(0, 0.5f);
                ffIcon.DOFade(1, 0.5f);
            }
        }
        */
    }
    public void SwapNamePlate(bool to)
    {
        namePlate = to ? cgNamePlate : dialogueNamePlate;
        /*
        if(namePlate == cgNamePlate)
        {
            Debug.Log("Set To CG");
        }
        else
        {
            Debug.Log("Set To Dialogue");
        }
        */
    }
    public bool cantToggle = false;
    void ToggleAutoMode()
    {
        if (inLeaving || cantToggle)
            return;
        autoMode = !autoMode;
        if (mouseIcon != null && autoIcon != null)
        {
            if (autoMode)
            {
                mouseIcon.TurnOff();
                autoIcon.TurnOn();
            }
            else
            {
                //Debug.LogWarning("Toggled AutoMode");
                autoIcon.TurnOff();
                //mouseIcon.TurnOn();
            }
        }
    }
    void FFModeOn(CallbackContext context)
    {

        if (InMenu.Value || DialogueAnimConfig.instance.inInstant)
            return;
        //Debug.Log("FFModeOn Called");
        ffMode = true;
        GameManager.instance.cantBeInMenu = true;
        Time.timeScale = ffModeSpeed;
        /*mouseIcon.enabled = false;
        if (ffIcon.color.a == 0)
        {
            mouseImage.DOFade(0, 0.5f);
            ffIcon.DOFade(1, 0.5f);
        }*/
    }
    public void FFModeOff(CallbackContext context)
    {
        //Debug.Log("FFModeOff Called");
        ffMode = false;
        Time.timeScale = 1;
        if (InMenu.Value || DialogueAnimConfig.instance.inInstant)
            return;

        /*if (!autoMode)
        {
            mouseIcon.enabled = true;
            mouseImage.DOFade(1, 0.5f);
        }
        ffIcon.DOFade(0, 0.5f);*/
    }
    #endregion

    public static string SaveDialogueName = "";
    public static int SaveDirectToNum = 0;
    public static List<bool> SaveCondition = new List<bool>();
    public static int SaveLineNum = 0;
    public static int SaveTransitionNum = 0;
    public static Backlog Backlog = new Backlog();
    public static void ClearSaveData()
    {
        SaveDialogueName = "";
        SaveDirectToNum = 0;
        SaveCondition.Clear();
        SaveLineNum = 0;
        SaveTransitionNum = 0;
    }
    public bool inLeaving = false;
    public void DialoguePlay(Dialogue[] dial, bool inleave = false) => StartCoroutine(PlayDialogue(dial, inleave));
    public IEnumerator PlayDialogue(Dialogue[] dial, bool inleave = false)
    {
        inLeaving = inleave;

        //Debug.Log("FFMode before is: " + ffMode);
        _controls.UI.FastForward.started += FFModeOn;
        _controls.UI.FastForward.canceled += FFModeOff;
        //Debug.Log("FFMode after is: " + ffMode);

        SaveLineNum = 0;

        foreach (Dialogue currentDialogue in dial)
        {

            int count = 0; // This is for FTE Stuff

            for (int i = 0; i < currentDialogue.Lines.Count; i++)
            {
                Line currentLine = currentDialogue.Lines[i];
                OnLineStart?.Invoke(null, null);
                //Debug.Log("Started Line: " + i);

                #region Loading File
                if (GameSaver.LoadingFile)
                {
                    if (GameSaver.CurrentGameData.DialogueData.LineNum != 0)
                    {
                        //yield return new WaitForSeconds(Time.unscaledDeltaTime);
                        i = LoadToLine(currentDialogue);

                    }
                    if (OnLoadCG)
                    {
                        Debug.LogWarning("Passed CG Zone");
                        LastCG.TriggerDialogueEvent();
                        CGPlayer.cgDone = false;
                        yield return new WaitUntil(() => CGPlayer.cgDone);
                        CGPlayer.cgDone = false;
                        //Debug.LogWarning("Ended CG Zone");
                    }

                    if (OnLoadVid && LastVid != null)
                    {
                        Debug.LogWarning("Passed Vid Zone");
                        //Debug.LogWarning("Last Vid: " + (LastVid != null));
                        //Debug.LogWarning(" OnLoadVid: " + OnLoadVid);
                        LastVid.TriggerDialogueEvent();
                        CGPlayer.vidDone = false;
                        yield return new WaitUntil(() => CGPlayer.vidDone);
                        CGPlayer.vidDone = false;
                    }

                    if (DialogueAnimConfig.instance.protagUIIsShown)
                        DialogueAnimConfig.instance.ShowProtag();
                    else
                        DialogueAnimConfig.instance.HideProtag();
                    Debug.Log("called line: " + i);
                    currentLine = currentDialogue.Lines[i];
                    SaveDirectToNum = GameSaver.CurrentGameData.DialogueData.DirectToNum;
                    SaveLineNum = GameSaver.CurrentGameData.DialogueData.LineNum;
                }
                #endregion
                //Debug.Log("REACHED START OF VIDS AND CGS");
                OnLastLine = i == currentDialogue.Lines.Count - 1;//&& !DialogueData.HasInnerDialogue(currentDialogue);
                if (currentLine.StopEnv)
                {
                    Debug.LogWarning("Stop Env Called");
                    SoundManager.instance.StopEnv();
                }
                #region CG and Blur
                var cg = currentLine.DiaEvents.Where(n => n.GetType() == typeof(CGDisplay));
                if (cg.Count() > 0)
                {
                    CGPlayer.cgDone = false;
                    cg.ElementAt(0).TriggerDialogueEvent();
                    yield return new WaitUntil(() => CGPlayer.cgDone);
                    CGPlayer.cgDone = false;
                }
                var vid = currentLine.DiaEvents.Where(n => n.GetType() == typeof(VideoDisplay));
                if (vid.Count() > 0)
                {
                    VideoDisplay display = (VideoDisplay)vid.ElementAt(0);
                    bool b = display.SVValue.mainClip == null && !display.SVValue.playOnly;
                    if (b && GameSaver.LoadingFile)
                    {

                    }
                    else
                    {
                        vid.ElementAt(0).TriggerDialogueEvent();
                        CGPlayer.vidDone = false;
                        yield return new WaitUntil(() => CGPlayer.vidDone);
                        CGPlayer.vidDone = false;
                        VideoDisplay video = (VideoDisplay)vid.ElementAt(0);
                        if (video.SVValue.playOnly)
                        {
                            Debug.Log("Called Video Play Only");
                            SaveLineNum++;
                            continue;
                        }
                    }

                }
                //Debug.Log("REACHED END OF VIDS AND CGS");
                if (currentLine.BlurVision)
                    DialogueAnimConfig.instance.BlurVision();

                var flashback = currentLine.DiaEvents.Where(n => n.GetType() == typeof(Flashback));
                if (flashback.Count() > 0)
                {
                    CGPlayer.cgDone = false;
                    flashback.ElementAt(0).TriggerDialogueEvent();
                    yield return new WaitUntil(() => CGPlayer.cgDone);
                    CGPlayer.cgDone = false;
                }
                #endregion

                var intro = currentLine.DiaEvents.Where(n => n.GetType() == typeof(CharIntro));
                if (intro.Count() > 0)
                {
                    trigger = false;
                    intro.ElementAt(0).TriggerDialogueEvent();
                    yield return new WaitUntil(() => trigger);
                    Debug.Log("Ended Char Intro");
                }

                #region Dialogue Line Events
                if (currentLine.DiaEvents.Count != 0)
                    DialogueEvents(currentLine);



                if (currentLine.Events.Count != 0 && !GameSaver.LoadingFile) // Event Caller from Dialogue Line
                {
                    foreach (SceneEvent se in currentLine.Events.ToArray())
                    {
                        se.Raise();

                        while (trigger == false) // Will wait until trigger is true from Event Ending
                        {
                            yield return null;
                        }
                        trigger = false;
                    }
                }

                #endregion


                if (currentLine.StopSFX)
                    SoundManager.instance.StopSoundFX();

                GetLineInfo(currentLine);

                currentActor = DialogueAnimConfig.instance.FindActor(currentChar.FirstName);

                #region Camera Animations
                var leaving = currentLine.DiaEvents.Where(n => n.GetType() == typeof(CharacterLeave));
                if (leaving.Count() > 0) // Character leave
                {
                    foreach (IDialogueEvent e in leaving)
                    {
                        e.TriggerDialogueEvent();
                        CharacterLeave c = (CharacterLeave)e;
                        //if c.data.exit

                    }
                }
                if (currentLine.PanToChar)
                {
                    Actor actorPan = DialogueAnimConfig.instance.FindActor(currentLine.CharToPan.FirstName);
                    if (!actorPan)
                    {
                        Debug.LogWarning("NOTIFY: PAN TO CHARACTER ACTOR WAS NOT FOUND IN THE SCENE");
                    }
                    else
                    {
                        Debug.LogWarning("NOTIFY: PAN TO CHARACTER ACTOR IS: " + actorPan.displayName);
                    }
                    DialogueAnimConfig.instance.ChangeFocus(actorPan, lastActor);
                    lastActor = actorPan;
                    yield return new WaitForSeconds(DialogueAnimConfig.instance.characterPanTime);
                }
                else if (!protagSpeaking && currentActor != null && lastActor != currentActor && !currentLine.DontPan
                    && !currentLine.Leave && !CGPlayer.inVid && !CGPlayer.inCG) //Look at the current speaking character
                {
                    //Debug.LogWarning("ChangeFocus: " + currentActor.displayName + ", " + lastActor);
                    if (GameSaver.LoadingFile)
                        yield return new WaitForSeconds(Time.unscaledDeltaTime);
                    /* The Above had to be added because for some reason loading on the line to kai before the
                     * tree room shakes : it'd put the camera on the wrong angle
                     * D_Ch0_Inv_ToTrial_AfterStatue Line before tree vid
                     */
                    DialogueAnimConfig.instance.ChangeFocus(currentActor, lastActor);
                    yield return new WaitForSeconds(Time.unscaledDeltaTime);

                    if (ffMode)
                        yield return new WaitForSeconds(Time.unscaledDeltaTime * 5);

                }
                var shakes = currentLine.DiaEvents.Where(n => n.GetType() == typeof(ShakeObject));
                if (shakes.Count() > 0 && !ffMode)
                {
                    if (!currentLine.PanToChar && !protagSpeaking && currentActor != null && lastActor != currentActor && !currentLine.DontPan
                    && !currentLine.Leave && !CGPlayer.inVid && !CGPlayer.inCG)
                        yield return new WaitForSeconds(DialogueAnimConfig.instance.characterPanTime);

                    foreach (IDialogueEvent e in shakes)
                        e.TriggerDialogueEvent();
                }
                #endregion

                #region VFX
                /*if (currentLine.VFXNumber != 0)
                    animationDB.Play(VFXDB.VFXClips[currentLine.VFXNumber - 1].name);*/
                #endregion

                #region Music
                if (currentLine.MusicChange && !GameSaver.LoadingFile)
                {
                    if (currentLine.Music != null)
                        SoundManager.instance.PlayMusic(currentLine.Music);
                    else
                        SoundManager.instance.StopSound();
                }
                #endregion

                #region Voice Line and Sound Effects
                if (currentLine.VoiceSFX != null && !GameSaver.LoadingFile)
                {
                    SoundManager.instance.PlayVoiceLine(currentLine.VoiceSFX);
                }
                if (currentLine.EnvSFX != null && !GameSaver.LoadingFile)
                {
                    SoundManager.instance.PlayEnv(currentLine.EnvSFX);
                }
                if (currentLine.SFX.Count > 0 && !GameSaver.LoadingFile)
                {
                    for (int j = 0; j < currentLine.SFX.Count; j++)
                    {
                        SoundManager.instance.PlaySFX(currentLine.SFX[j]);
                    }
                }
                #endregion

                var faint = currentLine.DiaEvents.Where(n => n.GetType() == typeof(FaintAnim));
                if (faint.Count() > 0)
                {
                    faint.ElementAt(0).TriggerDialogueEvent();
                }

                #region Changing Sprites and Nameplate
                SetNameplate(currentLine);


                if (protagSpeaking && currentLine.Expression != null && currentLine.Expression.Sprite != null)
                    DialogueAnimConfig.instance.UISpriteChange(currentLine, protagSpeaking);
                if (protagSpeaking && currentLine.DiaEvents.Where(n => n.GetType() == typeof(LastActorSprite)).Count() > 0
                    && !GameSaver.LoadingFile)
                {
                    if (ffMode)
                    {
                        yield return new WaitUntil(() => DialogueAnimConfig.SpriteChanged == true);
                    }
                    else
                    {
                        yield return new WaitForSeconds(DialogueAnimConfig.instance.spriteChangeTime * 2 + 0.1f);
                    }

                    DialogueAnimConfig.SpriteChanged = false;
                }
                if (!protagSpeaking && currentLine.Expression != null && currentLine.Expression.Sprite != null)
                {
                    DialogueAnimConfig.instance.WorldSpriteChange(currentLine, currentActor);
                    if (ffMode)
                    {
                        yield return new WaitUntil(() => DialogueAnimConfig.SpriteChanged == true);
                    }
                    else
                    {
                        yield return new WaitForSeconds(DialogueAnimConfig.instance.spriteChangeTime * 2 + 0.1f);
                    }

                    DialogueAnimConfig.SpriteChanged = false;
                }
                #endregion

                #region Dialogue Animations
                if (currentLine.Leave)
                {
                    DialogueAnimConfig.instance.ActorLeft(lastActor, false);
                    //yield return new WaitForSeconds(1);
                }

                if (currentLine.FlashWhite)
                    DialogueAnimConfig.instance.FlashWhite();



                #endregion


                #region Displaying Text
                if (currentLine.Text == "")
                {
                    if (!ffMode && currentLine.AutomaticLine)
                    {
                        yield return new WaitForSeconds(currentLine.TimeToNextLine);
                    }
                    Debug.LogWarning("CurrentLine Text didn't have text");
                    if (!GameSaver.LoadingFile)
                    {
                        SaveLineNum++;
                        continue;
                    }
                }
                else // Normal Display Text
                {
                    //if(cantToggle)
                    //cantToggle = false;
                    if (CGPlayer.inVid)
                    {
                        CGPlayer.instance.FadeBox(true);
                    }
                    if (cgAutoIcon != null && !cgAutoIcon.isActiveAndEnabled)
                    {
                        cgAutoIcon.gameObject.SetActive(true);
                    }
                    //Debug.LogWarning("Displaying Text Pray");
                    DialogueTextConfig.instance.DisplayText(currentLine);
                    DialogueTextConfig.instance.isWaitingForClick = true;
                }
                if (autoIcon != null && !autoIcon.gameObject.activeSelf && !inLeaving)
                {
                    autoIcon.gameObject.SetActive(true);
                }
                while (DialogueTextConfig.instance.isWaitingForClick)
                {
                    if (ffMode)
                    {
                        break;
                    }
                    yield return null;
                }
                while (DialogueAnimConfig.instance.isFainting)
                {
                    yield return null;
                }
                #endregion

                if (GameSaver.LoadingFile)
                {
                    Debug.LogWarning("Reached Finished Loading Dialogue");
                    DialogueLoaded = true;
                    yield return new WaitUntil(() => !GameSaver.LoadingFile);
                    DialogueLoaded = false;
                    ffMode = false;
                    AudioListener.volume = 1;
                }
                if (!ffMode && DiaCamEvents.t != null && DiaCamEvents.t.IsActive() && DiaCamEvents.t.IsPlaying())
                {
                    while (DiaCamEvents.t.IsPlaying())
                    {
                        yield return null;
                    }
                }
                if (!DialogueAnimConfig.instance.inInstant && !ffMode)
                {
                    GameManager.instance.cantBeInMenu = false;
                }
                if (DialogueAnimConfig.fading)
                {
                    yield return new WaitUntil(() => !DialogueAnimConfig.fading);
                }

                #region Wait for Button Click

                yield return new WaitForSeconds(Time.deltaTime);

                //mouseIcon.SetBool("canClick", true);
                if (!inLeaving && !autoMode && mouseIcon != null && !mouseIcon.on)
                {
                    mouseIcon.TurnOn();
                }

                if (count == currentDialogue.Lines.Count - 1 && currentDialogue.Choices.Count != 0) // If has Choices
                {
                    yield return new WaitForSeconds(0.3f);
                    ConfigureChoiceText(currentDialogue, currentChar);
                    GameObject lastSelectedGO = null;
                    yield return new WaitForSeconds(0.3f);
                    // TO-DO: Put a wait until ConfigureChoiceText Function is finished for now putting wait for seconds hang time
                    while (!_controls.UI.Submit.triggered)
                    {
                        if (EventSystem.current.currentSelectedGameObject == null)
                            EventSystem.current.SetSelectedGameObject(lastSelectedGO);
                        else
                            lastSelectedGO = EventSystem.current.currentSelectedGameObject;

                        if (_controls.UI.Submit.triggered)
                            break;

                        yield return null;
                    }
                    Choose(EventSystem.current.currentSelectedGameObject, currentChar);
                    Debug.Log("A choice was chosen");
                }
                else // Regular Dialogue
                {
                    while (!_controls.UI.Submit.triggered)
                    {
                        while (InMenu.Value)
                        {
                            yield return null;
                        }
                        if (autoMode)
                        {
                            goNext = false;
                            StartCoroutine(WaitForAuto());
                            while (!goNext)
                            {
                                if (_controls.UI.Submit.triggered && !InMenu.Value)
                                    break;

                                if (ffMode && !InMenu.Value)
                                {
                                    //yield return new WaitForSeconds(Time.deltaTime);
                                    break;
                                }
                                if (!autoMode)
                                {
                                    if (mouseIcon != null)
                                    {
                                        mouseIcon.TurnOn();
                                    }
                                    break;
                                }
                                yield return null;
                            }
                            if (autoMode)
                                break;
                        }

                        if (ffMode)
                        {
                            //yield return new WaitForSeconds(Time.deltaTime);
                            //DialogueTextConfig.instance.ClearText();
                            break;
                        }

                        if (_controls.UI.Submit.triggered)
                            break;

                        yield return null;
                    }
                    while (InMenu.Value)
                    {
                        yield return null;
                    }
                    if (!ffMode)
                        SoundManager.instance.PlayNext();
                }
                //mouseIcon.SetBool("canClick", false);
                SoundManager.instance.StopVoiceLine();
                if (!autoMode && mouseIcon != null)
                {
                    mouseIcon.TurnOff();
                }

                DialogueAssetReader.Backlog.AddLine(currentChar, DialogueTextConfig.instance.GetBox().text
                    , currentLine.AliasNumber - 1, DialogueTextConfig.instance.GetBox().color); // So Audio only shows from the trial rn
                DialogueTextConfig.instance.ClearText();
                ClearChoiceText();
                #endregion

                var tbGet = currentLine.DiaEvents.Where(n => n.GetType() == typeof(TruthBulletGet));
                if (tbGet.Count() > 0)
                {
                    DialogueEventSystem.TriggerEvent("TruthBulletGetEnd"); // To fade out the Truth Bullet Image
                }

                GameManager.instance.cantBeInMenu = true;

                if (ffMode)
                    yield return new WaitForSeconds(0.185f);

                count += 1;
                if (!protagSpeaking && !currentLine.DontPan)
                    lastActor = currentActor;

                SaveLineNum += 1;
                OnLineEndEvent?.Invoke();
            }

            #region End Dialogue Options
            if (currentDialogue.UnlockPause)
                PlayerInfo.instance.Info.pauseAccess = true;
            if (currentDialogue.ClearLock)
            {
                //GateDatabase gdb = (GateDatabase)Resources.Load("Gates/GateDatabase");
                //ProgressionManager.instance.ClearLockedDialogue();
                //ProgressionManager.instance.GlobalLock = null;
                Door.InvokeGLL();
            }
            if (currentDialogue.FlagTrigger.Enabled)
            {
                TriggerFlag t = currentDialogue.FlagTrigger;
                ProgressionManager.instance.TriggerFlag(t.Chapter, t.Objective, t.Flags);
            }
            if (currentDialogue.Variable.Enabled) // Conditions
            {
                Dialogue[] d = new Dialogue[1];
                Variable v = currentDialogue.Variable;
                if ((v.BoolVariable != null && v.BoolVariable.Resolve()) ||
                    ProgressionManager.instance.EvaluateVariable(v.Chapter, v.Objective, v.Flags))
                {
                    d[0] = currentDialogue.Variable.NextDialogueTrue;
                    SaveCondition.Add(true);
                }
                else if (v.NextDialogueFalse != null)
                {
                    d[0] = currentDialogue.Variable.NextDialogueFalse;
                    SaveCondition.Add(false);
                }

                if (d[0] != null)
                {
                    _controls.UI.FastForward.started -= FFModeOn;
                    _controls.UI.FastForward.canceled -= FFModeOff;
                    StartCoroutine(PlayDialogue(d));
                    yield break;
                }
            }

            if (currentDialogue.SceneTransition.Enabled) // SCENE TRANSITION
            {
                if (currentDialogue.SceneTransition.ToDark)
                {
                    GlobalFade.instance.FadeTo(0.5f);
                    yield return new WaitForSeconds(0.5f);
                }
                if (currentDialogue.SceneTransition.OnLoadNoDark)
                    RoomLoader.OnLoad += DummyFadeOut;
                if (!currentDialogue.SceneTransition.ToMenu)
                    RoomLoader.PreEndLoad += ToTPFDRoom; // Handles if the area is TPFD

                if (!currentDialogue.SceneTransition.AtEnd && !currentDialogue.SceneTransition.ToMenu)
                {
                    AsyncOperation a = SceneManager.LoadSceneAsync(currentDialogue.SceneTransition.Scene);
                    a.allowSceneActivation = false;
                    yield return new WaitUntil(() => a.progress >= 0.9f);
                    a.allowSceneActivation = true;
                    yield return new WaitUntil(() => !RoomLoader.instance.inLoading.Value);
                }
                else if (currentDialogue.SceneTransition.AtEnd)
                    DialogueAnimConfig.MoveAtEnd = currentDialogue.SceneTransition.AtEnd;

                if (currentDialogue.SceneTransition.ToMenu)
                {
                    CleanUp();
                    ReturnToMenu(currentDialogue.SceneTransition.Scene);
                    yield break;
                }
            }

            if (currentDialogue.DirectTo != null && currentDialogue.DirectTo.Enabled)
            {
                _controls.UI.FastForward.started -= FFModeOn;
                _controls.UI.FastForward.canceled -= FFModeOff;
                Dialogue[] c = new Dialogue[1];
                c[0] = currentDialogue.DirectTo.NewDialogue;
                StartCoroutine(PlayDialogue(c));

                SaveDirectToNum += 1;
                if (currentDialogue.SceneTransition.Enabled && !currentDialogue.SceneTransition.AtEnd)
                {
                    SaveTransitionNum = SaveDirectToNum;
                }
                yield break;
            }

            if (currentDialogue.EndVidEnabled)
            {
                CGPlayer.instance.PlayEndVideo(currentDialogue.EndVideo);
                yield return new WaitUntil(() => CGPlayer.inEndVideo);
                DialogueAnimConfig.instance.StopDialogue();
                InMenu.Value = true;
                yield return new WaitUntil(() => !CGPlayer.inEndVideo);
                InMenu.Value = false;
                yield break;
            }

            if (currentDialogue.UnlockPauseOption)
                PlayerInfo.instance.Info.pauseOptions[currentDialogue.PauseOptionNum] = true;

            if (currentDialogue.StartInvestigation.Enabled)
            {
                CleanUp();
                DialogueAnimConfig.instance.StartInvestigation(currentDialogue.StartInvestigation);
                yield break;
            }
            #endregion

            //Debug.Log("Made it to here");
            OnLineEnd?.Invoke(null, null);

        }
        //Debug.Log("Dialogue should stop");

        CleanUp();
        DialogueAnimConfig.instance.StopDialogue();
        yield break;
    }
    void DummyFadeOut()
    {
        RoomLoader.OnLoad -= DummyFadeOut;
        Debug.LogWarning("DummyFade out Called");
        GlobalFade.instance.FadeOut(0.5f);
    }
    void CleanUp()
    {
        if (ffMode)
            FFModeOff(new CallbackContext());
        _controls.UI.FastForward.started -= FFModeOn;
        _controls.UI.FastForward.canceled -= FFModeOff;

        ClearSaveData();
    }
    public void SetNameplate(Line currentLine)
    {
        if (currentLine.AliasNumber != 0)
        {
            namePlate.texture = currentChar.Aliases[currentLine.AliasNumber - 1].Nameplate;
        }
        else if (currentLine.Speaker != null && currentChar.Nameplate != null)
            namePlate.texture = currentChar.Nameplate;

        if (!currentLine.Speaker || currentLine.Speaker.FirstName.Contains("Tutorial"))
            SetNameTransparent();
    }
    /// <summary>
    /// For when going to another TPFD Scene in the middle of dialogue
    /// </summary>
    void ToTPFDRoom()
    {
        RoomLoader.PreEndLoad -= ToTPFDRoom;
        if (!(Door.CurrentArea != null))
            return;
        if (Door.CurrentArea.isTPFD && GameManager.instance.currentMode != GameManager.Mode.TPFD)
            GameManager.instance.ChangeMode(GameManager.Mode.TPFD);
        if (GameManager.instance.currentMode != GameManager.Mode.TPFD)
            return;
        DialogueAnimConfig.instance.SetCamTPFD();
    }
    bool goNext = false;
    IEnumerator WaitForAuto()
    {
        float waitTime = DialogueTextConfig.instance.GetBox().text.Length * 0.05f;
        waitTime = waitTime > 3 ? 3 : waitTime;
        yield return new WaitForSeconds(3);
        if (SoundManager.instance.VoicePlaying())
        {
            while (SoundManager.instance.VoicePlaying())
            {
                yield return null;
            }
        }
        if (autoMode)
            goNext = true;
        yield return null;
    }
    public static void ReturnToMenu(string TitleScreenSceneName) => instance.StartCoroutine(FadeToMenu(TitleScreenSceneName));
    static IEnumerator FadeToMenu(string TitleScreenSceneName)
    {
        MenuGroup.CanSelect = false;
        EventSystem.current.SetSelectedGameObject(null);

        GlobalFade.instance.FadeTo(0.3f);
        yield return new WaitForSecondsRealtime(0.3f);

        UIHandler.CallToTitle();
        EventSystem.current.SetSelectedGameObject(null);
        GameSaver.FirstTimeLoaded = true;
        RoomLoader.instance.RoomsCannotLoad();
        //ProgressionManager.instance.ClearLockedDialogue();

        DialogueAssetReader.instance.StopAllCoroutines();
        DialogueAnimConfig.instance.DisableVisuals();
        DialogueAnimConfig.instance.mainCanvas.enabled = false;
        DialogueAnimConfig.instance.ShowMainUI(false);
        DialogueTextConfig.instance.ClearText();
        MenuGroup.CanSelect = true;
        SceneManager.LoadSceneAsync(TitleScreenSceneName);
        yield break;
    }

    #region Load Dialogue
    public static bool DialogueLoaded = false;
    public static bool LoadThroughNLine = false;
    private bool OnLoadCG = false;
    private CGDisplay LastCG;
    private bool OnLoadVid = false;
    private VideoDisplay LastVid;
    public static void LoadDialogue()
    {
        SaveDialogueName = GameSaver.CurrentGameData.DialogueData.DialogueName;
        SaveDirectToNum = GameSaver.CurrentGameData.DialogueData.DirectToNum;
        SaveLineNum = GameSaver.CurrentGameData.DialogueData.LineNum;
        Backlog.Load(GameSaver.CurrentGameData.BackData);
        Dialogue[] load = new Dialogue[1];
        load[0] = GameSaver.LoadDialogue;
        //Debug.LogWarning("StartDialogue from RoomLoader");
        Actor find = DialogueAnimConfig.instance.FindActor(load[0].Lines[0].Speaker.FirstName);
        Actor actor = find != null ? find : GameSaver.LoadActor;
        if (actor == GameSaver.LoadActor)
        {
            //Debug.LogWarning("LOAD ACTOR WAS USED: ");
        }
        DialogueAnimConfig.instance.StartDialogue(actor, load);
        GameSaver.LoadActor = null;
        // The above may not be ideal in terms of finding the right actor if the first lines character is the protag
    }
    public void LoadThrough(Dialogue dia, int directNum, int conditionNum)
    {
        LoadThroughNLine = true;
        SaveDirectToNum = GameSaver.CurrentGameData.DialogueData.DirectToNum;
        SaveCondition = GameSaver.CurrentGameData.DialogueData.Conditional;
        SaveLineNum = GameSaver.CurrentGameData.DialogueData.LineNum;
        SaveTransitionNum = GameSaver.CurrentGameData.DialogueData.TransitionNum;
        Debug.Log("SaveLine Num is: " + SaveLineNum);
        ffMode = true;
        if (directNum == SaveDirectToNum && conditionNum == SaveCondition.Count) // Check point to when we reach the correct dialogue asset
        {
            Dialogue[] t = new Dialogue[1];
            t[0] = dia;
            Debug.Log("LoadThrough");
            DialoguePlay(t);
            return;
        }
        for (int i = 0; i < dia.Lines.Count; i++)
        {
            Line currentLine = dia.Lines[i];
            GetLineInfo(currentLine);

            currentActor = DialogueAnimConfig.instance.FindActor(currentChar.FirstName);
            if (SaveTransitionNum > 0)
            {
                if (SaveDirectToNum > 0 && directNum >= SaveTransitionNum)
                    DialogueEvents(currentLine);
            }
            else
            {
                DialogueEvents(currentLine);
            }
            //SLCam(currentLine);
            var cg = currentLine.DiaEvents.Where(n => n.GetType() == typeof(CGDisplay));
            if (cg.Count() > 0)
            {
                CGDisplay display = (CGDisplay)cg.ElementAt(0);
                OnLoadCG = display.SCGValue.CG != null || display.SCGValue.prefab != null;
                if (OnLoadCG)
                    LastCG = display;
                else
                    LastCG = null;

            }
            var vid = currentLine.DiaEvents.Where(n => n.GetType() == typeof(VideoDisplay));
            if (vid.Count() > 0)
            {
                VideoDisplay display = (VideoDisplay)vid.ElementAt(0);
                OnLoadVid = display.SVValue.mainClip != null && !display.SVValue.playOnly;
                if (OnLoadVid)
                    LastVid = display;
                else
                {
                    //Debug.LogWarning("To Null");
                    LastVid = null;
                }

            }
            var item = currentLine.DiaEvents.Where(n => n.GetType() == typeof(ShowItem));
            if (item.Count() > 0)
            {
                ShowItem showItem = (ShowItem)item.ElementAt(0);
                if (showItem.Image == null)
                {
                    showItem.TriggerDialogueEvent();
                }
            }
            if (SaveTransitionNum == 0)
            {
                var leave = currentLine.DiaEvents.Where(n => n.GetType() == typeof(CharacterLeave));
                if (leave.Count() > 0)
                {
                    leave.ElementAt(0).TriggerDialogueEvent();
                }
            }
            if (currentChar.Nameplate != null)
                namePlate.texture = currentChar.Nameplate;

            if (protagSpeaking)
            {
                DialogueAnimConfig.instance.UISpriteChange(currentLine, protagSpeaking);
            }

            if (currentLine.PanToChar)
            {
                Actor actorPan = DialogueAnimConfig.instance.FindActor(currentLine.CharToPan.FirstName);
                if (!actorPan)
                {
                    Debug.LogWarning("NOTIFY: PAN TO CHARACTER ACTOR WAS NOT FOUND IN THE SCENE");
                }
                else
                {
                    Debug.LogWarning("NOTIFY: PAN TO CHARACTER ACTOR IS: " + actorPan.displayName);
                }
                DialogueAnimConfig.instance.ChangeFocus(actorPan, lastActor);
                lastActor = actorPan;
            }
            else if (currentActor != null)
            {
                DialogueAnimConfig.instance.ChangeFocus(currentActor, lastActor);
            }

            if (currentLine.Text.Contains("^"))
            {
                if (!DialogueAnimConfig.instance.protagUIIsShown)
                    DialogueAnimConfig.instance.protagUIIsShown = true;
                else
                    DialogueAnimConfig.instance.protagUIIsShown = false;
            }
            /*if (!protagSpeaking && currentLine.Expression.Sprite != null)
            {
                DialogueAnimConfig.instance.WorldSpriteChange(currentLine, currentActor);


                DialogueAnimConfig.SpriteChanged = false;
            }*/

            if (!protagSpeaking && !currentLine.DontPan)
                lastActor = currentActor;


        }
        if (directNum != SaveDirectToNum)
        {
            if (dia.DirectTo.NewDialogue != null)
                LoadThrough(dia.DirectTo.NewDialogue, directNum + 1, conditionNum);
            else if (conditionNum != SaveCondition.Count) // Pretty sure this can't be reached but putting it here to cover
            {
                if (SaveCondition[conditionNum])
                    LoadThrough(dia.Variable.NextDialogueTrue, directNum, conditionNum + 1);
                else
                    LoadThrough(dia.Variable.NextDialogueFalse, directNum, conditionNum + 1);
            }
        }
        else if (SaveCondition.Count > 0 && conditionNum != SaveCondition.Count)
        {
            // Static list of bools , detailing the track in order, written during gameplay 
            // Here is where I'd go through the list of bools 
            // Call Load through again with a different variable called conditionNum
            if (SaveCondition[conditionNum])
                LoadThrough(dia.Variable.NextDialogueTrue, directNum, conditionNum + 1);
            else
                LoadThrough(dia.Variable.NextDialogueFalse, directNum, conditionNum + 1);
        }
        else
        {
            Dialogue[] t = new Dialogue[1];
            Debug.Log("Playing Direct To of: " + dia.name);
            t[0] = dia.DirectTo.NewDialogue;
            DialoguePlay(t);
        }
    }

    int LoadToLine(Dialogue dia)
    {
        Debug.Log("Load to Line Started");
        for (int i = 0; i < dia.Lines.Count; i++)
        {
            Line currentLine = dia.Lines[i];
            if (i == GameSaver.CurrentGameData.DialogueData.LineNum)
            {
                Debug.Log("Loaded to line: " + i);
                LoadThroughNLine = false;
                ffMode = false;


                var vids = currentLine.DiaEvents.Where(n => n.GetType() == typeof(VideoDisplay));
                if (vids.Count() > 0)
                {
                    VideoDisplay display = (VideoDisplay)vids.ElementAt(0);
                    OnLoadVid = display.SVValue.mainClip != null && !display.SVValue.playOnly;
                    LastVid = null;
                    if (!OnLoadVid)
                    {
                        //Debug.LogWarning("To Null");

                    }

                }
                var cgs = currentLine.DiaEvents.Where(n => n.GetType() == typeof(CGDisplay));
                if (cgs.Count() > 0)
                {
                    CGDisplay display = (CGDisplay)cgs.ElementAt(0);
                    bool localOnLoadCG = display.SCGValue.CG != null || display.SCGValue.prefab != null;
                    if (localOnLoadCG)
                    {
                        OnLoadVid = false;
                        //Debug.LogWarning("FromCG");
                        LastVid = null;
                    }

                }

                return i;
            }

            GetLineInfo(currentLine);

            currentActor = DialogueAnimConfig.instance.FindActor(currentChar.FirstName);

            DialogueEvents(currentLine);

            //SLCam(currentLine);

            var cg = currentLine.DiaEvents.Where(n => n.GetType() == typeof(CGDisplay));
            if (cg.Count() > 0)
            {
                CGDisplay display = (CGDisplay)cg.ElementAt(0);
                OnLoadCG = display.SCGValue.CG != null || display.SCGValue.prefab != null;
                if (OnLoadCG)
                {
                    LastCG = display;
                    OnLoadVid = false;
                    //Debug.LogWarning("FromCG");
                    LastVid = null;
                }
                else
                    LastCG = null;

            }
            var vid = currentLine.DiaEvents.Where(n => n.GetType() == typeof(VideoDisplay));
            if (vid.Count() > 0)
            {
                VideoDisplay display = (VideoDisplay)vid.ElementAt(0);
                OnLoadVid = display.SVValue.mainClip != null && !display.SVValue.playOnly;
                if (OnLoadVid)
                {
                    LastVid = display;
                    OnLoadCG = false;
                    LastCG = null;
                }
                else
                {
                    //Debug.LogWarning("To Null");
                    LastVid = null;
                }

            }
            var item = currentLine.DiaEvents.Where(n => n.GetType() == typeof(ShowItem));
            if (item.Count() > 0)
            {
                ShowItem showItem = (ShowItem)item.ElementAt(0);
                if (showItem.Image == null)
                {
                    showItem.TriggerDialogueEvent();
                }
            }
            var leave = currentLine.DiaEvents.Where(n => n.GetType() == typeof(CharacterLeave));
            if (leave.Count() > 0)
            {
                leave.ElementAt(0).TriggerDialogueEvent();
            }
            if (currentChar.Nameplate != null)
                namePlate.texture = currentChar.Nameplate;

            if (currentLine.PanToChar)
            {
                Actor actorPan = DialogueAnimConfig.instance.FindActor(currentLine.CharToPan.FirstName);
                if (!actorPan)
                {
                    Debug.LogWarning("NOTIFY: PAN TO CHARACTER ACTOR WAS NOT FOUND IN THE SCENE");
                }
                else
                {
                    Debug.LogWarning("NOTIFY: PAN TO CHARACTER ACTOR IS: " + actorPan.displayName);
                }
                DialogueAnimConfig.instance.ChangeFocus(actorPan, lastActor);
                lastActor = actorPan;
            }
            else if (currentActor != null && lastActor != currentActor && (!OnLoadCG && !OnLoadVid))
            {
                DialogueAnimConfig.instance.ChangeFocus(currentActor, lastActor);
                // Test calling look dialogue on the new actor every time to see if this fix sidways sprite problems?
                //Debug.Log("Calling Change Focus");
            }

            if (protagSpeaking)
            {
                DialogueAnimConfig.instance.UISpriteChange(currentLine, protagSpeaking);
            }
            if (currentLine.Text.Contains("^"))
            {
                if (!DialogueAnimConfig.instance.protagUIIsShown)
                    DialogueAnimConfig.instance.protagUIIsShown = true;
                else
                    DialogueAnimConfig.instance.protagUIIsShown = false;
            }


            if (!protagSpeaking && !currentLine.DontPan)
                lastActor = currentActor;
        }
        LoadThroughNLine = false;
        return 0;
    }
    /*
    void SLCam(Line currentLine)
    {
        var camMove = currentLine.DiaEvents.Where(n => n.GetType() == typeof(CamToPosition));
        if (camMove.Count() > 0)
        {
            CamToPosition c = (CamToPosition)camMove.ElementAt(0);
            CTPTuple data = c.cTPTuple;
            if (!data.keepFocus)
            {
                DialogueEventSystem.TriggerEvent("ToggleBlur", false);
                DialogueEventSystem.TriggerEvent("CheckFocus");
            }
        }
    }
    */
    #endregion

    #region Get Line Information
    public Character FindChar(string n) => database.GetCharacter(n);
    void GetLineInfo(Line currentLine)
    {
        if (currentLine.Speaker == null)
            return;
        if (currentLine.Speaker.FirstName.Equals(protagFirstName) || currentLine.Speaker.FirstName.Equals("Tutorial"))
        {
            protagSpeaking = true;
            currentChar = FindChar(currentLine.Speaker.FirstName);
        }
        else
        {
            protagSpeaking = false;
            currentChar = currentLine.Speaker;
        }
    }
    #endregion

    #region Dialogue Events
    readonly List<Type> exclusionTypes = new List<Type>()
    {
        typeof(ShakeObject), typeof(CGDisplay), typeof(VideoDisplay), typeof(FaintAnim), typeof(CharacterLeave),
        typeof(CharIntro), typeof(Flashback)
    };
    /*
    readonly List<Type> slExclusionTypes = new List<Type>()
    {
        typeof(ShakeObject), typeof(CGDisplay), typeof(VideoDisplay), typeof(FaintAnim), typeof(CharacterLeave),
        typeof(CharIntro), typeof(Flashback), typeof(CamToPosition)
    };*/
    bool ExcludeCheck(IDialogueEvent e)
    {
        foreach (Type t in exclusionTypes)
            if (e.GetType() == t)
                return false;

        return true;
    }
    /*
    bool SLExcludeCheck(IDialogueEvent e)
    {
        foreach (Type t in slExclusionTypes)
            if (e.GetType() == t)
                return false;

        return true;
    }*/
    void DialogueEvents(Line currentLine)
    {
        //List<IDialogueEvent> events = currentLine.DiaEvents;
        var events = currentLine.DiaEvents.Where(n => ExcludeCheck(n));

        foreach (IDialogueEvent e in events)
            e.TriggerDialogueEvent();
    }
    /*
    void SLDialogueEvents(Line currentLine)
    {
        //List<IDialogueEvent> events = currentLine.DiaEvents;
        var events = currentLine.DiaEvents.Where(n => SLExcludeCheck(n));

        foreach (IDialogueEvent e in events)
            e.TriggerDialogueEvent();
    }*/
    #endregion

    #region Choice Configuration
    void ClearChoiceText()
    {
        foreach (TextMeshProUGUI tex in ChoiceTextObjects)
        {
            tex.text = "";
        }
    }
    void ConfigureChoiceText(Dialogue dia, Character character)
    {

        ChoiceTextObjects[0].text = dia.Choices[0].ChoiceText;
        ChoiceTextObjects[1].text = dia.Choices[1].ChoiceText;
        ChoiceOptions[0].gameObject.SetActive(true);
        ChoiceOptions[1].gameObject.SetActive(true);
        if (ChoiceGroup)
        {
            ChoiceGroup.Reveal();
            SoundManager.instance.PlayChoiceSound();
        }
        ChoiceOptions[0].Select();
        ChoiceOptions[2].enabled = false;
    }
    void Choose(GameObject choice, Character character)
    {
        SoundManager.instance.PlaySFX(choiceConfirm);
        try
        {
            if (choice == ChoiceOptions[0].gameObject)
                choice1.Invoke();
            if (choice == ChoiceOptions[1].gameObject)
                choice2.Invoke();
        }
        catch
        {
            Debug.LogError("A choice function hasn't been added");
        }
        if (ChoiceGroup)
            ChoiceGroup.Hide();
    }
    #endregion



    void ChangeItemSelect(object o)
    {
        CISTuple data = (CISTuple)o;
        var items = FindObjectsOfType<ItemActor>().Where(n => n.gameObject.name == data.objectName);
        if (items.Count() > 0)
        {
            items.ElementAt(0).selectable = data.isSelectable;
            items.ElementAt(0).SetSelectable(data.isSelectable);
        }

    }
    public static void ExitChar(Actor actor)
    {
        Debug.LogWarning("ExitChar Called!");

        //OnLineStart -= () => { ExitChar(actor); };
        actor.transform.parent.gameObject.transform.position = new Vector3(0, -20, 0);
    }
}

