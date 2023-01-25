//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class EndCredits : MonoBehaviour
{
    [SerializeField] string menuScene = "";
    [SerializeField] TextMeshProUGUI text = null;
    [SerializeField] List<AudioClip> list = new List<AudioClip>();

    //[SerializeField] AudioSource creditAudio = null;
    //[SerializeField] StudioEventEmitter creditAudio = null;
    //[SerializeField] VideoPlayer video = null;
    bool started = false;
    bool canSkip = false;
    bool skipping = false;
#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif

    private void Awake()
    {
        if(GameManager.instance != null)
            GameManager.instance.cantBeInMenu = true;
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
        //GameManager.instance.menuAccess = false;
#if ENABLE_INPUT_SYSTEM
        _controls.UI.Pause.started += StartSkip;
#endif

        StartCredits();
    }
    private void OnDestroy()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.UI.Pause.started -= StartSkip;
#endif
        //GameManager.instance.menuAccess = true;
    }
    void StartCredits()
    {
        StartCoroutine(StartCreditsRoutine());
    }
    IEnumerator StartCreditsRoutine()
    {

        if (GlobalFade.instance)
        {
            while (GlobalFade.instance.isTransitioning)
            {
                yield return null;
            }
            if(GlobalFade.instance.IsDark)
                GlobalFade.instance.FadeOut(1);
        }
        for(int i = 0; i < list.Count; i++)
        {
            SoundManager.instance.PlayMusic(list[i], true);
            yield return new WaitUntil(() => SoundManager.instance.MusisIsPlaying());
            while (SoundManager.instance.MusisIsPlaying())
            {
                yield return null;
            }
        }
        yield break;
    }
    void StartSkip(InputAction.CallbackContext context)
    {
        Debug.Log("CALLED");
        if (skipping)
            return;
        if (canSkip)
        {
            skipping = true;
            StartCoroutine(Leave());
        }
        if (!started)
        {
            Debug.Log("Started");
            started = true;
            StartCoroutine(ShowText());
        }

    }
    IEnumerator ShowText()
    {
        text.DOFade(1, 1);
        //yield return new WaitForSecondsRealtime(0.2f);
        canSkip = true;
        yield return new WaitForSecondsRealtime(1);
        text.DOFade(0, 1);
        yield return new WaitForSecondsRealtime(1);
        started = false;
        canSkip = false;

        yield break;
    }
    IEnumerator EndOfVideo()
    {
        /*
        while (!video.isPlaying)
        {
            yield return null;
        }
        while (video.isPlaying)
        {
            yield return null;
        }
        */
        //StartCoroutine(Leave());
        yield break;
    }
    public void LeaveCredits() => StartCoroutine(Leave());
    IEnumerator Leave()
    {
        GlobalFade.instance.FadeTo(0.5f);
        /* if using unity audio
        for (int i = 0; creditAudio.volume != 0; i++)
        {
            creditAudio.volume -= 0.05f;
            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }
        */
        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadSceneAsync(menuScene);
        yield break;
    }

}
