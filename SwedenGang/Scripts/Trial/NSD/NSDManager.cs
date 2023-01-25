//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Camera;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSD;
using DREditor.TrialEditor;
using DREditor.Characters;
using UnityEngine.Rendering;
using DG.Tweening;
using DREditor.PlayerInfo;
using UnityEngine.Video;
using DREditor.Dialogues;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using DREditor.EventObjects;

public class NSDManager : MinigameManagerBase
{
    public static NSDManager instance = null;
    [SerializeField] TrialCameraAnimDatabase tCAD = null;
    [SerializeField] BoolWithEvent InDialogue = null;
    [SerializeField] BoolWithEvent InMenu = null;
    private List<string> animNames => tCAD.GetNames();
    [HideInInspector] public DRTrialCamera trialCamera = null;
    Camera cam;
    [SerializeField] Camera ShatterCamera = null;
    [SerializeField] TruthBulletMenu truthMenu = null;
    public bool lieMode = false;
    [SerializeField] bool debugMode = false;
    [SerializeField] bool skipEarly = false;
    public NSDBuilder debate = null;
    public List<PhraseText> phraseTexts = new List<PhraseText>();
    public List<WhiteNoiseText> wnTexts = new List<WhiteNoiseText>();

    [HideInInspector] public TruthBullet currentBullet;
    int bulletNumber = 0;
    NSDBullet firedBullet => GetComponentInChildren<NSDBullet>();
    [HideInInspector] public bool canFire = true;
    [HideInInspector] public bool canEffect = true;
    [HideInInspector] public Vector2 firePosition;
    [SerializeField] float fireOffsetX = -204;
    [SerializeField] float fireOffsetY = -2;
    [HideInInspector] public Vector3 mousePos;

    [Header("Bullet Load Animation")]
    [SerializeField] RawImage loadChamber = null;
    [SerializeField] RawImage loadChamberShadow = null;
    [SerializeField] List<GameObject> BulletObjects = new List<GameObject>();
    [SerializeField] float bulletLoadSpeed = 0.5f;
    [SerializeField] float bloX;
    [SerializeField] float bloY;
    [SerializeField] GameObject BulletLoadUI = null;

    [Header("Main NSD UI")]
    [SerializeField] GameObject MainNSDUI = null;
    [SerializeField] RectTransform BulletMask = null;
    [SerializeField] TMP_Text BulletText = null; // public for empathy check
    [SerializeField] List<RawImage> indicators = new List<RawImage>();
    [SerializeField] Texture indicatorOff = null;
    [SerializeField] Texture indicatorOn = null;
    [SerializeField] RectTransform mainChamber = null;
    [SerializeField] RectTransform mainChamberShadow = null;
    [SerializeField] Animator ringAnimation = null;

    [Header("NSD UI")]
    [SerializeField] RawImage NSDTextTex = null;

    //[Header("Health and Influence Handler")]
    //public NSDStats debateStats = null;

    [Header("Bullet Selection")]
    [SerializeField] List<RawImage> SelectBullets = new List<RawImage>();
    [SerializeField] List<TMP_Text> SelectBulletsTexts = new List<TMP_Text>();
    [SerializeField] GameObject SelectionChamber = null;
    [SerializeField] Texture ActiveBullet = null;
    [SerializeField] Texture InactiveBullet = null;
    [SerializeField] RawImage SelectionImage = null;
    [SerializeField] Animator SelectionPieces = null;
    //[SerializeField] KeyCode SelectKey = KeyCode.Q;
    bool canSelect = true;

    

    [Header("Effects")]
    [SerializeField] Animator openingAnimator = null;
    [SerializeField] RawImage openingBlackFade = null;
    [SerializeField] Volume volume = null;
    Camera textCamera => GameObject.Find("NSD Text Camera").GetComponent<Camera>();
    [SerializeField] CharacterDatabase cdb = null;
    List<Actor> actors = new List<Actor>();
    bool inDebate = false;
    [SerializeField] float slowSpeed = 0.25f;
    bool slow = false;
    [SerializeField] Volume slowVolume = null;
    [SerializeField] float fastSpeed = 1.5f;
    bool fast = false;
    [SerializeField] Volume fastVolume = null;

    [Header("Timer")]
    [SerializeField] RawImage timerImage = null;
    public TrialTimer timer => GetComponentInChildren<TrialTimer>();

    [Header("Counter Animation")]
    public RawImage counterTexture = null;
    public VideoPlayer counter => GetComponentInChildren<VideoPlayer>();
    [SerializeField] Canvas ansCanvas = null;
    [SerializeField] Animator ansAnimator = null;
    [SerializeField] AnimationClip animClip = null;

    [Header("Consent Animation")]
    [SerializeField] Animator consentAnimator = null;
    [SerializeField] string consentAnimName = "IAgree";
    [SerializeField] float consentAnimLength = 3.1f;
    [SerializeField] RawImage consentImage = null;
    [SerializeField] Texture2D[] consentTex;

    [Header("Panel Counter")]
    [SerializeField] RawImage panelCounter = null;
    [SerializeField] RawImage panelPortrait = null;
    [SerializeField] RawImage emptyPortrait = null;
    [SerializeField] RawImage[] panelCounters = new RawImage[20];
    [SerializeField] Texture notTalking = null;
    [SerializeField] Texture talking = null;
    [SerializeField] TMP_Text panelSpeaker = null;

    [Header("Audio")]
    [SerializeField] AudioSource ffEmit = null;
    [SerializeField] AudioSource slowEmit = null;
    [SerializeField] AudioSource empHitEmit = null;
    [SerializeField] AudioClip Music;
    [SerializeField] AudioClip NSDStartSFX;
    [SerializeField] AudioClip BulletLoad;
    [SerializeField] AudioClip BulletFire;
    [SerializeField] AudioClip FireSilencedBulletSFX;
    [SerializeField] public AudioClip HitEmpathySFX;
    [SerializeField] public AudioClip HitIncorrectSFX;
    [SerializeField] public AudioClip HitWNOSFX;
    [SerializeField] AudioClip HitWNXSFX;
    [SerializeField] public AudioClip BulletHitSFX;
    [SerializeField] public AudioClip CounterSound;
    [SerializeField] public AudioClip ProtagCounterSound;
    [SerializeField] public AudioClip ProtagConsentSound;
    [SerializeField] public AudioClip BreakSound;
    [SerializeField] AudioClip BulletChangeSound;
    [SerializeField] AudioClip BulletSelectInSFX;
    [SerializeField] AudioClip BulletSelectOutSFX;

    List<TruthBullet> TruthBullets = new List<TruthBullet>();

    

#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        trialCamera = FindObjectOfType<DRTrialCamera>();
        cam = trialCamera.GetComponentInChildren<Camera>();
        GetActors();
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
    }

    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Enable();
#endif
        TrialDialogueManager.EndFU += RestartPanels;
        TrialTimer.TimeUp += TimeUp;
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Disable();
#endif
        TrialDialogueManager.EndFU -= RestartPanels;
        TrialTimer.TimeUp -= TimeUp;
        DeActivateControls();
    }
    List<GameObject> actorObjects = new List<GameObject>();
    void GetActors()
    {
        foreach (Character character in cdb.Characters)
        {
            if (GameObject.Find(character.FirstName))
            {
                GameObject charac = GameObject.Find(character.FirstName);
                actors.Add(charac.GetComponentInChildren<Actor>());
                actorObjects.Add(charac);
            }
        }
    }

    /*void TurnOffCutOut()
    {
        foreach(Actor actor in actors)
        {
            actor.characterb.enabled = false;
        }
    }*/

    private void Start()
    {
        if (debate && debugMode)
        {
            StartDebate(debate);
            //TurnOffCutOut();
        }
        //Application.targetFrameRate = 15;
    }

    private void Update()
    {
        if (inDebate)
        {
            CamUpdate();
            

            if (canEffect)
                SpeedChange();
        }
    }

    #region Interactables
    void CamUpdate()
    {
        textCamera.transform.position = cam.transform.position;
        textCamera.transform.rotation = cam.transform.rotation;
        textCamera.fieldOfView = cam.fieldOfView;
        ShatterCamera.transform.position = cam.transform.position;
        ShatterCamera.transform.rotation = cam.transform.rotation;
    }
    bool canCycle = false;
    bool changingBullet = false;
    void ChangeBullet(CallbackContext context)
    {
        if (TruthBullets.Count == 1)
            return;
        //Debug.Log("Called Change Bullet");
        
        if (!changingBullet && (canFire || InCycle && canCycle))
        {
            //Debug.Log(_controls.Minigame.Change.ReadValue<Vector2>());
            if (_controls.Minigame.Change.ReadValue<Vector2>().y > 0.5)
            {
                //Debug.Log(_controls.Minigame.Change.ReadValue<Vector2>().y);
                CantFire();
                changingBullet = true;
                SwapBullet(true, false);
            }
            else if (_controls.Minigame.Change.ReadValue<Vector2>().y < -0.5)
            {
                CantFire();
                changingBullet = true;
                SwapBullet(false, false);
            }
        }
    }
    IEnumerator ChangeBulletAnim(bool inSelect)
    {
        
        currentBullet = TruthBullets[bulletNumber];
        float duration = 0.1f;
        BulletMask.DOSizeDelta(new Vector2(90, BulletMask.sizeDelta.y), duration)
            .SetUpdate(true);
        PlaySFX(BulletChangeSound);
        //PlaySFX(BulletSelectInSFX);

        yield return new WaitForSecondsRealtime(duration);
        BulletText.text = currentBullet.Title;
        BulletMask.DOSizeDelta(new Vector2(1050, BulletMask.sizeDelta.y), duration)
            .SetUpdate(true);
        //PlaySFX(BulletSelectOutSFX);
        yield return new WaitForSecondsRealtime(duration);
        if (!inSelect)
            canFire = true;
        canSelect = true;
        //if (InCycle)
            //canCycle = true;
        yield break;
    }
    [SerializeField] float selectionDuration = 0.5f;

    #region Cycle Through Bullets
    
    bool InCycle = false;
    bool cyclePass = false;
    void StartCycle(CallbackContext context)
    {
        //Debug.Log("pass: " + cyclePass);
        if (TruthBullets.Count == 1 || !cyclePass || InMenu.Value)
            return;
        PlaySFX(BulletSelectInSFX);
        NSDReticle.Instance.ShowReticleOverride(false);
        InCycle = true;
        CantFire();
        for (int i = 0; i < SelectBullets.Count; i++)
        {
            SelectBullets[i].DOFade(1, 0);
        }

        MainNSDUI.transform.DOMoveX(-100, selectionDuration)
            .SetUpdate(true);

        SelectionPieces.SetBool("Toggle", true);
        canCycle = true;
        //PlayerInput.actions["Navigation"].performed += ChangeBulletOver;
    }
    void EndCycle(CallbackContext context)
    {
        if (TruthBullets.Count == 1 || !cyclePass || InMenu.Value)
            return;
        PlaySFX(BulletSelectOutSFX);
        NSDReticle.Instance.ShowReticleOverride(true);
        MainNSDUI.transform.DOMoveX(0, selectionDuration)
                .SetUpdate(true);

        SelectionPieces.SetBool("Toggle", false);

        for (int i = 0; i < SelectBullets.Count; i++)
        {
            SelectBullets[i].DOFade(1, 0)
                .SetUpdate(true)
                .SetDelay(0.25f);
        }

        //PlayerInput.actions["Navigation"].performed -= ChangeBulletOver;
        canFire = true;
        InCycle = false;
        canCycle = false;
    }
    #endregion

    void SwapBullet(bool isUp, bool inSelect)
    {
        if (TruthBullets.Count == 1)
            return;
        StartCoroutine(SwapRoutine(isUp, inSelect));
    }
    IEnumerator SwapRoutine(bool isUp, bool inSelect)
    {
        
        if (InCycle)
            canCycle = false;
        indicators[bulletNumber].texture = indicatorOff;
        SelectBullets[bulletNumber].texture = InactiveBullet;

        BulletMove(false);

        if (isUp)
        {
            bulletNumber -= 1;
            if (bulletNumber == -1)
                bulletNumber = TruthBullets.Count - 1;
        }
        else
        {
            bulletNumber += 1;
            if (bulletNumber == TruthBullets.Count)
                bulletNumber = 0;
        }


        BulletMove(true);

        indicators[bulletNumber].texture = indicatorOn;
        if (isUp)
            RotateSelectionChamber(true);
        else
            RotateSelectionChamber(false);
        yield return StartCoroutine(ChangeBulletAnim(inSelect));

        
        if (InCycle)
            canCycle = true;
        changingBullet = false;
        yield break;
    }
    void BulletMove(bool isForward)
    {
        Vector3 pos = SelectBullets[bulletNumber].transform.localPosition;
        if (isForward)
            SelectBullets[bulletNumber].transform.DOLocalMove(new Vector3(pos.x + 75, pos.y + 20, 0), 0.1f)
                .SetUpdate(true);
        else
            SelectBullets[bulletNumber].transform.DOLocalMove(new Vector3(pos.x - 75, pos.y - 20, 0), 0.1f)
                .SetUpdate(true);
    }
    void RotateSelectionChamber(bool isUp)
    {
        Vector3 rot = new Vector3(mainChamber.rotation.eulerAngles.x, mainChamber.rotation.eulerAngles.y,
            mainChamber.rotation.eulerAngles.z);

        if (isUp)
            rot.z += 60;
        else
            rot.z -= 60;

        mainChamber.DORotateQuaternion(Quaternion.Euler(rot), 0.3f)
            .SetUpdate(true);
        mainChamberShadow.DORotateQuaternion(Quaternion.Euler(rot), 0.3f)
            .SetUpdate(true);

        SelectBullets[bulletNumber].texture = ActiveBullet;
        SelectionImage.texture = TruthBullets[bulletNumber].Picture.texture;

        Vector3 sel = new Vector3(SelectionChamber.transform.rotation.eulerAngles.x,
            SelectionChamber.transform.rotation.eulerAngles.y,
            SelectionChamber.transform.rotation.eulerAngles.z);

        if (isUp)
            sel.z += 60;
        else
            sel.z -= 60;

        SelectionChamber.transform.DORotateQuaternion(Quaternion.Euler(sel), 0.3f)
            .SetUpdate(true);
        SelectionChamber.transform.DORotateQuaternion(Quaternion.Euler(sel), 0.3f)
            .SetUpdate(true);
    }
    void InitializeSelectionBullets()
    {
        for (int i = 0; i < 6; i++)
        {
            SelectBulletsTexts[i].text = "";
        }

        for (int i = 0; i < TruthBullets.Count; i++)
        {
            SelectBulletsTexts[i].text = TruthBullets[i].Title;
        }
    }

    

    void FadeBulletIndicators(float to, float duration)
    {
        for(int i = 0; i < TruthBullets.Count; i++)
        {
            indicators[i].DOFade(to, duration)
                .SetUpdate(true);
        }
    }
    void FireTruthBullet(CallbackContext context)
    {
        if (canFire && !InCycle)
        {
            canFire = false;
            canSelect = false;
            cyclePass = false;
            string bulletString;
            if (lieMode)
                bulletString = currentBullet.LieTitle;
            else
                bulletString = currentBullet.Title;

            mousePos = NSDReticle.Position;
            firePosition = new Vector3(NSDReticle.Position.x * (float)decimal.Divide(8, Screen.width) + fireOffsetX,
                NSDReticle.Position.y * (float)decimal.Divide(4, Screen.height) + fireOffsetY, 0);
            firedBullet.FireBullet(firePosition, bulletString);
            PlaySFX(BulletFire);


            BulletMask.DOAnchorPos(new Vector2(BulletMask.anchoredPosition.x + 1920, BulletMask.anchoredPosition.y + 100), 0.5f)
                .SetUpdate(true);

            NSDReticle.ShowOrHide();
            CantFire();
        }
    }
    void FireSilencer(CallbackContext context)
    {
        if (canFire)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.ScreenPointToRay(NSDReticle.Position), out hit))
            {
                if (hit.transform.gameObject.GetComponent<PhraseText>() != null)
                {
                    timer.RemoveTime(3);
                }
                if (hit.transform.gameObject.GetComponent<WhiteNoiseText>() != null)
                {
                    timer.AddTime(5);
                    PlaySFX(FireSilencedBulletSFX);
                    hit.transform.gameObject.GetComponent<WhiteNoiseText>().ShatterWhiteNoise(hit.point);
                }
                else
                {
                    PlaySFX(HitWNXSFX);
                }
            }
        }
    }
    public void CanFire()
    {
        StartCoroutine(Reload());
        //Cursor.lockState = CursorLockMode.None;
    }
    IEnumerator Reload()
    {
        Vector2 pos = new Vector2(-838.4f, -437.2f);
        FadeBulletIndicators(1, 0.3f);
        BulletText.text = TruthBullets[bulletNumber].Title;
        

        
        BulletMask.DOAnchorPos(pos, 0.5f)
            .SetUpdate(true)
            .SetDelay(0.2f);
        pos.x -= 1920;
        pos.y -= 100;
        BulletMask.DOAnchorPos(pos, 0)
            .SetUpdate(true);
        yield return new WaitForSecondsRealtime(0.5f);

        
        if(!InDialogue.Value)
            NSDReticle.ShowOrHide();
        canFire = true;
        canSelect = true;
        cyclePass = true;
        yield break;
    }
    public void CantFire()
    {
        canFire = false;
        //Cursor.lockState = CursorLockMode.Confined;
    }
    #endregion

    #region Speed Change
    void SpeedChange()
    {
        if (PlayerInfo.instance.CurrentStamina <= 0)
        {
            RemoveSlow();
        }

        if (slow && PlayerInfo.instance.CurrentStamina > 0)
        {
            DrainStamina();
            if (TrialStats.Instance != null)
                TrialStats.Instance.DrainStaminaVisual();
            //Debug.Log(PlayerInfo.instance.CurrentStamina);
        }

        if (!slow && PlayerInfo.instance.CurrentStamina <= PlayerInfo.instance.MaxStamina)
        {
            RegenStamina();
            if (TrialStats.Instance != null)
                TrialStats.Instance.RegenStaminaVisual();
            //Debug.Log("Raising: " + PlayerInfo.instance.CurrentStamina);
        }
        if (PlayerInfo.instance.CurrentStamina == PlayerInfo.instance.MaxStamina && TrialStats.Instance != null && TrialStats.Instance.StaminaVisualWrong())
        {
            TrialStats.Instance.RegenStaminaVisual();
        }
    }
    bool velocityCanChange = false;
    void StartVelocity(CallbackContext context)
    {
        if (velocityCanChange && !slow && !fast && !InCycle)
        {
            
            fast = true;
            fastVolume.enabled = true;
            Time.timeScale = fastSpeed;
            DOTween.timeScale = fastSpeed;
            ffEmit.Play();
        }
    }
    void EndVelocity(CallbackContext context)
    {
        if (fast)
        {
            
            fast = false;
            fastVolume.enabled = false;
            Time.timeScale = 1;
            DOTween.timeScale = 1;
            ffEmit.Stop();
        }
    }
    void StartSpeed(CallbackContext context)
    {
        if (PlayerInfo.instance.CurrentStamina > 0 && !fast && !slow && !InCycle)
        {
            
            slow = true;
            volume.enabled = false;
            slowVolume.enabled = true;
            Time.timeScale = slowSpeed;
            slowEmit.Play();
        }
    }
    void EndSpeed(CallbackContext context)
    {
        if (slow)
        {
            RemoveSlow();
        }
    }
    public void RemoveSlow()
    {
        
        fast = false;
        fastVolume.enabled = false;
        slow = false;
        slowVolume.enabled = false;
        volume.enabled = true;
        Time.timeScale = 1;
        DOTween.timeScale = 1;
        slowEmit.Stop();
    }
    #endregion

    #region Main Manager
    public void PlayNSD(ScriptableObject asset)
    {
        NSDManager.instance.StartDebate((NSDBuilder)asset);
    }
    public void StartDebate(NSDBuilder asset)
    {
        StartCoroutine(PlayDebate(asset));
    }
    AnimationClip GetClip(string clipName)
    {
        AnimationClip[] clips = trialCamera.CameraAnimator.runtimeAnimatorController.animationClips;
        foreach(AnimationClip clip in clips)
        {
            if (clip.name == clipName)
                return clip;
        }
        Debug.LogError("Animation Clip " + clipName + " was Not found! Check the DRTrialCamera Animator and make sure it's there!");
        return null;
    }
    void OnPlayerDeath()
    {
        //volume.enabled = false;
        //SoundManager.instance.StopSound();
    }
    List<TruthBullet> SetBulletsFromDiff(NSDBuilder asset)
    {
        switch (GameManager.instance.actionDifficulty)
        {
            case GameManager.Difficulty.Kind:
                return asset.TruthBulletsKind;
            case GameManager.Difficulty.Normal:
                return asset.TruthBulletsNormal;
            case GameManager.Difficulty.Mean:
                return asset.TruthBulletsMean;
            default:
                return asset.TruthBulletsKind;
        }
    }
    IEnumerator PlayDebate(NSDBuilder asset) // Debate Opening
    {

        TruthBullets = SetBulletsFromDiff(asset);

        

        if(TruthBullets.Count == 0)
        {
            Debug.LogError("NSD Asset Must have Bullets for the current difficulty set in the " +
                "GameManager!");
            yield break;
        }
        Cursor.lockState = CursorLockMode.Confined;
        if (skipEarly)
        {
            Time.timeScale = 5;
            DOTween.timeScale = 5;
        }
        TrialDialogueManager.PlayerHasDied += OnPlayerDeath;
        trialCamera.TriggerAnim("NSD-transition");
        if (asset.StartingMusic == null)
            PlayMusic(Music);
        else
            PlayMusic(asset.StartingMusic);
        openingBlackFade.DOColor(new Color32(0, 0, 0, 255), 1);
        yield return new WaitForSeconds(GetClip("NSD-transition").length/1.5f);
        
        //yield return new WaitForSeconds(1);
        volume.enabled = true;
        openingBlackFade.DOColor(new Color32(0, 0, 0, 0), 0.5f);
        trialCamera.TriggerAnim("NSD-intro");
        openingAnimator.SetTrigger("StartIntro");
        yield return new WaitForSeconds(0.5f);
        PlaySFX(NSDStartSFX);
        
        yield return new WaitForSeconds(4);
        // Animate the NSD UI to start loading the bullets
        debate = asset;

        BulletLoadUI.transform.DOMoveX(0, 0);
        loadChamberShadow.DOFade(1, 0.3f);
        loadChamber.DOFade(1, 0.3f);
        yield return new WaitForSeconds(0.3f);
        
        Vector3 rot = new Vector3(loadChamber.rectTransform.rotation.eulerAngles.x, loadChamber.rectTransform.rotation.eulerAngles.y,
            loadChamber.rectTransform.rotation.eulerAngles.z);

        int sb = 0;
        float yOffset = 0;
        if (TruthBullets.Count % 2 != 0)
        {
            for (int i = 0; i < BulletObjects.Count; i++)
            {
                Vector3 p = BulletObjects[i].transform.position;
                p.y -= 8;
                BulletObjects[i].transform.position = p;
            }
        }

        if (TruthBullets.Count <= 2)
        {
            sb = 2;
        }
        else if(TruthBullets.Count == 3 || TruthBullets.Count == 4)
        {
            sb = 1;
        }

        if (TruthBullets.Count > 6)
        {
            Debug.LogError("There can be a min of 1 and a max of 6 bullets!");
        }

        for(int i = 0; i < 6; i++)
        {
            SelectBullets[i].enabled = false;
        }
        for(int i = 0; i < TruthBullets.Count; i++)
        {
            SelectBullets[i].enabled = true;
        }
        for (int i = 0; i < TruthBullets.Count; i++) // Reset Particle Animation
        {
            BulletObjects[i + sb].transform.Find("Particle").GetComponent<ParticleSystem>().Play();
            BulletObjects[i + sb].transform.Find("Burst").GetComponent<RawImage>().DOFade(1, 0);
        }
        //yield return new WaitForSeconds(0.5f);
        Vector3[] bulletOGPos = new Vector3[TruthBullets.Count]; // To reset position of loaded bullets
        for (int i = 0; i < TruthBullets.Count; i++) // Load Bullet Animation
        {
            
            
            TMP_Text bText = BulletObjects[i + sb].GetComponentInChildren<TMP_Text>();
            RectTransform rect = BulletObjects[i + sb].GetComponent<RectTransform>();
            Vector3 p = rect.position;
            bulletOGPos[i] = p;
            bText.text = TruthBullets[i].Title;
            if (bText.text.Length > 20)
            {
                bText.alignment = TextAlignmentOptions.Left;
                yield return new WaitForSeconds(Time.deltaTime);
                bText.autoSizeTextContainer = true;
                Vector2 set = rect.sizeDelta;
                set.x += (10 * (bText.text.Length - 20)) + 50;
                rect.sizeDelta = set;
                p.x += (bText.text.Length - 20)/2;
                rect.position = p;
            }
            


            BulletObjects[i + sb].transform.DOMove(new Vector3(p.x + bloX, p.y + bloY + yOffset, p.z), bulletLoadSpeed);
            rot.z += 66.1f;
            loadChamber.transform.DORotateQuaternion(Quaternion.Euler(rot.x, rot.y,
                rot.z), bulletLoadSpeed);
            loadChamberShadow.transform.DORotateQuaternion(Quaternion.Euler(rot.x, rot.y,
                rot.z), bulletLoadSpeed);
            BulletObjects[i + sb].transform.Find("Burst").GetComponent<RawImage>().DOFade(0, 0.1f)
                .SetDelay(bulletLoadSpeed);
            PlaySFX(BulletLoad);
            yield return new WaitForSeconds(bulletLoadSpeed*1.25f);
            BulletObjects[i + sb].transform.Find("Particle").GetComponent<ParticleSystem>().Stop();
        }
        // Chamber Idle Rotate
        rot.z += 80f;
        loadChamber.transform.DORotateQuaternion(Quaternion.Euler(rot.x, rot.y,
            rot.z), 2);
        loadChamberShadow.transform.DORotateQuaternion(Quaternion.Euler(rot.x, rot.y,
            rot.z), 2);
        yield return new WaitForSeconds(2);
        // Ring Rotation
        ringAnimation.enabled = true;
        // Bullet Indicators
        for (int i = 0; i < TruthBullets.Count; i++)
        {
            indicators[i].texture = indicatorOff;
        }
        bulletNumber = 0;
        indicators[0].texture = indicatorOn;
        for (int i = 0; i < TruthBullets.Count; i++)
        {
            indicators[i].enabled = true;
        }

        // Load UI Slides out and resets
        BulletLoadUI.transform.DOMoveX(-100, 0.2f);
        loadChamberShadow.DOFade(0, 0)
            .SetDelay(0.2f);
        loadChamber.DOFade(0, 0)
            .SetDelay(0.2f);
        for (int i = 0; i < TruthBullets.Count; i++)
        {
            RectTransform rect = BulletObjects[i + sb].GetComponent<RectTransform>();
            rect.position = bulletOGPos[i];
        }
        
        // Main UI Slides in
        MainNSDUI.transform.DOMoveX(0, 0.2f);

        panelCounter.DOFade(1, 0.2f);
        for(int i = 0; i < debate.PanelGroup.Count; i++)
        {
            panelCounters[i].DOFade(1, 0.2f);
        }
        emptyPortrait.DOFade(1, 0.2f);
        panelPortrait.DOFade(1, 0.2f);

        NSDReticle.ShowOrHide();
        if (TrialStats.Instance != null)
            TrialStats.Instance.ShowNSDStats();

        //Start the Debate
        InitializeSelectionBullets();
        currentBullet = TruthBullets[0];
        BulletText.text = currentBullet.Title;

        SelectBullets[bulletNumber].texture = ActiveBullet;
        BulletMove(true);

        trialCamera.SmoothFocus = false;
        inDebate = true;
        timer.Anim(1, 0.5f);
        asset.SetTimerBasedOnDifficulty(timer);
        timer.PauseTimer();
        timer.ResetTimer();
        yield return new WaitForSeconds(0.5f);
        timer.ResumeTimer();

        if (debugMode)
        {
            Time.timeScale = 1;
            DOTween.timeScale = 1;
        }
        SelectionImage.texture = TruthBullets[bulletNumber].Picture.texture;
        ActivateControls();
        cyclePass = true;
        co = PlayPanels();
        StartCoroutine(co);
        yield return null;
    }
    #endregion

    void ActivateControls()
    {
        //Debug.LogWarning("Enabling Controls");
        _controls.Minigame.Cycle.started += StartCycle;
        _controls.Minigame.Cycle.canceled += EndCycle;
        _controls.Minigame.Silence.started += FireSilencer;
        _controls.Minigame.Shoot.started += FireTruthBullet;
        _controls.Minigame.Accelerate.started += StartVelocity;
        _controls.Minigame.Accelerate.canceled += EndVelocity;
        _controls.Minigame.Focus.started += StartSpeed;
        _controls.Minigame.Focus.canceled += EndSpeed;
        _controls.Minigame.Change.started += ChangeBullet;
        _controls.Minigame.BulletList.started += CheckTruthMenu;
        _controls.UI.Controls.started += ShowControls;
    }
    void DeActivateControls()
    {
        //Debug.LogWarning("Disabling Controls");
        _controls.Minigame.Cycle.started -= StartCycle;
        _controls.Minigame.Cycle.canceled -= EndCycle;
        _controls.Minigame.Silence.started -= FireSilencer;
        _controls.Minigame.Shoot.started -= FireTruthBullet;
        _controls.Minigame.Accelerate.started -= StartVelocity;
        _controls.Minigame.Accelerate.canceled -= EndVelocity;
        _controls.Minigame.Focus.started -= StartSpeed;
        _controls.Minigame.Focus.canceled -= EndSpeed;
        _controls.Minigame.Change.started -= ChangeBullet;
        _controls.Minigame.BulletList.started -= CheckTruthMenu;
        _controls.UI.Controls.started -= ShowControls;
    }
    void CheckTruthMenu(CallbackContext context)
    {
        if (fast || slow || !canFire || TruthBulletMenu.OnMenu || InMenu.Value)
            return;
        DeActivateControls();
        TruthBulletMenu.OnMenu = true;
        CanSet(false);
        truthMenu.ShowTrial(context);
        StartCoroutine(WaitForMenuEnd());
    }
    void ShowControls(CallbackContext context)
    {
        bool b = !(fast || slow || !canFire || TruthBulletMenu.OnMenu || InCycle);
        if (!b)
            return;
        DeActivateControls();
        CanSet(false);
        GameManager.CallControls(b);
        StartCoroutine(WaitForControlsEnd());
    }
    IEnumerator WaitForMenuEnd()
    {
        while (TruthBulletMenu.OnMenu)
        {
            yield return null;
        }
        ActivateControls();
        CanSet(true);
        yield break;
    }
    IEnumerator WaitForControlsEnd()
    {
        while (InMenu.Value)
        {
            yield return null;
        }
        ActivateControls();
        CanSet(true);
        yield break;
    }
    void CanSet(bool mode)
    {
        canEffect = mode;
        canFire = mode;
        canSelect = mode;
        velocityCanChange = mode;
    }
    private NSDBuilder.Panel currentPanel;
    IEnumerator PlayPanels()
    {
        // Following is for when the Panels reset
        CanSet(true);

        // Start of Panel Loop
        for (int i = 0; i < debate.PanelGroup.Count; i++)
        {
            NSDBuilder.Panel panel = debate.PanelGroup[i];
            yield return PlayPanel(panel, i);
        }

        // End of the list of Panels for NSD, Disabling stuff to show end dialogue
        canEffect = false;
        RemoveSlow();
        CantFire();
        HideUI();
        DeActivateControls();
        if(InCycle)
            EndCycle(new CallbackContext());
        NSDReticle.Instance.ShowReticleOverride(false);
        PhraseReset();
        ffEmit.Stop();
        TrialDialogueManager.instance.PlayPanelEnd(debate.EndDialogue);
        yield break;
    }
    public void PlayTestPanel(NSDBuilder.Panel panel, int i)
    {
        inDebate = true;
        StartCoroutine(PlayPanel(panel, i));
    }
    //string prev = "";
    IEnumerator PlayPanel(NSDBuilder.Panel panel, int i)
    {
        currentPanel = panel;
        PhraseReset();

        PlayVoiceLine(panel.VoiceSFX);
        trialCamera.SeatFocus = panel.Speaker.TrialPosition;
        yield return new WaitForSeconds(Time.deltaTime / 2);
        
        if (panel.camAnimIdx != 10)
        {
            if (i == 0)
            {
                trialCamera.CameraAnimator.Rebind();
                trialCamera.CameraAnimator.Play(animNames[panel.camAnimIdx]);
            }
            else
                trialCamera.TriggerAnim(animNames[panel.camAnimIdx]);
            //Debug.LogWarning("Calling: " + animNames[panel.camAnimIdx]);
        }

        NextPanelCounter(i, panel);

        //trialCamera.transform.position.y = Character trial Height

        if (panel.Expression.Sprite != null)
        {
            Actor currentChar = GetCurrentActor(panel.Speaker);
            currentChar.character.material.mainTexture = panel.Expression.Sprite.mainTexture;
            currentChar.characterb.material.mainTexture = panel.Expression.Sprite.mainTexture;
            currentChar.characterbMask.material.mainTexture = panel.Expression.Sprite.mainTexture;
        }


        //yield return new WaitForSeconds(0.55f);
        float duration;
        
        if (panel.PhraseGroup.Count == 0)
            duration = trialCamera.CameraAnimator.GetCurrentAnimatorStateInfo(0).length;
        else
        {
            duration = PlayPhrase(panel);
            PlayWhiteNoise(panel);
        }
        yield return new WaitForSeconds(duration);
        yield break;
    }
    Actor GetCurrentActor(Character character)
    {
        for(int i = 0; i < actorObjects.Count; i++)
        {
            if (actorObjects[i].name == character.FirstName)
                return actors[i];
        }
        Debug.LogError("Couldn't find " + character.FirstName + " in the list of actors, " +
            "do you have that actor prefab with a wrong name? ");
        return null;
    }
    void NextPanelCounter(int i, NSDBuilder.Panel panel)
    {
        if (i != 0)
        {
            panelCounters[i - 1].texture = notTalking;
            panelCounters[i].texture = talking;
        }
        else
        {
            panelCounters[debate.PanelGroup.Count - 1].texture = notTalking;
            panelCounters[i].texture = talking;
        }
        //Debug.Log("SPEAKER IS: " + panel.Speaker.FirstName);
        panelSpeaker.text = panel.Speaker.FirstName;
        panelPortrait.texture = panel.Speaker.NSDPortrait;
    }
    
    private float PlayPhrase(NSDBuilder.Panel panel)
    {
        float panelLength = 0;
        float clipLength = 0;
        for (int i = 0; i < panel.PhraseGroup.Count; i++)
        {
            NSDBuilder.Phrase currentPhrase = panel.PhraseGroup[i];
            PhraseText phraseText = phraseTexts[i];

            //phraseText.gameObject.SetActive(true);

            phraseText.InitializeText(currentPhrase);
            AnimationClip animationClip = new AnimationClip
            {
                legacy = true
            };

            //PlayVoiceLine();

            currentPhrase.InitializeCurves(currentPhrase, animationClip);
            phraseText.phraseAnim.AddClip(animationClip, "transform");


            phraseText.phraseAnim.Play("transform");

            phraseText.StartAnimation(currentPhrase);

            if (i != 0 && clipLength < animationClip.length)
            {
                panelLength = animationClip.length;
            }
            if (i == 0)
            {
                panelLength = animationClip.length;
            }
            clipLength = animationClip.length;
        }
        //Debug.Log(panelLength + panel.waitTime);
        return panelLength + panel.waitTime;
    }

    void PlayWhiteNoise(NSDBuilder.Panel panel)
    {
        for(int i = 0; i < panel.whiteNoises.Count; i++)
        {
            NSDBuilder.WhiteNoise wn = panel.whiteNoises[i];
            WhiteNoiseText wnText = wnTexts[i];
            wnText.InitializeWhiteNoise(wn);
            AnimationClip animationClip = new AnimationClip
            {
                legacy = true
            };

            wn.InitializeCurves(wn, animationClip);
            wnText.Anim.AddClip(animationClip, "transform");


            wnText.Anim.Play("transform");
            wnText.StartAnimation(wn);
        }
    }

    #region ShowNSDUI
    void ShowUI() => StartCoroutine(ShowUIC());
    IEnumerator ShowUIC()
    {
        if (TrialStats.Instance != null)
            TrialStats.Instance.ShowNSDStats();
        NSDReticle.ShowOrHide();
        float duration = 0.5f;
        Vector2 pos = new Vector2(-838.4f, -437.2f);
        BulletMask.DOAnchorPos(pos, 0.5f)
            .SetDelay(0.2f);
        pos.x += 1920;
        pos.y += 100;
        BulletMask.DOAnchorPos(pos, 0);
        MainNSDUI.transform.DOMoveX(0, duration);

        panelCounter.DOFade(1, duration);
        for (int i = 0; i < debate.PanelGroup.Count; i++)
        {
            panelCounters[i].DOFade(1, duration);
        }
        panelSpeaker.text = "";
        emptyPortrait.DOFade(1, duration);
        panelPortrait.DOFade(1, duration);

        timerImage.DOFade(1, duration);
        timer.ResumeTimer();
        timer.TimerText.DOFade(1, duration);
        yield break;
    }
    #endregion

    #region HideNSDUI
    void HideUI() => StartCoroutine(HideUIC());
    IEnumerator HideUIC()
    {
        if (TrialStats.Instance != null)
            TrialStats.Instance.HideNSDStats();
        NSDReticle.ShowOrHide();
        float duration = 0.5f;
        Vector2 pos = new Vector2(-838.4f, -437.2f);
        BulletMask.DOAnchorPos(pos, 0.5f)
            .SetDelay(0.2f);
        pos.x -= 1920;
        pos.y -= 100;
        BulletMask.DOAnchorPos(pos, 0);
        MainNSDUI.transform.DOMoveX(-100, duration);

        panelCounter.DOFade(0, duration);
        for (int i = 0; i < debate.PanelGroup.Count; i++)
        {
            panelCounters[i].DOFade(0, duration);
        }
        panelSpeaker.text = "";
        emptyPortrait.DOFade(0, duration);
        panelPortrait.DOFade(0, duration);

        timerImage.DOFade(0, duration);
        timer.PauseTimer();
        timer.TimerText.DOFade(0, duration);
        yield break;
    }
    #endregion
    public float DisplayCounter(bool isConsent)
    {
        if (isConsent)
        {
            
            //Debug.Log(currentPanel.SpeakerNumber);
            consentImage.texture = consentTex[currentPanel.SpeakerNumber];
            
            consentAnimator.Play(consentAnimName);
            return consentAnimLength;
        }
        else // Counter
        {
            float waitTime;
            if (counter != null)
            {
                StartCoroutine(CounterPlay());
                waitTime = (float)counter.length - 0.5f;
            }
            else
            {
                ansAnimator.Play(animClip.name);
                ansCanvas.enabled = true;
                waitTime = animClip.length;
            }
            
            return waitTime;
        }
    }
    IEnumerator CounterPlay()
    {
        counter.Play();
        //yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => counter.frame > 3);
        counterTexture.enabled = true;

        yield break;
    }
    public void PhraseReset()
    {
        foreach(PhraseText text in phraseTexts)
        {
            text.Reset();
        }
        
        foreach(WhiteNoiseText text in wnTexts)
        {
            text.Reset();
        }
    }
    IEnumerator co;
    public void TimeUp()
    {
        StartCoroutine(TimeUpRoutine());
    }
    IEnumerator TimeUpRoutine()
    {
        while (!canFire || TruthBulletMenu.OnMenu || InMenu.Value)
        {
            if (InCycle)
            {
                
                break;
            }
            yield return null;
        }
        if(InCycle)
            EndCycle(new CallbackContext());
        NSDTextTex.enabled = false;
        RemoveSlow();
        StopPanels(false);
        PhraseReset();
        TrialDialogueManager.PlayTimeUp();
        yield break;
    }
    public void StopPanels(bool fuckedUp, TrialDialogue fuckUp = null)
    {
        inDebate = false;
        EndVelocity(new CallbackContext());
        EndSpeed(new CallbackContext());
        StopCoroutine(co);
        HideUI();
        DeActivateControls();
        ffEmit.Stop();
        if (fuckedUp)
        {
            NSDTextTex.enabled = false;
            //TrialDialogueManager.EndFU += RestartPanels;
            TrialDialogueManager.instance.PlayFuckUp(fuckUp);
        }
        
    }
    public void RestartPanels()
    {
        for (int i = 0; i < panelCounters.Length; i++)
            panelCounters[i].texture = notTalking;
        ShowUI();
        co = PlayPanels();
        StartCoroutine(co);
        inDebate = true;
        ActivateControls();
        NSDTextTex.enabled = true;
        if(!cyclePass)
            cyclePass = true;
    }
    void TurnOffIndicators()
    {
        foreach (RawImage x in indicators)
        {
            x.enabled = false;
        }
    }
    public void PlayEmpHitSound()
    {
        empHitEmit.Play();
    }
    public void StopNSD() // Called when Ending the NSD
    {
        NSDTextTex.enabled = false;
        if (TrialStats.Instance != null)
            TrialStats.Instance.ResetStamina();
        PhraseReset();
        volume.enabled = false;
        TurnOffIndicators();
        DeActivateControls();
        TrialDialogueManager.PlayerHasDied -= OnPlayerDeath;
        TrialDialogueManager.EndFU -= RestartPanels;
        EndMinigame();
        if (!debugMode)
            StartCoroutine(Unload());
        SoundManager.instance.PlayMusic(null); // Stops playing nsd music
    }
    IEnumerator Unload()
    {
        yield return new WaitUntil(() => UIBreak.finished);
        UIBreak.finished = false;
        Destroy(gameObject);
        yield break;
    }
}
