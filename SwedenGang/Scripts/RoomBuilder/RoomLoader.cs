//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DG.Tweening;
using DREditor.Camera;
using DREditor.FPC;
using DREditor.EventObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System;
using UnityEngine.EventSystems;
using System.Linq;
using DREditor.Dialogues;
using DREditor.Gates;
/// <summary>
/// Singleton that Loads areas.
/// Requires Global Fade.
/// </summary>
public class RoomLoader : MonoBehaviour
{
    public static RoomLoader instance = null;
    public BoolWithEvent inDialogue = null;
    public BoolWithEvent inMenu = null;
    public BoolWithEvent inTPFD = null;
    public BoolWithEvent inLoading = null;

    [Header("Debugging Tools (Hover for Tooltips)")]
    [Tooltip("For Testing OnLoad and EndLoad Event Calls (Mostly for Actuators and Subsequents)")]
    [SerializeField] bool DebugTestOnLoad = false;
    [Tooltip("For Testing going from one area to another")]
    [SerializeField] bool DebugMoveArea = false;
    [Tooltip("For Testing coming from a gate and spawning the player \n " +
        "Note: Requires DebugToGate Filled")]
    [SerializeField] bool DebugLoadFromGate = false;
    [Tooltip("To load a room at the current progression stated in the Progression Manager")]
    [SerializeField] bool DebugLoadRoomAtProgression = false;
    [SerializeField] Gate debugToGate = null;
    [SerializeField] Gate debugFromGate = null;

    //[Header("Audio")]
    //[SerializeField][EventRef] string leaveRoomSound = "";

    Gate ToGate;
    Vector3 StartPosition;
    Vector3 StartRotation;
    //string FromArea;
    public void SetToGate(Gate g) => ToGate = g;

    public delegate void OnLoadDelegate();
    public static event OnLoadDelegate StartLoad;
    public static event OnLoadDelegate OnLoad;
    public static event OnLoadDelegate PreEndLoad;
    public static event OnLoadDelegate EndLoad;

    public static bool skipActivateControls = false;

    Stopwatch timer = new Stopwatch();

    private GameObject player;
    private SmoothMouseLook cam;
    private CameraBehaviour beh;
    private GameObject system;
    //private ControlMonobehaviours control;

    private LoadRoomOptions options;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(this);
    }
    private void Start()
    {

        GetAssets();
        if (DebugMoveArea || DebugTestOnLoad || DebugLoadFromGate)
        {
            RoomsCanLoad();
        }
        if (DebugTestOnLoad)
        {
            Debug.LogWarning("NOTIFY! DebugTestOnLoad is ON in RoomLoader. This should only be on to test" +
                " OnLoad Events/Triggers, otherwise it should be turned OFF");
            OnLoad?.Invoke();
            EndLoad?.Invoke();
        }
        if (DebugLoadFromGate)
        {
            if (debugToGate != null && debugFromGate != null)
                ToGate = debugToGate;
            else
                Debug.LogError("ERROR: DebugLoadFromGame is ON yet a DebugToGate or FromGate hasn't been filled!");
            GateFrom fromG = ToGate.GetFromLocation(debugFromGate);
            if (fromG != null)
            {
                StartPosition = fromG.position;
                StartRotation = fromG.rotation;
            }
            else
            {
                StartPosition = Vector3.zero;
                StartRotation = Vector3.zero;
            }
            if (player == null)
                GetAssets();

            cam.ClearCorePos();
            if (StartPosition != Vector3.zero)
            {
                Debug.Log("Player set to GateFrom Position");
                player.transform.position = StartPosition;

                player.transform.localEulerAngles = new Vector2(player.transform.localEulerAngles.x, StartRotation.y);
                SetCamCore();
                //cam.transform.localEulerAngles = new Vector2(player.transform.localEulerAngles.x, StartRotation.y);
                //cam.targetCharacterDirection = new Vector2(StartRotation.x, StartRotation.y);

                beh.CallLook();
            }
            else
            {
                Debug.Log("FromG null or position is zero");
                player.transform.position = StartPosition;
                player.transform.localEulerAngles = new Vector2(player.transform.localEulerAngles.x, StartRotation.y);
                cam.transform.localEulerAngles = new Vector2(player.transform.localEulerAngles.x, StartRotation.y);
                //cam.targetCharacterDirection = new Vector2(StartRotation.x, StartRotation.y);
            }
            //LoadRoom(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        if(DebugLoadRoomAtProgression)
            LoadRoom(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }
    void GetAssets()
    {
        try
        {
            player = GameObject.Find("Player");
            cam = GameObject.Find("Main Camera").GetComponent<SmoothMouseLook>();
            beh = cam.GetComponent<CameraBehaviour>();
            system = GameObject.Find("PlayerSystem");
            //control = system.GetComponent<ControlMonobehaviours>();
        }
        catch
        {

        }
    }
    public void RoomsCanLoad()
    {
        //Debug.LogWarning("Rooms Can Load Called");
        SceneManager.sceneLoaded += LoadRoom;
    }
    public void RoomsCannotLoad()
    {
        SceneManager.sceneLoaded -= LoadRoom;
    }
    public void SetCurrentRoomData(RoomInstanceManager.InstanceData data, Gate gate)
    {
        StartPosition = data.Position;
        StartRotation = data.Rotation;
        ToGate = gate;
    }
    public void ChangeRoom(Gate gate, LoadRoomOptions options = null)
    {
        StartCoroutine(RoomChange(gate, options));
    }
    IEnumerator RoomChange(Gate gate, LoadRoomOptions options = null)
    {
        this.options = options;
        if (player == null)
            GetAssets();

        StartLoad?.Invoke();
        SoundManager.instance.StopEnv();
        //PlayerManager.instance.EnableControlMono(false);
        EventSystem.current.SetSelectedGameObject(null);
        DialogueAnimConfig.instance.ShowMainUI(false);
        ItemDisplayer.instance.HideName();
        Gate progressionGate = null;
        if (ProgressionManager.instance != null)
        {
            progressionGate = ProgressionManager.instance.CurrentObjective.ProgressionGate;
        }
        //Debug.LogWarning("Checking Gate");
        //Debug.LogWarning(progressionGate);
        //Debug.LogWarning(progressionGate == gate || !(progressionGate != null));
        if (ProgressionManager.instance != null && ProgressionManager.instance.CheckObjective() && (progressionGate == gate || 
            !(progressionGate != null)))
        {
            Debug.Log("CLEARED INSTANCE DATA AND CHANGING OBJECTIVE");
            ProgressionManager.instance.ChangeObjective();
            RoomInstanceManager.instance.ClearRoomData();
        }
        else
        {
            try
            {
                RoomManager.RoomSaved = false;
                RoomInstanceManager.instance.SaveRoom(SceneManager.GetActiveScene().name);
            }
            catch
            {
                Debug.LogWarning("There is no RoomInstanceManager in the scene, " +
                    "or the Room could not be saved.");
            }
        }
        
        
        yield return new WaitForSeconds(0.5f); // Wait for UI to finish animating
        if (RoomManager.RoomSaved) 
            yield return new WaitUntil(() => RoomManager.RoomSaved);
        // on 7/2 I noticed that the above is supposed to have a ! operator, but it's working correctly so 
        // I don't want to touch it.
        RoomManager.RoomSaved = false;
        // Wait until inDialogue.Value = false;?
        
        if (!(options != null) || !options.blackTransition)
        {
            DialogueAnimConfig.instance.rt = new RenderTexture(Screen.width, Screen.height, 32);
            Camera.main.targetTexture = DialogueAnimConfig.instance.rt;
            DialogueAnimConfig.instance.freezeImage.texture = DialogueAnimConfig.instance.rt;
            Camera.main.Render();
            Camera.main.targetTexture = null;
            
            DialogueAnimConfig.instance.FreezeCanvas.enabled = true;
        }

        //yield return new WaitUntil(() => Camera.main.targetTexture == null);

        

        inLoading.Value = true;
        
        ToGate = gate;
        GateFrom fromG = ToGate.GetFromLocation(SceneManager.GetActiveScene().name);
        if (fromG != null)
        {
            StartPosition = fromG.position;
            StartRotation = fromG.rotation;
        }
        else
        {
            Debug.LogWarning("FromG was Null");
            StartPosition = Vector3.zero;
            StartRotation = Vector3.zero;
        }


        if (!(options != null) || !options.blackTransition)
        {
            //DialogueAnimConfig.instance.freezeImage.DOFade(0, 0);
            Color background = new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f));

            
            DialogueAnimConfig.instance.freezeImage.DOColor(background, 1);
            DialogueAnimConfig.instance.freezeBack.DOColor(background, 2);
            yield return new WaitUntil(() => DialogueAnimConfig.instance.freezeBack.color == background);
        }
        else if (options != null && options.blackTransition)
        {
            GlobalFade.instance.FadeTo(0.5f);
            yield return new WaitForSecondsRealtime(0.5f);
        }
        //SoundManager.instance.PlaySFX(leaveRoomSound);

        timer.Reset();
        timer.Start();

        AsyncOperation asyncOperation;
        // If a gate has a deadly life room then use it.
        if ((options != null && options.toInvestigation) || GameManager.instance.currentState == GameManager.State.Deadly)
        {
            asyncOperation = SceneManager.LoadSceneAsync(gate.GetInvestigationRoom(ProgressionManager.instance.GetChapterNum()));
        }
        else
        {
            asyncOperation = SceneManager.LoadSceneAsync(gate.toAreaName);
        }
        while (!asyncOperation.isDone)
        {
            print(asyncOperation.progress);
            yield return null;
        }


        
        yield break;
    }
    public static bool LoadingStart = false;
    void LoadRoom(Scene scene, LoadSceneMode mode) => StartCoroutine(FadeToRoom(scene, mode));
    IEnumerator FadeToRoom(Scene scene, LoadSceneMode mode)
    {
        if (LoadingStart)
            yield break;
        LoadingStart = true;
        GameManager.instance.SetLocation("");
        Debug.LogWarning("Scene " + scene.name + " Loading");
        // Stuff to load scene when level loaded
        timer.Stop();
        TimeSpan timeTaken = timer.Elapsed;
        string foo = "Time taken for unity to load scene: " + timeTaken.ToString(@"m\:ss\.fff");
        Debug.Log(foo);

        if (player == null)
        {
            player = GameObject.Find("Player");
            cam = GameObject.Find("Main Camera").GetComponent<SmoothMouseLook>();
            beh = cam.GetComponent<CameraBehaviour>();
            system = GameObject.Find("PlayerSystem");
            //control = system.GetComponent<ControlMonobehaviours>();
        }
        PlayerManager.instance.EnableScripts(false);
        PlayerManager.instance.EnableControlMono(false);
        if (GameManager.instance.currentMode == GameManager.Mode.Trial)
            DialogueAnimConfig.instance.ReticleCanvas.enabled = false;
        cam.ClearCorePos();
        RaycastReticle.Instance.ResetReticleVisual();
        if (StartPosition != Vector3.zero)
        {
            //Debug.LogWarning("Player set to GateFrom Position");
            player.transform.position = StartPosition;

            player.transform.localEulerAngles = new Vector2(player.transform.localEulerAngles.x, StartRotation.y);
            
            SetCamCore();
            if (GameSaver.LoadingFile)
                cam.SetCorePos(GameSaver.CurrentGameData.RoomData.MouseAbs, new Vector2(1.401298e-45f, 1.401298e-45f));
            beh.CallLook();
        }
        else
        {
            //Debug.LogWarning("FromG null or position is zero");
            player.transform.position = StartPosition;
            player.transform.localEulerAngles = new Vector2(player.transform.localEulerAngles.x, StartRotation.y);
            cam.SetCorePos(new Vector2(StartRotation.y, player.transform.localEulerAngles.x),
                    new Vector2(1.401298e-45f, 1.401298e-45f));
            //Debug.LogWarning("The player was moved");
        }
        // to gate is tpfd was here
        if (ToGate == null)
        {
            ToGate = Door.ADB.GetArea(SceneManager.GetActiveScene().name);
        }
        
        GateIsTPFD();

        yield return new WaitForSeconds(0.1f); // Buffer time for the position 

        // if there's no Room Builder it'll just load the scene shell
        if (RoomManager.instance && GameManager.instance.currentMode != GameManager.Mode.Trial) 
        {
            Debug.Log("Loading Room Instance Managers LoadRoom with Mode at: " + GameManager.instance.currentMode);
            RoomInstanceManager.instance.LoadRoom(SceneManager.GetActiveScene().name);
            yield return new WaitUntil(() => RoomManager.RoomLoaded);
            Debug.Log("Finished Room Instance Managers LoadRoom");
            RoomManager.RoomLoaded = false;
        }
        else if (GameSaver.LoadingFile)
        {
            ProgressionManager.instance.Load();
            Debug.Log("NOTIFY: Either there's no Room Data for this objective or " +
                "a Room Builder isn't in the scene. STANDBY: LOADING ROOM SHELL");
            /* This is here because when loading to a room that doesn't have a room manager
             * (Saving in a room with no room manager and then loading that file)
             * the game doesn't load the players progression data.
             */
        }
        else
        {
            Debug.Log("NOTIFY: Either there's no Room Data for this objective or " +
                "a Room Builder isn't in the scene. STANDBY: LOADING ROOM SHELL");
        }

        // For Loading Trial Data
        if (GameSaver.LoadingFile && TrialManager.instance != null && GameManager.instance.currentMode == GameManager.Mode.Trial)
        {
            TrialLoader.Load(GameSaver.CurrentGameData.TrialData);
        }
        if (GameSaver.LoadingFile)
        {
            DialogueAssetReader.Backlog.Load(GameSaver.CurrentGameData.BackData);
        }
        bool test = false;
        // Loading File that was in the middle of dialogue
        if (GameSaver.LoadingFile && GameSaver.CurrentGameData.DialogueData.DialogueName != "")
        {
            if (GameManager.instance.currentMode != GameManager.Mode.Trial)
            {
                var dialogueHolders = FindObjectsOfType<MonoBehaviour>().OfType<IDialogueHolder>();
                foreach (IDialogueHolder holder in dialogueHolders)
                {
                    holder.CheckIfSaveDialogue();
                }
            }
            else
            {
                TrialLoader.FindDialogue();
            }
            Debug.LogWarning("Started Looking for Dialogue: " + GameSaver.CurrentGameData.DialogueData.DialogueName);
            yield return new WaitUntil(() => DialogueData.FoundDialogue);
            DialogueData.FoundDialogue = false;
            Debug.LogWarning("Found Dialogue");
            test = true;
            if (GameManager.instance.currentMode != GameManager.Mode.Trial)
            {
                DialogueAssetReader.LoadDialogue();
                yield return new WaitUntil(() => DialogueAssetReader.DialogueLoaded);
                DialogueAssetReader.DialogueLoaded = false;
                
            }
            else
            {
                TrialDialogueManager.LoadDialogue();
                TrialLoader.LoadTrialAtDialogueIndex();
                yield return new WaitUntil(() => TrialDialogueManager.DialogueLoaded);
                TrialDialogueManager.DialogueLoaded = false;
                Debug.LogWarning("Loaded Trial Dialogue");
            }
            
        }

        

        
        yield return new WaitForSeconds(0.3f); // Buffer time for the position 
        if (GameManager.instance.currentMode != GameManager.Mode.Trial && !inDialogue.Value && !GameSaver.LoadingFile)
        {
            yield return new WaitForEndOfFrame();
            Color fBackground = DialogueAnimConfig.instance.freezeImage.color;
            fBackground.a = 0;
            DialogueAnimConfig.instance.freezeImage.DOColor(fBackground, 2);
            DialogueAnimConfig.instance.freezeBack.DOColor(fBackground, 1);
            Debug.Log("Starting Fading Room Color");
            yield return new WaitUntil(() => DialogueAnimConfig.instance.freezeImage.color.a == 0);
            Debug.Log("Finished Fading Room Color");
            yield return new WaitForSeconds(Time.deltaTime);
            DialogueAnimConfig.instance.FreezeCanvas.enabled = false;
            yield return new WaitForSeconds(Time.deltaTime);
            DialogueAnimConfig.instance.freezeImage.color = new Color(1, 1, 1, 1);

        }


        if (GameManager.instance.currentMode == GameManager.Mode.TPFD && TPFDAnimator.instance != null) // if tpfd animate room
        {
            Debug.LogWarning("Room Animation Started, waiting for it to finish");
            TPFDAnimator.instance.StartAnimation();
            TPFDAnimator.RoomAnimating = true;
            yield return new WaitUntil(() => !TPFDAnimator.RoomAnimating);
            Debug.LogWarning("Room Animation Finished");
        }
        if (GameManager.instance.currentMode == GameManager.Mode.TPFD && TPFDAnimator.instance != null)
            Debug.LogWarning("No TPFDAnimator found in room");

        if (GameManager.instance.currentMode != GameManager.Mode.Trial && (!inDialogue.Value || (!CGPlayer.inCG && !CGPlayer.inVid)))
        {
            Debug.Log("Showing UI Visuals");
            DialogueAnimConfig.instance.ShowMainUI(true); // Show UI Visuals
            DialogueAnimConfig.instance.reticleAnimator.transform.localPosition = Vector3.zero;
            if (GameSaver.LoadingFile && GameSaver.CurrentGameData.DialogueData.DialogueName != "")
                DialogueAnimConfig.instance.ReticleCanvas.enabled = false;
            else
                DialogueAnimConfig.instance.ReticleCanvas.enabled = true;
        }
        else if(GameManager.instance.currentMode == GameManager.Mode.Trial)
        {
            DialogueAnimConfig.instance.EnableReticleCanvas(false);
        }

        

        

        OnLoad?.Invoke();
        Debug.Log("No longer loading");
        GateIsTPFD();
        if (GameSaver.LoadingFile)
        {
            SoundManager.instance.LoadMusic(GameSaver.CurrentGameData.MusicData);
            SoundManager.instance.LoadEnv(GameSaver.CurrentGameData.EnvData);
            cam.GetComponent<AudioListener>().enabled = true;
            DialogueAnimConfig.instance.mainCanvas.enabled = true;
            
            if (GlobalFade.instance.IsDark)
            {
                GlobalFade.instance.FadeOut(0.3f);
                yield return new WaitForSeconds(0.3f);
            }
            /*if (GameManager.instance.currentMode == GameManager.Mode.TPFD)
            {
                control.TPFDScript.enabled = true;
                yield return new WaitForSeconds(1f);
            }*/
        }
        inTPFD.Invoke();
        cam.GetComponent<AudioListener>().enabled = true;

        if (GameManager.instance.currentMode != GameManager.Mode.Trial)
        {
            DialogueAnimConfig.instance.mainCanvas.enabled = true;
            float wait = 0;
            try
            {
                wait = DialogueAnimConfig.instance.mainAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
                
            }
            catch
            {

            }
            yield return new WaitForSecondsRealtime(wait);
        }
        if (ToGate != null)
        {
            if (ToGate.isTPFD)
            {
                GameManager.instance.ChangeMode(GameManager.Mode.TPFD);
                Debug.LogWarning("Changing Mode to TPFD");
            }
            else if(!GameSaver.LoadingFile)
            {
                GameManager.instance.ChangeMode(GameManager.Mode.ThreeD);
                SubAreaManager m = FindObjectOfType<SubAreaManager>();
                if (!(m != null && m.Data.activated))
                    PlayerManager.instance.EnableMonoScripts(true);
            }
        }
        if (GameSaver.LoadingFile && GameManager.instance.currentMode == GameManager.Mode.ThreeD)
        {
            SubAreaManager m = FindObjectOfType<SubAreaManager>();
            if (!(m != null && m.Data.activated))
                PlayerManager.instance.EnableMonoScripts(true);
        }
        LoadingStart = false;
        inLoading.Value = false; // Player gets control back
        Door.inLeaveProcess = false;
        inMenu.Value = false;
        
        if (GameSaver.LoadingFile && GameSaver.CurrentGameData.DialogueData.DialogueName == "")
        {
            if (!(GameManager.instance.currentMode == GameManager.Mode.Trial))
                GameManager.instance.cantBeInMenu = false;
            if (!inTPFD.Value)
            {
                MovePlayer m = player.GetComponent<MovePlayer>();
                m.enabled = true;
            }
        }

        if (!PlayerManager.instance.GetControl() && !(GameSaver.LoadingFile && GameManager.instance.currentMode == GameManager.Mode.Trial)
            && !skipActivateControls)
        {
            Debug.LogWarning("Activated Control");
            PlayerManager.instance.EnableControlMono(true);

        }
        if (GameSaver.LoadingFile && test && !inDialogue.Value)
        {
            Debug.LogWarning("In Dialogue set to True from ");
            inDialogue.Value = true;
        }
        if (GameSaver.LoadingFile)
            GameSaver.LoadingFile = false;
        Debug.LogWarning("REACHED END OF ROOM LOADER");
        PreEndLoad?.Invoke();
        if (!inDialogue.Value)
        {
            Debug.Log("Can Select is True on End of Room Loader");
            RaycastReticle.canSelect = true;
            EndLoad?.Invoke();
        }
        else
        {
            Debug.LogWarning("End of RoomLoader with In Dialogue True");
        }
        if (!inDialogue.Value)
        {
            if (GameManager.instance.currentMode == GameManager.Mode.TPFD)
                GameManager.SetControls("tpfd");
            if (GameManager.instance.currentMode == GameManager.Mode.ThreeD)
                GameManager.SetControls("threed");
        }
        ToGate = null;
        yield break;
    }
    void SetCamCore()
    {
        cam.SetCorePos(new Vector2(StartRotation.y, player.transform.localEulerAngles.x),
                    new Vector2(1.401298e-45f, 1.401298e-45f));
    }
    void GateIsTPFD()
    {
        if (ToGate != null)
        {
            if (ToGate.isTPFD)
            {
                if (GameManager.instance.currentMode != GameManager.Mode.TPFD)
                    GameManager.instance.ChangeMode(GameManager.Mode.TPFD);
                Debug.LogWarning("Setting Camera to TPFD Position");
                TPFDManager man = FindObjectOfType<TPFDManager>();
                man.StartEarly();
                PlayerManager.instance.TPFDControllable(true);
                PnCCamera pnc = PlayerManager.instance.GetTPFD();
                PlayerManager.instance.mainCamera.transform.rotation = pnc.GetCalculatedRotation();

                
                if (man.GetInitialHAngle() >= 180)
                    PlayerManager.instance.mainCamera.transform.position = pnc.GetCalculatedPosition();
                else
                    PlayerManager.instance.mainCamera.transform.position = pnc.GetCalculatedPositionZ();
            }
        }
    }
}
/// <summary>
/// Found in RoomLoader.cs, tells room loader how to load the room
/// </summary>
[Serializable]
public class LoadRoomOptions
{
    public bool toInvestigation = false;
    public bool blackTransition = false;
    public object Clone()
    {
        LoadRoomOptions o = new LoadRoomOptions();
        o.toInvestigation = toInvestigation;
        o.blackTransition = blackTransition;
        return o;
    }
}
