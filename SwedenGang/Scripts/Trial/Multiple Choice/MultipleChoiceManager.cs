//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using MultipleChoice;
using UnityEngine.EventSystems;
using static UnityEngine.InputSystem.InputAction;

public class MultipleChoiceManager : MinigameManagerBase
{
    public static MultipleChoiceManager instance = null;
    [Header("Debugging")]
    [Tooltip("Turn True if testing in a scene")]
    [SerializeField] bool debugMode = false;
    [Tooltip("Current MC Builder being used, or if you're testing, the MCBuilder you're testing with")]
    [SerializeField] MultipleChoiceBuilder mcb = null;
    //[Tooltip("The Multiple Choice Builder in the event of the player dying (THIS MUST BE FILLED)")]
    //[SerializeField] MultipleChoiceBuilder gameOver = null;
    [Header("Animation")]
    [Tooltip("The Animator that handles the intro Animation to Multiple Choice Minigame")]
    [SerializeField] Animator animator = null;
    [SerializeField] string introTriggerString = "Intro";
    [SerializeField] float introAnimWaitTime;
    [SerializeField] TMP_Text questionText = null;
    [SerializeField] Image damonBar = null;
    [SerializeField] Color chosenColor;
    [SerializeField] MassImageAnimator characterEyes = null;
    [SerializeField] List<MCChoice> choices = new List<MCChoice>();
    public float appearDuration = 0.5f;
    public float selectDuration = 0.2f;
    [Header("I Got It Section")]
    [SerializeField] Canvas ansCanvas = null;
    [SerializeField] Animator ansAnimator = null;
    [SerializeField] string ansAnimName = "IGotIt";
    [SerializeField] AudioClip ansSound;
    [SerializeField] AudioClip ansVO;
    [SerializeField] AudioClip startSound;

    EventSystem eventSystem => EventSystem.current;
    bool inSelect = false;
    GameObject selected;
    MultipleChoiceBuilder.Choice chosen;
    bool inDialogue = false;
    //bool gameIsOver = false;
    //bool inMC = false;
    TrialTimer timer => GetComponentInChildren<TrialTimer>();

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
    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Enable();
#endif
        TrialDialogueManager.EndFU += MultipleChoiceManager.instance.ShowMC;
        TrialTimer.TimeUp += TimeUp;
        TrialDialogueManager.PlayerHasDied += ResetTime;
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Disable();
#endif
        TrialDialogueManager.EndFU -= MultipleChoiceManager.instance.ShowMC;
        TrialTimer.TimeUp -= TimeUp;
        TrialDialogueManager.PlayerHasDied -= ResetTime;
    }
    private void Start()
    {
        if (debugMode)
            StartChoosing(mcb);
        _controls.UI.Submit.started += EvaluateChoice;
        if (ansCanvas != null && ansCanvas.enabled)
            ansCanvas.enabled = false;
    }
    private void Update()
    {
        
        if (inSelect && selected != eventSystem.currentSelectedGameObject) // Handles Choice swapping 
        {
            if (eventSystem.currentSelectedGameObject == null)
            {
                eventSystem.SetSelectedGameObject(selected);
                return;
            }
            GetChoice(selected).DeSelect();
            GetChoice(eventSystem.currentSelectedGameObject).Select();
            selected = eventSystem.currentSelectedGameObject;
        }
    }
    void EvaluateChoice(CallbackContext context)
    {
        if (inSelect)
        {
            inSelect = false;
            GetChoice(selected).DeSelect();
            eventSystem.SetSelectedGameObject(null);

            if (CheckAnswer())
            {
                // Play "That's It!" Animations and Use End Dialogue
                SoundManager.instance.PlaySubmit();
                StartCoroutine(IGotIt());
            }
            else
            {
                // Play the Fuck Up Trial Dialogue
                HideMC();
            }
        }
    }
    public void PlayMC(ScriptableObject asset)
    {
        MultipleChoiceManager.instance.StartChoosing((MultipleChoiceBuilder)asset);
    }

    public void StartChoosing(MultipleChoiceBuilder mcb)
    {
        StartCoroutine(BeginChoice(mcb));
    }
    IEnumerator BeginChoice(MultipleChoiceBuilder mcb)
    {
        //if (!gameIsOver)
            //inMC = true;
        if (mcb.startDialogue != null)
        {
            TrialDialogueManager.instance.UniTrialDialogue(mcb.startDialogue);
            SetDialogueBool();
            TrialDialogueManager.DialogueEnded += SetDialogueBool;
            while (inDialogue)
            {
                yield return null;
            }
            TrialDialogueManager.DialogueEnded -= SetDialogueBool;
        }

        this.mcb = mcb;
        animator.SetTrigger(introTriggerString);
        SoundManager.instance.PlaySFX(startSound);
        yield return new WaitForSeconds(introAnimWaitTime);
        if(characterEyes != null)
        {
            characterEyes.SetFade();
            MassImageAnimator.Fade(1, 1);
        }
        questionText.text = mcb.question;
        for(int i = 0; i < choices.Count; i++)
        {
            choices[i].choiceText.text = mcb.choices[i].text;
        }
        
        questionText.DOFade(1, 1);
        yield return new WaitForSeconds(1);
        int choicesAmount = 0;
        for (int i = 0; i < choices.Count; i++)
        {
            if(mcb.choices[i].text != "")
            {
                choices[i].Show();
                choicesAmount++;
            }
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.2f * choicesAmount);

        if(timer != null)
        {
            timer.Anim(1, 0.5f);
            mcb.SetTimerBasedOnDifficulty(timer);
            timer.PauseTimer();
            timer.ResetTimer();
            //yield return new WaitForSeconds(0.5f);
            timer.ResumeTimer();
        }

        eventSystem.SetSelectedGameObject(choices[0].choiceText.gameObject);
        selected = eventSystem.currentSelectedGameObject;
        choices[0].Select();
        inSelect = true;
        yield break;
    }

    MCChoice GetChoice(GameObject ob)
    {
        foreach(MCChoice choice in choices)
        {
            if (choice.choiceText.gameObject == ob)
                return choice;
        }
        return null;
    }

    bool CheckAnswer()
    {
        for(int i = 0; i < mcb.choices.Length; i++)
        {
            if (GetChoice(selected).choiceText.text == mcb.choices[i].text)
            {
                chosen = mcb.choices[i];
                return mcb.choices[i].isAnswer;
            }
        }
        return false;
    }
    void HideMC()
    {

        HideVisuals();
        if (timer != null)
            timer.PauseTimer();
        
        TrialDialogueManager.instance.PlayFuckUpNonNSD(chosen.wrongDialogue);
    }
    void HideVisuals()
    {
        if (timer != null)
            timer.Anim(0, 0.5f);
        foreach (MCChoice choice in choices)
        {
            choice.TempHide();
        }
        MassImageAnimator.Fade(0, 1);
        questionText.DOFade(0, 1);
        damonBar.DOFade(0, 1);
    }
    public void ShowMC()
    {
        StartCoroutine(ReThink());
    }
    IEnumerator ReThink()
    {

        foreach (MCChoice choice in choices)
        {
            if (choice.choiceText.text != "")
            {
                choice.TempShow();
            }
        }
        MassImageAnimator.Fade(1, 1);
        questionText.DOFade(1, 1);
        damonBar.DOFade(1, 1);
        eventSystem.SetSelectedGameObject(choices[0].choiceText.gameObject);
        selected = eventSystem.currentSelectedGameObject;
        choices[0].Select();
        if (timer != null)
            timer.Anim(1, 1);
        yield return new WaitForSeconds(1);
        if (timer != null)
            timer.ResumeTimer();
        yield return new WaitForEndOfFrame();
        inSelect = true;
        yield break;
    }
    IEnumerator IGotIt()
    {
        if (timer != null)
        {
            timer.StopTimer();
            timer.Anim(0, 0.5f);
        }
        
        HideVisuals();
        yield return new WaitForSeconds(0.75f);
        ansAnimator.Play(ansAnimName);
        ansCanvas.enabled = true;
        SoundManager.instance.PlaySFX(ansSound);
        SoundManager.instance.PlayVoiceLine(ansVO);
        yield return new WaitForSeconds(Time.deltaTime);
        yield return new WaitForSeconds(ansAnimator.GetCurrentAnimatorStateInfo(0).length);
        EndMinigame();
        //inMC = false;
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
        yield break;
    }
    public void SetDialogueBool() => inDialogue = !inDialogue;
    public void TimeUp()
    {
        inSelect = false;
        GetChoice(selected).DeSelect();
        EventSystem.current.SetSelectedGameObject(null);
        //yield return new WaitForSeconds(Time.deltaTime);
        // fade everything out
        HideVisuals();
        TrialDialogueManager.EndFU += ResetTime;
        TrialDialogueManager.PlayTimeUp();
    }
    void ResetTime()
    {
        TrialDialogueManager.EndFU -= ResetTime;
        if (timer != null)
            timer.ResetTimer();
    }
}
