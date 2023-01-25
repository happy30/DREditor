//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

public class TrialPrepUI : MonoBehaviour
{
    [SerializeField] MenuGroup group = null;
    [SerializeField] TextMeshProUGUI title = null;
    [Tooltip("When loading the trial prep UI Scene from a save file the time for fading into this UI")]
    [SerializeField] float fadeOutTime = 0.3f;
    [SerializeField] Animator animator = null;
    [SerializeField] string triggerString = "Show";
    [Tooltip("When Begin trial is selected, how long it takes to fade to black after the animator animation (still applies if" +
        " there is no animator!)")]
    [SerializeField] float animatorFadeTime = 0.3f;
    //[SerializeField] TextMeshProUGUI chapterTitleText = null;
    [SerializeField] string trialSceneName = "GYM_Trial";
    [Header("Optional")]
    [Header("All Video fields must be filled out to work")]
    [SerializeField] VideoPlayer video = null;
    [SerializeField] RawImage videoProjector = null;
    [SerializeField] Canvas videoCan = null;
    [SerializeField] bool skipAnimatorFadeOut = false;
    [SerializeField] AudioClip startSound = null;
    [SerializeField] AudioClip startMusic = null;

    private void Awake()
    {
        /* Note:
         * I've added this code to Save Points just so save points don't have to be restricted to just 
         * being used for starting of trials. */
        if (GameManager.instance.currentMode != GameManager.Mode.Trial)
            GameManager.instance.ChangeMode(GameManager.Mode.Trial);
        GameManager.instance.addState = AddState.Preparation;
        GameManager.instance.cantBeInMenu = true;
        ControlsUI.Override = true;
        SoundManager.instance.PlayMusic(null);

        
    }
    private void Start()
    {
        if (!GameSaver.LoadingFile)
        {
            RoomLoader.skipActivateControls = true;
            DialogueAnimConfig.MuteEndDialogue = true;
            RoomLoader.PreEndLoad += EndLoad;
            DialogueAnimConfig.OnFinishedDialogue += EndDia;
        }
        StartCoroutine(InitializePrep());
    }
    void EndLoad()
    {
        RoomLoader.skipActivateControls = false;
        
    }
    void EndDia()
    {
        DialogueAnimConfig.MuteEndDialogue = false;
    }
    void InitializeTrialPrep()
    {
        /* Stuff like changing the Title based on current chapter
         */
        title.text = ProgressionManager.instance.GetChapter().title;
    }
    bool startedMusic = false;
    public void PlayStartMusic() // called on "No" Option of SavePoint UI's PopUp
    {
        if(!startedMusic && startMusic != null && !GameSaver.LoadingFile && !SoundManager.instance.CurrentEventInMusicIs(startMusic))
        {
            startedMusic = true;
            SoundManager.instance.PlayMusic(startMusic);
            //Debug.LogWarning("Played Start Music");
        }
    }
    IEnumerator InitializePrep()
    {
        if (!GlobalFade.instance.IsDark)
            GlobalFade.instance.FadeTo(0);
        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        
        InitializeTrialPrep();

        if (GameSaver.LoadingFile)
        {
            group.EnableCanvas(true);
            yield return new WaitUntil(() => !GameSaver.LoadingFile);
            if (GlobalFade.instance.IsDark)
            {
                GlobalFade.instance.FadeOut(fadeOutTime);
                yield return new WaitForSecondsRealtime(fadeOutTime);
            }
            PlayStartMusic();
            group.Reveal();
        }

        yield break;
    }
    public void ShowPause()
    {
        EventSystem.current.SetSelectedGameObject(null);
        group.EnableButtons(false);
        PauseMenu.OnResumeEnd += ResumePrep;
        PauseMenu.instance.StartPause();
        PauseMenu.instance.PlayPauseSound();
    }
    void ResumePrep()
    {
        PauseMenu.OnResumeEnd -= ResumePrep;
        group.EnableButtons(true);
        group.EvaluateSelect();

    }
    private void OnDestroy()
    {
        PauseMenu.OnResumeEnd -= ResumePrep;
        ControlsUI.Override = false;
        RoomLoader.PreEndLoad -= EndLoad;
        DialogueAnimConfig.OnFinishedDialogue -= EndDia;
        EndLoad();
        EndDia();
    }
    public void BeginTrial()
    {
        EventSystem.current.SetSelectedGameObject(null);
        GameManager.instance.addState = AddState.None;
        StartCoroutine(StartTrial());
    }
    IEnumerator StartTrial()
    {
        SoundManager.instance.PlayMusic(null);
        if (animator)
        {
            animator.SetTrigger(triggerString);
            yield return new WaitForSeconds(Time.unscaledDeltaTime);
            if (animator.GetCurrentAnimatorStateInfo(0).length == float.PositiveInfinity)
            {
                yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).length != float.PositiveInfinity);
            }
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }
        if (video)
        {
            GlobalFade.instance.FadeTo(0.2f);
            yield return new WaitForSeconds(0.2f);
            videoCan.enabled = true;
            video.Prepare();
            yield return new WaitUntil(() => video.isPrepared);
            video.Play();

            yield return new WaitUntil(() => video.isPlaying);
            yield return new WaitUntil(() => video.frame >= 1);
            //GlobalFade.instance.FadeOut(0);
            //videoPlayer.playbackSpeed = 0;
            RenderTexture x = new RenderTexture(Screen.width, Screen.height, 32);
            videoProjector.texture = x;
            video.targetTexture = x;
            if(startSound != null)
                SoundManager.instance.PlaySFX(startSound);
            yield return new WaitForSeconds(0.2f);
            GlobalFade.instance.FadeOut(0.2f);
            yield return new WaitUntil(() => !video.isPlaying);
        }

        if (!skipAnimatorFadeOut)
        {
            GlobalFade.instance.FadeTo(animatorFadeTime);
            yield return new WaitForSeconds(animatorFadeTime);
        }
        else
        {
            GlobalFade.instance.FadeTo(0);
        }


        // Load the Trial Room
        SceneManager.LoadSceneAsync(trialSceneName);
        yield break;
    }
}
