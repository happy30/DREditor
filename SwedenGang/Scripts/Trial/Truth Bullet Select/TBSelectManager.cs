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
using UnityEngine.Events;

public class TBSelectManager : MinigameManagerBase
{
    public static TBSelectManager instance = null;

    [Header("Debugging")]
    [Tooltip("Turn True if testing in a scene")]
    [SerializeField] bool debugMode = false;
    [SerializeField] TBSelectBuilder debugtbsb = null;
    

    [Header("Required")]
    [SerializeField] TruthBulletMenu menu = null;
    [SerializeField] ConfirmUI confirmUI = null;

    [Header("Question Section")]
    [SerializeField] QuestionToggle questionToggle = null;

    /*
    [Tooltip("The Animator for displaying the confirm selection UI")]
    [SerializeField] Animator confirmAnimator = null;
    [Tooltip("Name of the animation clip in the confirm animator to show the Confirm UI")]
    [SerializeField] string showString = "Show";
    [Tooltip("Name of the animation clip in the confirm animator to hide the Confirm UI")]
    [SerializeField] string hideString = "Hide";
    */

    [Header("I Got It Section")]
    [SerializeField] Canvas ansCanvas = null;
    [SerializeField] Animator ansAnimator = null;
    [SerializeField] string ansAnimName = "IGotIt";
    [SerializeField] AudioClip ansSound;
    [SerializeField] AudioClip ansVO;
    [Tooltip("How long the game waits until it plays the \"I Got It\" animation")]
    [SerializeField] float menuFadeWaitTime = 0.5f;

    TrialTimer timer => GetComponentInChildren<TrialTimer>();
    private TBSelectBuilder tbsb = null;
    private TextMeshProUGUI selection = null;
    private Button LastButton = null;
    private TBSelectBuilder.Selection Chosen = null;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        if (debugMode)
        {
            StartSelection(debugtbsb);
            GameManager.instance.cantBeInMenu = true;
        }
        
        if (ansCanvas != null && ansCanvas.enabled)
            ansCanvas.enabled = false;
    }
    private void OnEnable()
    {
        TrialDialogueManager.EndFU += Restart;
        TrialTimer.TimeUp += TimeUp;
        TrialDialogueManager.PlayerHasDied += ResetTime;
    }
    private void OnDisable()
    {
        TrialDialogueManager.EndFU -= Restart;
        TrialTimer.TimeUp -= TimeUp;
        TrialDialogueManager.PlayerHasDied -= ResetTime;
        menu.scrollGroup.StartEvents.RemoveListener(Test);
    }
    public void PlayTBSelect(ScriptableObject asset)
    {
        TBSelectManager.instance.StartSelection((TBSelectBuilder)asset);
    }
    public void StartSelection(TBSelectBuilder tbsb)
    {
        this.tbsb = tbsb;
        StartCoroutine(BeginSelection(tbsb));
    }
    bool inSelect = false;
    IEnumerator BeginSelection(TBSelectBuilder tbsb)
    {
        /* Show the TBM Animation, should automate everything else?
         * Enable Ability to show the question and display and set the Time UI
         */
        menu.scrollGroup.StartEvents.AddListener(Test);
        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        menu.ShowTrial(new CallbackContext());

        if (timer != null)
        {
            timer.Anim(1, 0.5f);
            tbsb.SetTimerBasedOnDifficulty(timer);
            timer.PauseTimer();
            timer.ResetTimer();
            yield return new WaitForSecondsRealtime(0.5f);
            timer.ResumeTimer();
        }
        yield return new WaitForSecondsRealtime(0.25f);
        questionToggle.SetQuestion(tbsb.question);
        questionToggle.Activate();
        
        
        yield break;
    }
    void Test() => inSelect = true;
    public void SetButtonSelection(Button button)
    {
        LastButton = button;
    }
    public void SetBulletSelection(TextMeshProUGUI title)
    {
        selection = title;
    }
    /// <summary>
    /// Displays the UI to confirm the choice 
    /// </summary>
    public void CallConfirmUI()
    {
        if (!inSelect) return;
        /* Set Selected game object to null
         * Display the Confirm UI
         * Set Selection to no / yes 
         */
        confirmUI.Show(); // The Menu Group will auto select it for us
        //confirmAnimator.Play(showString);
        
    }
    /// <summary>
    /// Called when the player chooses "No" on the Confirmation UI
    /// </summary>
    public void LeaveConfirmUI()
    {
        /* Set Selected game object to null
         * Hide the Confirm UI
         * Set Selection to the previous selection
         */
        confirmUI.Hide();
        //confirmAnimator.Play(hideString);
        EventSystem.current.SetSelectedGameObject(LastButton.gameObject);
        
    }
    /// <summary>
    /// Called when the player chooses "Yes" on the Confirmation UI
    /// </summary>
    public void ConfirmSelection()
    {
        confirmUI.Hide();
        timer.PauseTimer();
        Debug.Log(selection.text);
        //confirmAnimator.Play(hideString);
        if (CheckAnswer())
            OnCorrect();
        else
            OnFuckUp(Chosen.wrongDialogue);

        HideVisuals();
    }
    bool CheckAnswer()
    {
        for (int i = 0; i < tbsb.selections.Count; i++)
        {
            if (selection.text == tbsb.selections[i].bulletOption.Title)
            {
                Chosen = tbsb.selections[i];
                return tbsb.selections[i].isAnswer;
            }
        }
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
        SoundManager.instance.PlayVoiceLine(ansVO);
        SoundManager.instance.PlaySFX(ansSound);
        yield return new WaitForSeconds(Time.deltaTime);
        yield return new WaitForSeconds(ansAnimator.GetCurrentAnimatorStateInfo(0).length);
        EndMinigame();
        StartCoroutine(UnLoad());
        yield break;
    }
    void HideVisuals()
    {
        menu.HideTrial(new CallbackContext());
        timer.Anim(0, 0.5f);
        questionToggle.Deactivate();
    }
    void OnFuckUp(TrialDialogue dia)
    {
        TrialDialogueManager.instance.PlayFuckUpNonNSD(dia);
    }
    void Restart()
    {
        
        menu.ShowTrial(new CallbackContext());
        timer.Anim(1, 1);
        questionToggle.Activate();

        StartCoroutine(ResumeTimer());
    }
    IEnumerator ResumeTimer()
    {
        yield return new WaitForSecondsRealtime(1);
        timer.ResumeTimer();
        inSelect = true;
        yield break;
    }
    public void TimeUp()
    {
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
