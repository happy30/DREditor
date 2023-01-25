//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Camera;
using DREditor.Characters;
using DREditor.Debug;
using DREditor.Dialogues;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using DREditor.PlayerInfo;
using DG.Tweening;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using DREditor.EventObjects;
using UnityEngine.SceneManagement;
using System.Linq;
using DREditor.Dialogues.Events;
using System;

public class TrialDialogueManager : MinigameManagerBase
{
    [Header("Required")]
    [SerializeField] CharacterDatabase database = null;
    [SerializeField] DRTrialCamera cameraa = null;
    [SerializeField] AudioSource nex = null;
    [SerializeField] TrialCameraAnimDatabase tCAD = null;
    private List<string> animNames;
    public static TrialDialogueManager instance = null;
    public bool trigger = false;
    [SerializeField] BoolWithEvent InMenu = null;
    [SerializeField] BoolWithEvent InDialogue = null;
    Volume volume => GetComponentInChildren<Volume>();

    [Header("Debugging")]
    [SerializeField] bool debug = false;
    [SerializeField] float ffModeSpeed = 2;
    [SerializeField] TextMeshProUGUI dialogueBox = null;
    [SerializeField] float pauseTimeFF = 0.5f;

    [Header("Continue Confirmation")]
    [SerializeField] TrialDialogue continueConfirm = null;
    [SerializeField] TrialDialogue timeUpDialogue = null;

    [Header("Animation of the UI")]
    [SerializeField] Animator uiAnimator = null;

    [Header("Name Plate and Char Portrait")]
    [SerializeField] RawImage charPortrait = null;
    [SerializeField] RawImage charPlate = null;
    //[SerializeField] Texture plateStatic = null;
    //[SerializeField] Texture portriatStatic = null;
    [SerializeField] Texture2D transparentImage = null;
    [HideInInspector] public RawImage namePlate = null;
    Actor lastActor;

    public bool autoMode = false;
    public bool ffMode = false;
    [Header("Dialogue UI")]
    [SerializeField] DialogueIcon diaMouseIcon = null;
    [SerializeField] DialogueIcon diaAutoIcon = null;
    DialogueIcon mouseIcon = null;
    DialogueIcon autoIcon = null;

    [Header("Tutorial Options")]
    [SerializeField] string textBoxOnlyBoolString = "";

    [Header("Debug Viewing")]
    public Actor currentActor;
    //public Actor lastActor;
    Character currentChar;
    //PlayerInput playerInput => GameManager.instance.GetInput();

#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        cameraa = GameObject.Find("DRCameraAnchor").GetComponentInChildren<DRTrialCamera>();
        volume.enabled = true;
        animNames = tCAD.GetNames();

        namePlate = charPlate;
        mouseIcon = diaMouseIcon;

        autoIcon = diaAutoIcon;

#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
        //DontDestroyOnLoad(this);
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
        _controls.UI.FastForward.started -= FFModeOn;
        _controls.UI.FastForward.canceled -= FFModeOff;
        if (debug)
        {
            DRTrialCamera.OnTestTrialLine -= DebugTestLine;
        }
        DialogueEventSystem.StopListening("TrialTutorial", TutorialEvaluation);
        UIHandler.ToTitle -= ResetTrialDia;
    }
    void Start()
    {
        if (debug)
        {
            DRTrialCamera.OnTestTrialLine += DebugTestLine;
        }

        if (dialogueBox != null && DialogueTextConfig.instance != null)
            DialogueTextConfig.instance.SetTrialBox(dialogueBox);
        

        foreach(Character character in database.Characters)
        {
            if (character.TrialPosition > 15)
                continue;

            cameraa.CharHeightOffset[character.TrialPosition] = character.TrialHeight;
        }
        DialogueEventSystem.StartListening("TrialTutorial", TutorialEvaluation);
        UIHandler.ToTitle += ResetTrialDia;
    }
    void ResetTrialDia()
    {
        if (CGPlayer.inCG)
        {
            DialogueEventSystem.TriggerEvent("HideCG");
        }
        if (CGPlayer.inVid)
        {
            DialogueEventSystem.TriggerEvent("HideVideo", new SVTuple());
        }
    }
    private void Update()
    {
        if (InDialogue.Value
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

    #region Icons + FFMode
    public void EnableIcons(bool to)
    {
        //mouseImage.enabled = to;
        autoIcon.enabled = to;
        //ffIcon.enabled = to;
        if (autoMode)
        {
            mouseIcon.TurnOff();
            autoIcon.TurnOn();
        }
        else
        {
            autoIcon.TurnOff();
        }
    }
    public void SwapIcons() // For swapping Icons going to and from CG Mode
    {
        mouseIcon = mouseIcon == diaMouseIcon ? CGPlayer.instance.mouseIcon : diaMouseIcon;
        //mouseImage = mouseImage == diaMouseImage ? cgMouseImage : diaMouseImage;
        autoIcon = autoIcon == diaAutoIcon ? CGPlayer.instance.autoIcon : diaAutoIcon;
        //ffIcon = ffIcon == diaffIcon ? cgffIcon : diaffIcon;
        if (autoMode)
        {
            mouseIcon.TurnOff();
            autoIcon.TurnOn();
        }
        else
        {
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
    public bool cantToggle = false;
    void ToggleAutoMode()
    {
        if (cantToggle || InMenu.Value)
            return;
        autoMode = !autoMode;
        if (autoMode && mouseIcon != null && autoIcon != null)
        {
            mouseIcon.TurnOff();
            autoIcon.TurnOn();
        }
        else if (autoIcon != null)
        {
            autoIcon.TurnOff();
            //mouseIcon.TurnOn();
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

    public void PlayTrialDialogue(ScriptableObject dial)
    {
        if (dial.GetType().Equals(typeof(TrialDialogue)))
        {
            if (!GameSaver.LoadingFile)
            {
                TrialDiscussion disc = (TrialDiscussion)ScriptableObject.CreateInstance("TrialDiscussion");
                disc.trialDialogues[0] = (TrialDialogue)dial;
                PlayTrialDiscussion(disc);
                if (SaveDirectToNum == 0)
                    SaveDialogueName = disc.trialDialogues[0].name;
                Debug.Log("Trial Dialogue Manager is Playing");
            }
            else
            {
                LoadThrough((TrialDialogue)dial, 0, 0);
            }
        }
        else if(dial.GetType().Equals(typeof(TrialDiscussion)))
        {
            if (!GameSaver.LoadingFile)
            {
                TrialDialogueManager.instance.PlayTrialDiscussion(dial);
                TrialDiscussion d = (TrialDiscussion)dial;
                SaveDialogueName = d.trialDialogues[0].name;
                Debug.Log("Trial Dialogue Manager is Playing");
            }
            else
            {
                TrialDiscussion d = (TrialDiscussion)dial;
                LoadThrough(d.trialDialogues[0], 0, 0, d);
            }
        }
    }
    public void PlayTrialDiscussion(ScriptableObject dial)
    {
        volume.enabled = true;
        StartCoroutine(TrialDialogueBegin((TrialDiscussion)dial));
    }
    IEnumerator TrialDialogueBegin(TrialDiscussion dial, int skip = 0)
    {
        InDialogue.Value = true;
        GameManager.SetControls("trialdialogue");
        DialogueTextConfig.instance.ClearText();
        _controls.UI.FastForward.started += FFModeOn;
        _controls.UI.FastForward.canceled += FFModeOff;

        SaveLineNum = 0;

        for (int x = 0; x < dial.trialDialogues.Length; x++)
        {
            if (skip > 0)
                x = skip;
            TrialDialogue currentDialogue = dial.trialDialogues[x];
            
            int lineNum = 0;
            DialogueTextConfig.instance.ClearText();
            for (int i = 0; i < currentDialogue.Lines.Count; i++)
            {
                TrialLine currentTrialLine = currentDialogue.Lines[i];

                #region Loading File
                if (GameSaver.LoadingFile)
                {
                    if (GameSaver.CurrentGameData.DialogueData.LineNum != 0)
                    {
                        i = LoadToLine(currentDialogue);
                        if (OnLoadCG)
                        {
                            LastCG.TriggerDialogueEvent();
                            CGPlayer.cgDone = false;
                            yield return new WaitUntil(() => CGPlayer.cgDone);
                            CGPlayer.cgDone = false;
                        }
                        if (OnLoadVid && LastVid != null)
                        {
                            LastVid.TriggerDialogueEvent();
                            CGPlayer.vidDone = false;
                            yield return new WaitUntil(() => CGPlayer.vidDone);
                            CGPlayer.vidDone = false;
                        }
                        Debug.Log("called line: " + i);
                        currentTrialLine = currentDialogue.Lines[i];
                        SaveDirectToNum = GameSaver.CurrentGameData.DialogueData.DirectToNum;
                        SaveLineNum = GameSaver.CurrentGameData.DialogueData.LineNum;
                    }
                    ffMode = false;
                }
                #endregion

                #region CG
                var cg = currentTrialLine.DiaEvents.Where(n => n.GetType() == typeof(CGDisplay));
                if (cg.Count() > 0)
                {
                    cg.ElementAt(0).TriggerDialogueEvent();
                    CGPlayer.cgDone = false;
                    yield return new WaitUntil(() => CGPlayer.cgDone);
                    CGPlayer.cgDone = false;
                }
                var vid = currentTrialLine.DiaEvents.Where(n => n.GetType() == typeof(VideoDisplay));
                if (vid.Count() > 0)
                {
                    vid.ElementAt(0).TriggerDialogueEvent();
                    CGPlayer.vidDone = false;
                    yield return new WaitUntil(() => CGPlayer.vidDone);
                    CGPlayer.vidDone = false;
                    VideoDisplay video = (VideoDisplay)vid.ElementAt(0);
                    if (video.SVValue.playOnly)
                    {
                        SaveLineNum++;
                        continue;
                    }
                }
                #endregion

                if (currentTrialLine.DiaEvents.Count != 0)
                    DialogueEvents(currentTrialLine);

                if (currentTrialLine.Events != null)
                {
                    foreach (var ev in currentTrialLine.Events)
                    {
                        ev.Raise();
                        while (trigger == false) // Will wait until trigger is false from Event Ending
                        {
                            yield return null;
                        }
                        trigger = false;
                    }
                }
                // if there's more than 16 add a height offset to the trial cam anchor
                if (currentTrialLine.co != null && currentTrialLine.co.enabled)
                {
                    cameraa.ApplyOverrides(currentTrialLine.co, currentTrialLine.DontPan);
                }
                else
                {
                    cameraa.SetDefaultValues();
                    if (currentTrialLine.Speaker != null && currentTrialLine.Speaker.TrialPosition < 16 && !currentTrialLine.DontPan)
                    {
                        cameraa.SeatFocus = currentTrialLine.Speaker.TrialPosition;
                    }
                }

                //yield return new WaitForSeconds(0.0015f);
                GameObject currentCharObject = null;
                if (currentTrialLine.Speaker != null)
                    currentCharObject = GameObject.Find(currentTrialLine.Speaker.FirstName);
                else
                    continue;
                //For Above: May make a singleton that has the reference of all the characters

                MeshRenderer mesh = null;

                if(currentCharObject != null)
                {
                    mesh = currentCharObject.GetComponentInChildren<MeshRenderer>();
                    currentActor = currentCharObject.GetComponentInChildren<Actor>();
                }
                
                currentChar = currentTrialLine.Speaker;

                #region Changing Nameplate and Portrait

                if (CGPlayer.instance != null && (CGPlayer.inCG || CGPlayer.inVid))
                    SetNameplate(currentTrialLine);
                else if (lastActor != null && lastActor != currentActor || lineNum == 0)
                {
                    StartCoroutine(ChangeCharacterAnim(currentTrialLine));
                }
                #endregion

                string anim = animNames[currentTrialLine.camAnimIdx];
                
                if (currentTrialLine.camAnimIdx != 10)
                {
                    cameraa.CameraAnimator.Rebind();
                    cameraa.TriggerAnim(anim);
                }

                if (currentTrialLine.Expression.Sprite != null)
                {
                    mesh.material.mainTexture = currentTrialLine.Expression.Sprite.mainTexture;
                    currentActor.characterb.material.mainTexture = currentTrialLine.Expression.Sprite.mainTexture;
                }

                if (!uiAnimator.GetBool("ShowTDUI"))
                {
                    uiAnimator.SetBool("ShowTDUI", true);
                    yield return new WaitForSeconds(1);
                }

                #region Audio

                if (currentTrialLine.SFX.Count != 0)
                    SoundManager.instance.PlaySFX(currentTrialLine.SFX[0]);

                if (currentTrialLine.VoiceSFX != null && !GameSaver.LoadingFile)
                    SoundManager.instance.PlayVoiceLine(currentTrialLine.VoiceSFX);

                if (currentTrialLine.MusicChange && !GameSaver.LoadingFile)
                {
                    if (currentTrialLine.Music != null)
                        SoundManager.instance.PlayMusic(currentTrialLine.Music);
                    else
                        SoundManager.instance.StopSound();
                }
                
                #endregion

                #region Displaying Text
                if (currentTrialLine.Text == "")
                {
                    if (!ffMode && currentTrialLine.AutomaticLine)
                    {
                        yield return new WaitForSeconds(currentTrialLine.TimeToNextLine);
                    }
                    if (!GameSaver.LoadingFile)
                    {
                        SaveLineNum++;
                        continue;
                    }
                }
                else // Normal Display Text
                {
                    if (CGPlayer.inVid)
                    {
                        CGPlayer.instance.FadeBox(true);
                    }
                    if (!CGPlayer.instance.autoIcon.gameObject.activeSelf)
                    {
                        autoIcon.gameObject.SetActive(true);
                    }
                    DialogueTextConfig.instance.DisplayText(currentTrialLine);
                    DialogueTextConfig.instance.isWaitingForClick = true;
                }

                while (DialogueTextConfig.instance.isWaitingForClick)
                {
                    if (ffMode)
                    {
                        break;
                    }
                    yield return null;
                }
                #endregion

                if (GameSaver.LoadingFile)
                {
                    DialogueLoaded = true;
                    yield return new WaitUntil(() => !GameSaver.LoadingFile);
                    DialogueLoaded = false;
                    ffMode = false;
                    AudioListener.volume = 1;
                }
                if(!ffMode)
                    GameManager.instance.cantBeInMenu = false;

                #region Wait for Button Click
                //This should eventually be a coroutine then a wait until the coroutine is finished
                yield return new WaitForSeconds(Time.deltaTime);

                //mouseIcon.SetBool("canClick", true);
                if (mouseIcon != null && !autoMode && !mouseIcon.on)
                {
                    mouseIcon.TurnOn();
                }


                while (!_controls.UI.Submit.triggered)
                {
                    while (InMenu.Value)
                    {
                        yield return null;
                    }
                    if (autoMode)
                    {
                        goNext = false;
                        Coroutine c = StartCoroutine(WaitForAuto());
                        while (!goNext)
                        {
                            if (_controls.UI.Submit.triggered && !InMenu.Value)
                                break;

                            if (ffMode && !InMenu.Value)
                            {
                                yield return new WaitForSeconds(Time.deltaTime);
                                break;
                            }
                            if (mouseIcon != null && !autoMode)
                            {
                                mouseIcon.TurnOn();
                                break;
                            }
                            yield return null;
                        }
                        if (autoMode && goNext)
                            break;
                        else
                            StopCoroutine(c);
                    }

                    if (ffMode)
                    {
                        yield return new WaitForSeconds(pauseTimeFF);
                        //DialogueTextConfig.instance.ClearText();
                        break;
                    }

                    if (_controls.UI.Submit.triggered)
                        break;

                    yield return null;
                }
                if (!ffMode)
                    SoundManager.instance.PlayNext();
                //mouseIcon.SetBool("canClick", false);

                if (mouseIcon != null && !autoMode)
                {
                    mouseIcon.TurnOff();
                }

                DialogueAssetReader.Backlog.AddLine(currentChar, DialogueTextConfig.instance.GetBox().text, currentTrialLine.AliasNumber-1 ,
                    DialogueTextConfig.instance.GetBox().color);
                DialogueTextConfig.instance.ClearText();
                #endregion

                SoundManager.instance.StopVoiceLine();
                GameManager.instance.cantBeInMenu = true;
                nex.Play();

                if (lineNum != currentDialogue.Lines.Count - 1)
                    DialogueTextConfig.instance.ClearText();
                lineNum += 1;
                SaveLineNum++;
                lastActor = currentActor;
            }

            #region End Dialogue Options

            if (currentDialogue.FlagTrigger.Enabled)
            {
                TriggerFlag t = currentDialogue.FlagTrigger;
                ProgressionManager.instance.TriggerFlag(t.Chapter, t.Objective, t.Flags);
            }

            if (currentDialogue.DirectTo != null && currentDialogue.DirectTo.Enabled)
            {
                _controls.UI.FastForward.started -= FFModeOn;
                _controls.UI.FastForward.canceled -= FFModeOff;

                PlayTrialDialogue(currentDialogue.DirectTo.NewTrialDialogue);

                SaveDirectToNum += 1;
                if (currentDialogue.SceneTransition.Enabled && !currentDialogue.SceneTransition.AtEnd)
                {
                    SaveTransitionNum = SaveDirectToNum;
                }
                yield break;
            }
            else if (x != dial.trialDialogues.Length - 1)
            {
                SaveDirectToNum += 1;
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

            if (currentDialogue.skip > 0)
            {
                TrialManager.SkipSequences(currentDialogue.skip);
            }

            #endregion

            SaveLineNum = 0;

        }
        GameManager.instance.cantBeInMenu = true;
        DialogueTextConfig.instance.ClearText();
        CleanUp();
        EndMinigame();
        cameraa.SetDefaultValues();
        volume.enabled = false;
        yield break;
    }
    void CleanUp()
    {
        if (ffMode)
            FFModeOff(new CallbackContext());
        _controls.UI.FastForward.started -= FFModeOn;
        _controls.UI.FastForward.canceled -= FFModeOff;

        ClearSaveData();
        InDialogue.Value = false;
        Debug.Log("Clean Up Called");
    }
    IEnumerator ChangeCharacterAnim(TrialLine currentTrialLine)
    {
        if (currentTrialLine.AliasNumber > 0)
        {
            Debug.Log(currentTrialLine.AliasNumber);
            charPlate.texture = currentTrialLine.Speaker.Aliases[currentTrialLine.AliasNumber-1].TrialNameplate;
            charPortrait.texture = currentTrialLine.Speaker.Aliases[currentTrialLine.AliasNumber-1].TrialPortrait;
        }
        else
        {
            charPlate.texture = currentTrialLine.Speaker.TrialNameplate;
            charPortrait.texture = currentTrialLine.Speaker.TrialPortrait;
        }
        
        yield break;
    }
    public void ShowUI()
    {
        uiAnimator.SetBool("ShowTDUI", true);
    }
    public void HideUI()
    {
        uiAnimator.SetBool("ShowTDUI", false);
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
    public void SwapNamePlate()
    {
        namePlate = namePlate == charPlate ? CGPlayer.instance.namePlate : charPlate;
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

    #region Load Dialogue
    public static bool DialogueLoaded = false;
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
        
    }
    static IEnumerator WaitForActive(TrialDiscussion disc, int skip = 0)
    {
        yield return new WaitUntil(() => TrialDialogueManager.instance.isActiveAndEnabled);
        TrialDialogueManager.instance.StartCoroutine(TrialDialogueManager.instance.TrialDialogueBegin(disc, skip));
        yield break;
    }
    public void LoadThrough(TrialDialogue dia, int directNum, int conditionNum, TrialDiscussion disc = null, int skip = 0)
    {
        SaveDirectToNum = GameSaver.CurrentGameData.DialogueData.DirectToNum;
        SaveCondition = GameSaver.CurrentGameData.DialogueData.Conditional;
        SaveLineNum = GameSaver.CurrentGameData.DialogueData.LineNum;
        SaveTransitionNum = GameSaver.CurrentGameData.DialogueData.TransitionNum;
        Debug.Log("SaveLine Num is: " + SaveLineNum);

        


        ffMode = true;
        if (directNum == SaveDirectToNum && conditionNum == SaveCondition.Count) // Check point to when we reach the correct dialogue asset
        {
            Debug.Log("LoadThrough");
            if (disc != null)
                TrialDialogueManager.instance.StartCoroutine(WaitForActive(disc, skip));
            else
            {
                TrialDiscussion discu = (TrialDiscussion)ScriptableObject.CreateInstance("TrialDiscussion");
                discu.trialDialogues[0] = dia.DirectTo.NewTrialDialogue;
                PlayTrialDiscussion(discu);
            }
            return;
        }
        for (int i = 0; i < dia.Lines.Count; i++)
        {
            TrialLine currentLine = dia.Lines[i];
            currentChar = currentLine.Speaker;

            if (currentLine.Speaker != null)
            {
                currentActor = DialogueAnimConfig.instance.FindActor(currentLine.Speaker.FirstName);
            }
            if (SaveTransitionNum > 0)
            {
                if (SaveDirectToNum > 0 && directNum >= SaveTransitionNum)
                    DialogueEvents(currentLine);
            }
            else
            {
                DialogueEvents(currentLine);
            }

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
                OnLoadVid = display.SVValue.mainClip != null;
                if (OnLoadVid)
                    LastVid = display;
                else
                    LastVid = null;

            }
            lastActor = currentActor;


        }
        if (directNum != SaveDirectToNum)
        {
            if (!dia.DirectTo.Enabled && disc != null && disc.GetIndexOfDia(dia) != disc.trialDialogues.Length - 1)
            {
                LoadThrough(disc.trialDialogues[disc.GetIndexOfDia(dia) + 1], directNum + 1, conditionNum, disc, skip + 1);
                return;
            }
            if (dia.DirectTo.NewDialogue != null)
                LoadThrough(dia.DirectTo.NewTrialDialogue, directNum + 1, conditionNum);
        }
        else
        {
            Debug.Log("Playing Direct To of: " + dia.name);
            
            if(disc != null)
                StartCoroutine(TrialDialogueBegin(disc, skip));
            else
            {
                TrialDiscussion discu = (TrialDiscussion)ScriptableObject.CreateInstance("TrialDiscussion");
                discu.trialDialogues[0] = dia.DirectTo.NewTrialDialogue;
                PlayTrialDiscussion(discu);
            }
        }
    }

    int LoadToLine(TrialDialogue dia)
    {
        for (int i = 0; i < dia.Lines.Count; i++)
        {
            TrialLine currentLine = dia.Lines[i];
            if (i == GameSaver.CurrentGameData.DialogueData.LineNum)
            {
                Debug.Log("Returning: " + i);
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
                return i;
            }
            
            currentChar = currentLine.Speaker;

            if(currentLine.Speaker != null)
            {
                currentActor = DialogueAnimConfig.instance.FindActor(currentLine.Speaker.FirstName);
            }
            

            DialogueEvents(currentLine);

            var cg = currentLine.DiaEvents.Where(n => n.GetType() == typeof(CGDisplay));
            if (cg.Count() > 0)
            {
                CGDisplay display = (CGDisplay)cg.ElementAt(0);
                OnLoadCG = display.SCGValue.CG != null || display.SCGValue.prefab != null;
                if (OnLoadCG)
                {
                    LastCG = display;
                    OnLoadVid = false;
                    LastVid = null;
                }
                else
                    LastCG = null;

            }
            var vid = currentLine.DiaEvents.Where(n => n.GetType() == typeof(VideoDisplay));
            if (vid.Count() > 0)
            {
                VideoDisplay display = (VideoDisplay)vid.ElementAt(0);
                OnLoadVid = display.SVValue.mainClip != null;
                if (OnLoadVid)
                {
                    LastVid = display;
                    OnLoadCG = false;
                    LastCG = null;
                }
                else
                    LastVid = null;

            }

            if (currentLine.co != null && currentLine.co.enabled)
            {
                cameraa.ApplyOverrides(currentLine.co, currentLine.DontPan);
            }
            else
            {
                cameraa.SetDefaultValues();
                if (currentLine.Speaker != null && currentLine.Speaker.TrialPosition < 16 && !currentLine.DontPan)
                {
                    cameraa.SeatFocus = currentLine.Speaker.TrialPosition;
                }
            }
            string anim = animNames[currentLine.camAnimIdx];

            if (currentLine.camAnimIdx != 10)
            {
                cameraa.TriggerAnim(anim);
            }
            lastActor = currentActor;
        }

        return 0;
    }
    #endregion

    #region Dialogue Events
    readonly List<Type> exclusionTypes = new List<Type>()
    {
        typeof(ShakeObject), typeof(CGDisplay), typeof(VideoDisplay), typeof(FaintAnim), typeof(CharacterLeave)
    };
    bool ExcludeCheck(IDialogueEvent e)
    {
        foreach (Type t in exclusionTypes)
            if (e.GetType() == t)
                return false;

        return true;
    }
    void DialogueEvents(Line currentLine)
    {
        //List<IDialogueEvent> events = currentLine.DiaEvents;
        var events = currentLine.DiaEvents.Where(n => ExcludeCheck(n));

        foreach (IDialogueEvent e in events)
            e.TriggerDialogueEvent();
    }
    #endregion

    #region NSD Additions
    public void PlayFuckUp(TrialDialogue currentTrialDialogue) => StartCoroutine(FuckUp(currentTrialDialogue, true));
    public void PlayPanelEnd(TrialDialogue currentTrialDialogue) => StartCoroutine(FuckUp(currentTrialDialogue, false));
    public void PlayFuckUpNonNSD(TrialDialogue currentTrialDialogue) => StartCoroutine(FuckUp(currentTrialDialogue, true, true));
    public static void PlayContinue()
    {
        if(TrialDialogueManager.instance != null)
        {
            TrialDialogueManager.instance.DoContinue();
        }
    }
    public void DoContinue()
    {
        StartCoroutine(FuckUp(continueConfirm, false, true, true));
    }
    public static void PlayTimeUp() => instance.TimeUp();
    public void TimeUp() => StartCoroutine(FuckUp(timeUpDialogue, false, true, false, true));
    public delegate void voidDel();
    public static event voidDel EndFU;

    IEnumerator FuckUp(TrialDialogue currentTrialDialogue, bool isMistake, bool notNSD = false, bool continuing = false, bool timeUp = false)
    {
        //if (timeUp && !TrialStats.showing)
        //TrialStats.Instance.ShowNSDStats();
        InDialogue.Value = true;
        int lineNum = 0;
        DialogueTextConfig.instance.ClearText();
        if (currentTrialDialogue == null)
        {
            Debug.LogWarning("CRITICAL ERROR: THE FUCK UP DIALOGUE WAS NULL");
        }
        if (currentTrialDialogue.Lines == null)
        {
            Debug.LogWarning("CRITICAL ERROR: THE LINES WAS NULL");
        }
        foreach (TrialLine currentTrialLine in currentTrialDialogue.Lines)
        {
            if (currentTrialLine.Events != null)
            {
                foreach (var ev in currentTrialLine.Events)
                {
                    ev.Raise();
                    while (trigger == false) // Will wait until trigger is false from Event Ending
                    {
                        yield return null;
                    }
                    trigger = false;
                }
            }

            cameraa.SeatFocus = currentTrialLine.Speaker.TrialPosition;

            yield return new WaitForSeconds(0.0015f);

            if(isMistake && currentTrialLine == currentTrialDialogue.Lines[currentTrialDialogue.Lines.Count - 1]) // on final fuck up line
            {
                // play sfx, take damage , and then update visuals

                if(TrialStats.Instance != null)
                {
                    if (notNSD)
                        TrialStats.Instance.CalculateColor();
                    PlayerInfo.instance.TakeDamage(1);
                    TrialStats.Instance.TakeDamage();
                }
            }
            
            GameObject currentCharObject = GameObject.Find(currentTrialLine.Speaker.FirstName);
            //For Above: May make a singleton that has the reference of all the characters

            MeshRenderer mesh = currentCharObject.GetComponentInChildren<MeshRenderer>();

            Actor currentActor = currentCharObject.GetComponentInChildren<Actor>();

            #region Changing Nameplate and Portrait
            if (lastActor != null && lastActor != currentActor || lineNum == 0)
            {
                StartCoroutine(ChangeCharacterAnim(currentTrialLine));
            }
            #endregion

            string anim = animNames[currentTrialLine.camAnimIdx];

            if (lineNum == 0)
                cameraa.CameraAnimator.Play(anim);
            else
                cameraa.TriggerAnim(anim);
            //Debug.LogWarning("Calling: " + anim);
            //cameraa.CameraAnimator.Play(anim);

            if (currentTrialLine.Expression.Sprite != null)
            {
                mesh.material.mainTexture = currentTrialLine.Expression.Sprite.mainTexture;
                currentActor.characterb.material.mainTexture = currentTrialLine.Expression.Sprite.mainTexture;
            }
            if (!uiAnimator.GetBool("ShowTDUI"))
            {
                uiAnimator.SetBool("ShowTDUI", true);
                yield return new WaitForSeconds(1);
            }
            #region Audio

            //if (currentTrialLine.SFX.Count != 0)
            //SoundManager.instance.PlaySFX(currentTrialLine.SFX[0]);

            if (currentTrialLine.VoiceSFX != null)
                SoundManager.instance.PlayVoiceLine(currentTrialLine.VoiceSFX);


            #region Music
            if (currentTrialLine.MusicChange)
            {
                if (currentTrialLine.Music != null)
                    SoundManager.instance.PlayMusic(currentTrialLine.Music);
                else
                    SoundManager.instance.StopSound();
            }
            #endregion

            #endregion

            #region Displaying Text
            if (currentTrialLine.Text.Contains("$"))
            {
                SoundManager.instance.StopVoiceLine();
            }
            DialogueTextConfig.instance.DisplayText(currentTrialLine);

            DialogueTextConfig.instance.isWaitingForClick = true;

            while (DialogueTextConfig.instance.isWaitingForClick)
            {
                yield return null;
            }
            #endregion

            if (continuing && TrialStats.Instance != null) // Called On Game Over Continue
            {
                TrialStats.Instance.ShowNSDStats();
                TrialStats.Instance.RegenHealthVisual();
                yield return new WaitForSeconds(2);
                TrialStats.Instance.HideNSDStats();
                PlayerInfo.instance.ResetHealth();
            }

            #region Wait for Button Click

            yield return new WaitForSeconds(Time.deltaTime);

            //mouseIcon.SetBool("canClick", true);
            if (mouseIcon != null && !autoMode && !mouseIcon.on)
            {
                mouseIcon.TurnOn();
            }


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
                        if (_controls.UI.Submit.triggered)
                            break;

                        if (ffMode)
                        {
                            //yield return new WaitForSeconds(Time.deltaTime);
                            break;
                        }
                        if (mouseIcon != null && !autoMode)
                        {
                            mouseIcon.TurnOn();
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
            if (!ffMode)
                SoundManager.instance.PlayNext();
            //mouseIcon.SetBool("canClick", false);

            if (mouseIcon != null && !autoMode)
            {
                mouseIcon.TurnOff();
            }

            //DialogueAssetReader.Backlog.AddLine(currentChar, currentTrialLine.Text, currentTrialLine.VoiceSFX);
            DialogueTextConfig.instance.ClearText();
            #endregion


            nex.Play();

            if (lineNum != currentTrialDialogue.Lines.Count - 1)
                DialogueTextConfig.instance.ClearText();
            lineNum += 1;
            lastActor = currentActor;
        }
        if (!continuing && isMistake && PlayerInfo.instance.CurrentHealth <= 0 || timeUp)
        {
            // Game Over Code here
            Debug.Log("Player has Died");
            PlayerHasDied?.Invoke();
            yield return new WaitForSeconds(1);
            //MultipleChoiceManager.instance.StartGameOver();
            if(TrialStats.Instance != null)
                TrialStats.Instance.HideNSDStats();
            yield break;
        }
        
        uiAnimator.SetBool("ShowTDUI", false);
        if (isMistake && TrialStats.Instance != null)
            TrialStats.Instance.HideNSDStats();
        yield return new WaitForSeconds(1);
        Debug.Log("Finishing Trial Dia");
        InDialogue.Value = false;
        //cameraa.CameraAnimator.GetCurrentAnimatorStateInfo(0).norm
        EndFU?.Invoke();
        yield break;
    }
    #endregion

    #region For All Managers
    public delegate void del();
    public static event del DialogueEnded;
    //public static event del PlayerHasContinued;
    public static event del PlayerHasDied;
    public static void CallPlayerHasDied() => PlayerHasDied?.Invoke();
    public void UniTrialDialogue(TrialDialogue currentTrialDialogue) => StartCoroutine(PlayTD(currentTrialDialogue));
    IEnumerator PlayTD(TrialDialogue currentTrialDialogue)
    {
        int lineNum = 0;
        DialogueTextConfig.instance.ClearText();
        foreach (TrialLine currentTrialLine in currentTrialDialogue.Lines)
        {
            if (currentTrialLine.Events != null)
            {
                foreach (var ev in currentTrialLine.Events)
                {
                    ev.Raise();
                    while (trigger == false) // Will wait until trigger is false from Event Ending
                    {
                        yield return null;
                    }
                    trigger = false;
                }
            }

            cameraa.SeatFocus = currentTrialLine.Speaker.TrialPosition;

            yield return new WaitForSeconds(0.0015f);

            GameObject currentCharObject = GameObject.Find(currentTrialLine.Speaker.FirstName);
            //For Above: May make a singleton that has the reference of all the characters

            MeshRenderer mesh = currentCharObject.GetComponentInChildren<MeshRenderer>();

            Actor currentActor = currentCharObject.GetComponentInChildren<Actor>();

            #region Changing Nameplate and Portrait
            if (lastActor != null && lastActor != currentActor || lineNum == 0)
            {
                StartCoroutine(ChangeCharacterAnim(currentTrialLine));
            }
            #endregion

            string anim = animNames[currentTrialLine.camAnimIdx];

            cameraa.TriggerAnim(anim);

            if (currentTrialLine.Expression.Sprite != null)
            {
                mesh.material.mainTexture = currentTrialLine.Expression.Sprite.mainTexture;
                currentActor.characterb.material.mainTexture = currentTrialLine.Expression.Sprite.mainTexture;
            }
            if (!uiAnimator.GetBool("ShowTDUI"))
            {
                uiAnimator.SetBool("ShowTDUI", true);
                yield return new WaitForSeconds(1);
            }
            #region Audio

            //if (currentTrialLine.SFX.Count != 0)
            //SoundManager.instance.PlaySFX(currentTrialLine.SFX[0]);

            //if (currentTrialLine.VoiceSFX != null)
            //SoundManager.instance.PlayVoiceLine(currentTrialLine.VoiceSFX);

            // Music change not implemented yet

            #endregion

            #region Displaying Text
            DialogueTextConfig.instance.DisplayText(currentTrialLine);

            DialogueTextConfig.instance.isWaitingForClick = true;

            while (DialogueTextConfig.instance.isWaitingForClick)
            {
                yield return null;
            }
            #endregion


            #region Wait for Button Click

            yield return new WaitForSeconds(Time.deltaTime);

            //mouseIcon.SetBool("canClick", true);
            if (mouseIcon != null && !autoMode && !mouseIcon.on)
            {
                mouseIcon.TurnOn();
            }


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
                        if (_controls.UI.Submit.triggered)
                            break;

                        if (ffMode)
                        {
                            //yield return new WaitForSeconds(Time.deltaTime);
                            break;
                        }
                        if (mouseIcon != null && !autoMode)
                        {
                            mouseIcon.TurnOn();
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
            if (!ffMode)
                SoundManager.instance.PlayNext();
            //mouseIcon.SetBool("canClick", false);

            if (mouseIcon != null && !autoMode)
            {
                mouseIcon.TurnOff();
            }

            //DialogueAssetReader.Backlog.AddLine(currentChar, currentTrialLine.Text, currentTrialLine.VoiceSFX);
            DialogueTextConfig.instance.ClearText();
            #endregion


            nex.Play();

            if (lineNum != currentTrialDialogue.Lines.Count - 1)
                DialogueTextConfig.instance.ClearText();
            lineNum += 1;
            lastActor = currentActor;
        }
        

        uiAnimator.SetBool("ShowTDUI", false);

        yield return new WaitForSeconds(1);

        DialogueEnded?.Invoke();

        yield break;
    }
    #endregion

    #region TrialCamDebugTestLine
    public void DebugTestLine(TrialLine currentTrialLine)
    {
        // if there's more than 16 add a height offset to the trial cam anchor
        if (currentTrialLine.co != null && currentTrialLine.co.enabled)
        {
            cameraa.ApplyOverrides(currentTrialLine.co, currentTrialLine.DontPan);
        }
        else
        {
            cameraa.SetDefaultValues();
            if (currentTrialLine.Speaker.TrialPosition < 16 && !currentTrialLine.DontPan)
            {
                cameraa.SeatFocus = currentTrialLine.Speaker.TrialPosition;
            }
        }

        //yield return new WaitForSeconds(0.0015f);

        GameObject currentCharObject = GameObject.Find(currentTrialLine.Speaker.FirstName);
        //For Above: May make a singleton that has the reference of all the characters

        MeshRenderer mesh = currentCharObject.GetComponentInChildren<MeshRenderer>();

        currentActor = currentCharObject.GetComponentInChildren<Actor>();
        currentChar = currentTrialLine.Speaker;

        #region Changing Nameplate and Portrait

        if (CGPlayer.instance != null && CGPlayer.inCG)
            SetNameplate(currentTrialLine);
        else
            StartCoroutine(ChangeCharacterAnim(currentTrialLine));
        #endregion

        string anim = animNames[currentTrialLine.camAnimIdx];

        if (currentTrialLine.camAnimIdx != 10)
        {
            cameraa.TriggerAnim(anim);
        }

        if (currentTrialLine.Expression.Sprite != null)
        {
            mesh.material.mainTexture = currentTrialLine.Expression.Sprite.mainTexture;
            currentActor.characterb.material.mainTexture = currentTrialLine.Expression.Sprite.mainTexture;
        }

        if (!uiAnimator.GetBool("ShowTDUI"))
        {
            uiAnimator.SetBool("ShowTDUI", true);
            //yield return new WaitForSeconds(1);
        }

        #region Audio

        if (currentTrialLine.SFX.Count != 0)
            SoundManager.instance.PlaySFX(currentTrialLine.SFX[0]);

        if (currentTrialLine.VoiceSFX != null)
            SoundManager.instance.PlayVoiceLine(currentTrialLine.VoiceSFX);

        #endregion

        #region Displaying Text
        if (currentTrialLine.Text == "")
        {
            if (!ffMode && currentTrialLine.AutomaticLine)
            {
                //yield return new WaitForSeconds(currentTrialLine.TimeToNextLine);
            }
            return;
        }
        else // Normal Display Text
        {
            DialogueTextConfig.instance.StopAllCoroutines();
            DialogueTextConfig.instance.DisplayText(currentTrialLine);
            DialogueTextConfig.instance.isWaitingForClick = true;
        }

        #endregion

    }
    #endregion

    #region Tutorial Options
    void TutorialEvaluation(object o = null)
    {
        if (HasParameter(textBoxOnlyBoolString, uiAnimator))
            uiAnimator.SetBool(textBoxOnlyBoolString, o != null);
        else
            Debug.LogWarning("COULDN'T FIND PARAMETER FOR TUT EVAL");
    }
    public static bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
    #endregion
}
