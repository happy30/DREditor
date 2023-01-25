//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TruthBulletSelect;
using TMPro;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.EventSystems;
using DREditor.Dialogues;
using DG.Tweening;
using System.Linq;

public class SSManager : MinigameManagerBase
{
    public static SSManager instance = null;

    [Header("Debugging")]
    [Tooltip("Turn True if testing in a scene")]
    [SerializeField] bool debugMode = false;
    [SerializeField] SSBuilder debugssb = null;


    [Header("Required")]
    [SerializeField] AudioClip startSound;
    [SerializeField] Animator animator = null;
    [SerializeField] string animName = "SS_Intro_Clip";
    [SerializeField] Canvas canvas = null;
    [SerializeField] RawImage CG = null;

    [SerializeField] SSReticle reticle = null;
    [SerializeField] ConfirmUI confirmUI = null;

    [Header("Question Section")]
    [SerializeField] QuestionToggle questionToggle = null;
    [SerializeField] AudioClip confirm = null;


    [Header("I Got It Section")]
    [SerializeField] Canvas ansCanvas = null;
    [SerializeField] Animator ansAnimator = null;
    [SerializeField] string ansAnimName = "IGotIt";
    [SerializeField]AudioClip ansSound;
    [SerializeField]AudioClip ansVO;
    [Tooltip("How long the game waits until it plays the \"I Got It\" animation")]
    [SerializeField] float menuFadeWaitTime = 0.5f;

    Collider2D spot = null;
    TrialTimer timer => GetComponentInChildren<TrialTimer>();
    private SSBuilder ssb = null;
    
    
    private SSBuilder.Spot Chosen = null;

#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
    }

    private void Start()
    {
        if (debugMode)
        {
            StartSelection(debugssb);
            GameManager.instance.cantBeInMenu = true;
        }
        //Application.targetFrameRate = 15;
        if (ansCanvas != null && ansCanvas.enabled)
            ansCanvas.enabled = false;
    }

    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Enable();
#endif
        TrialDialogueManager.EndFU += Restart;
        TrialTimer.TimeUp += TimeUp;
        TrialDialogueManager.PlayerHasDied += ResetTime;
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Disable();
#endif
        TrialDialogueManager.EndFU -= Restart;
        TrialTimer.TimeUp -= TimeUp;
        TrialDialogueManager.PlayerHasDied -= ResetTime;
    }
    public void PlaySpotSelect(ScriptableObject asset)
    {
        SSManager.instance.StartSelection((SSBuilder)asset);
    }
    public void StartSelection(SSBuilder ssb)
    {
        this.ssb = ssb;
        StartCoroutine(BeginSelection());
    }
    IEnumerator BeginSelection()
    {
        /* Show the SS Animation, should automate everything else?
         * Enable Ability to show the question and display and set the Time UI
         */

        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        /* Set CG texture
         * Generate Spots
         * Set Timer 
         * Enable Canvas
         * Fade the Image
         * 
         */
        CG.DOFade(0, 0);
        CG.texture = ssb.texture;
        CG.DOFade(1, 1);
        InitializeSpots();
        animator.Play(animName);
        canvas.enabled = true;
        SoundManager.instance.PlaySFX(startSound);
        yield return new WaitForSeconds(Time.deltaTime);
        
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        

        if (timer != null)
        {
            ssb.SetTimerBasedOnDifficulty(timer);
            timer.PauseTimer();
            timer.ResetTimer();
            //yield return new WaitForSecondsRealtime(0.5f);
            timer.Anim(1, 0.5f);

        }
        yield return new WaitForSecondsRealtime(0.25f);
        if(questionToggle != null)
        {
            questionToggle.SetQuestion(ssb.question);


            questionToggle.Activate();
        }
        if (timer != null)
            timer.ResumeTimer();

        reticle.ShowReticle();

        _controls.UI.Submit.started += SubmitSpot;
        Cursor.lockState = CursorLockMode.Confined;
        yield break;
    }

    void InitializeSpots()
    {
        for(int i = 0; i < ssb.spots.Count; i++)
        {
            SSBuilder.Spot s = ssb.spots[i];
            GameObject g = new GameObject();
            g.name = ssb.spots[i].spotName;
            g.transform.parent = canvas.gameObject.transform;
            g.transform.localScale = new Vector3(1, 1, 1);
            g.transform.position = s.position * canvas.transform.localScale.x;
            g.transform.eulerAngles = s.rotation;
            BoxCollider2D b = g.AddComponent<BoxCollider2D>();
            b.size = s.size;
            b.offset = s.center;
        }
    }
    
    void SubmitSpot(CallbackContext ctx)
    {
        //Debug.Log("Called Submit!");
        spot = reticle.GetSpot();
        //Debug.LogWarning("Active was: " + confirmUI.choiceGroup.isActive);
        if (spot != null && !confirmUI.choiceGroup.isActive)
            CallConfirmUI();
        else
            Debug.LogWarning("Spot Was Null!");

    }

    /// <summary>
    /// Displays the UI to confirm the choice 
    /// </summary>
    public void CallConfirmUI()
    {
        _controls.UI.Submit.started -= SubmitSpot;
        /* Display the Confirm UI
         * Set Selection to no / yes 
         * Lock the Reticle
         */
        //Debug.LogWarning("On box: " + spot.gameObject.name);
        StartCoroutine(ShowConfirm());
    }
    IEnumerator ShowConfirm()
    {
        yield return new WaitForSeconds(Time.deltaTime);
        reticle.ShowReticle();
        confirmUI.Show(); // The Menu Group will auto select it for us
                          //confirmAnimator.Play(showString);
        

        yield break;
    }
    /// <summary>
    /// Called when the player chooses "No" on the Confirmation UI
    /// </summary>
    public void LeaveConfirmUI()
    {
        Debug.LogWarning("Called leave");
        /* Set Selected game object to null
         * Hide the Confirm UI
         * Unlock the reticle
         */
        EventSystem.current.SetSelectedGameObject(null);
        //confirmUI.choiceGroup.EndEvents.AddListener(FinishedLeave);
        confirmUI.Hide();

        //confirmAnimator.Play(hideString);
        // Unlock the reticle
        StartCoroutine(LeaveConfirm());
    }
    void FinishedLeave()
    {
        confirmUI.choiceGroup.EndEvents.RemoveListener(FinishedLeave);
        
    }
    IEnumerator LeaveConfirm()
    {
        yield return new WaitForSeconds(Time.deltaTime);
        reticle.ShowReticle();
        _controls.UI.Submit.started += SubmitSpot;
        yield break;
    }
    /// <summary>
    /// Called when the player chooses "Yes" on the Confirmation UI
    /// </summary>
    public void ConfirmSelection()
    {
        confirmUI.Hide();
        EventSystem.current.SetSelectedGameObject(null);
        timer.PauseTimer();
        //Debug.Log(selection.text);
        //confirmAnimator.Play(hideString);
        Chosen = GetChosen();
        if (CheckAnswer())
            OnCorrect();
        else
            OnFuckUp(Chosen.wrongDialogue);

        HideVisuals();
        SoundManager.instance.PlaySFX(confirm);
    }
    SSBuilder.Spot GetChosen()
    {
        return ssb.spots.Where(n => n.spotName == spot.gameObject.name).ElementAt(0);
        
    }
    bool CheckAnswer()
    {
        for (int i = 0; i < ssb.spots.Count; i++)
            if (spot.gameObject.name == ssb.spots[i].spotName)
                return ssb.spots[i].isAnswer;
        return false;
    }
    
    void OnCorrect()
    {
        /* Display "I Got It" Animation
         * Hide the Truth Bullet Menu 
         * End the Minigame
         */
        StartCoroutine(IGotIt());
    }
    IEnumerator IGotIt()
    {
        HideVisuals();
        yield return new WaitForSeconds(menuFadeWaitTime);

        ansAnimator.Play(ansAnimName);
        ansCanvas.enabled = true;
        SoundManager.instance.PlaySFX(ansSound);
        SoundManager.instance.PlayVoiceLine(ansVO);
        yield return new WaitForSeconds(Time.deltaTime);
        yield return new WaitForSeconds(ansAnimator.GetCurrentAnimatorStateInfo(0).length);
        EndMinigame();
        StartCoroutine(UnLoad());
        yield break;
    }
    void HideVisuals()
    {
        // Hide Visuals
        CG.DOFade(0, 0.5f);
        timer.Anim(0, 0.5f);
        questionToggle.Deactivate();
        reticle.ShowReticleOverride(false);
        reticle.enabled = false;
    }
    void OnFuckUp(TrialDialogue dia)
    {
        TrialDialogueManager.instance.PlayFuckUpNonNSD(dia);
    }
    void Restart()
    {
        
        // Show Visuals
        CG.DOFade(1, 0.5f);
        timer.Anim(1, 1);
        questionToggle.Activate();
        if(!reticle.enabled)
            reticle.enabled = true;
        reticle.ShowReticleOverride(true);
        StartCoroutine(ResumeTimer());
    }
    IEnumerator ResumeTimer()
    {
        yield return new WaitForSecondsRealtime(1);
        timer.ResumeTimer();
        _controls.UI.Submit.started += SubmitSpot;
        yield break;
    }
    public void TimeUp()
    {
        _controls.UI.Submit.started -= SubmitSpot;
        EventSystem.current.SetSelectedGameObject(null);

        // fade everything out
        confirmUI.Hide();

        HideVisuals();

        TrialDialogueManager.EndFU += ResetTime;
        TrialDialogueManager.PlayTimeUp();
        //StartCoroutine(PlayTimeUp());
    }
    void ResetTime()
    {
        TrialDialogueManager.EndFU -= ResetTime;
        timer.ResetTimer();
    }
    IEnumerator PlayTimeUp()
    {

        yield return new WaitForSeconds(0.5f);
        // set selection to null

        yield break;
    }
    IEnumerator UnLoad()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
        yield break;
    }

}

