//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using static UnityEngine.InputSystem.InputAction;
using System;

/// <summary>
/// Start Screen Process for DREditor By: Sweden
/// The screen will show a list of basic splash screens or images
/// and then load to the next scene.
/// Requires DREditor, The Unity Input System, and DOTween
/// </summary>
public class DRESplashScreen : MonoBehaviour
{
    [Header("Options")]
    [Tooltip("When the next scene is ready to load have the player click Submit Action")]
    [SerializeField] bool clickOnLoadComplete = false;

    [Header("Main")]
    [SerializeField] Image blackFadeOverlay = null;
    [SerializeField] Image mainImage = null;
    [SerializeField] float fadeTime = 0.5f;

    [Header("Splash Screens")]
    [SerializeField] Canvas splashCanvas = null;
    [SerializeField] List<SplashScreen> splashScreens = new List<SplashScreen>();

    [Header("Loading")]
    [SerializeField] Canvas loadCanvas = null;
    [SerializeField] string toSceneOnLoad;
    [SerializeField] Image loadingBar = null;
    //[SerializeField] float loadBarValue = 3.5f;
    AsyncOperation async;

    #region Controls
#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif
    private void Awake()
    {
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
    #endregion

    private void Start()
    {
        loadCanvas.enabled = false;
        splashCanvas.enabled = false;

        if (blackFadeOverlay != null)
            blackFadeOverlay.DOFade(1, 0);

        StartSplash();
    }

    #region Splash Screens
    public void StartSplash()
    {
        // Start Splash Screen before loading
        StartCoroutine(SplashRoutine());
    }
    IEnumerator SplashRoutine()
    {
        // Graphical Splash Set Up
        splashCanvas.enabled = true;

        foreach(SplashScreen splashScreen in splashScreens)
        {
            mainImage.sprite = splashScreen.Screen;

            blackFadeOverlay.DOFade(0, fadeTime);
            yield return new WaitForSeconds(fadeTime);

            yield return new WaitForSeconds(splashScreen.screenTime);

            blackFadeOverlay.DOFade(1, fadeTime);
            yield return new WaitForSeconds(fadeTime);
        }

        mainImage.enabled = false;

        StartLoad();
        yield break;
    }
    #endregion

    #region Loading
    public void StartLoad()
    {
        // Set Up loading Screen

        StartCoroutine(LoadRoutine());
    }
    
    IEnumerator LoadRoutine()
    {
        // Graphical Set up with the canvas
        loadCanvas.enabled = true;

        if (blackFadeOverlay != null)
        {
            blackFadeOverlay.DOFade(0, 0.5f);
            yield return new WaitForSeconds(0.5f);
        }

        

        // Start Loading the Scene to load
        async = SceneManager.LoadSceneAsync(toSceneOnLoad);
        async.allowSceneActivation = false;
        Vector3 pos = loadingBar.transform.position;
        // For Eden's Garden our value was 
        float progressValue;

        while (async.progress < 0.9f)
        {
            // Move loading Bar
            progressValue = Mathf.Clamp01(async.progress / 0.9f);
            loadingBar.fillAmount = progressValue;
            yield return null;
        }
        progressValue = Mathf.Clamp01(async.progress / 0.9f);
        loadingBar.fillAmount = progressValue;
        // On Completion let the player click/enter/submit to load the Main Menu Screen
        //Debug.Log("Loading Complete!");
        if (clickOnLoadComplete)
            _controls.UI.Submit.started += ConfirmLoad;
        else
            FadeToScene();

        yield break;
    }
    void ConfirmLoad(CallbackContext context)
    {
        _controls.UI.Submit.started -= ConfirmLoad;
        FadeToScene();
    }
    void FadeToScene()
    {
        StartCoroutine(FadeToSceneRoutine());
    }
    IEnumerator FadeToSceneRoutine()
    {
        blackFadeOverlay.DOFade(1, fadeTime);
        yield return new WaitForSeconds(fadeTime);
        async.allowSceneActivation = true;
        yield break;
    }
    #endregion

    [Serializable]
    internal class SplashScreen
    {
        public Sprite Screen;
        public float screenTime;
    }
}
