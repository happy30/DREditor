//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class EG_GameOver : MonoBehaviour
{
    [SerializeField] string TitleScreenSceneName = "GYM_MainMenu";
    [SerializeField] bool debugMode = false;
    [SerializeField] string animName = "GameOver";
    [SerializeField] string fadeAnimName = "Fade";
    [SerializeField] Animator animator = null;
    [SerializeField] Button continueButton = null;
    [SerializeField] Button giveUpButton = null;
    [SerializeField] float waitTime = 1;
    [SerializeField] Canvas[] canvases;
    //[SerializeField] RawImage tex = null;
    [SerializeField] Material mat = null;
    [SerializeField] MassImageAnimator imageAnimator = null;
    [SerializeField] AudioClip gameOverSound = null;
    public delegate void EG_GameOverHandler();
    public static event EG_GameOverHandler OnContinue;
    private void Start()
    {

        if (debugMode)
            GameOver();
        else
        {
            TrialDialogueManager.PlayerHasDied += GameOver;
            if(TrialDialogueManager.instance != null)
            {
                OnContinue += TrialDialogueManager.PlayContinue;
            }
            
            EnableObjects(false);
        }
    }
    
    public static void DisableTDMContinue()
    {
        /*
        try
        {
            
        }
        catch
        {

        }
        */
        OnContinue -= TrialDialogueManager.PlayContinue;
    }
    private void OnDisable()
    {
        TrialDialogueManager.PlayerHasDied -= GameOver;
        DisableTDMContinue();
    }
    void EnableObjects(bool to)
    {
        
        foreach (Canvas c in canvases)
            c.enabled = to;
    }
    public void GameOver()
    {
        imageAnimator.RemoveMat();
        animator.Play(animName);
        imageAnimator.LocalFadeImages(1, 0);
        EnableObjects(true);
        SoundManager.instance.PlaySFX(gameOverSound);
    }
    public void EndAnim()
    {
        imageAnimator.ApplyMat(mat);
        StartCoroutine(EndCoroutine());
    }
    IEnumerator EndCoroutine()
    {
        mat.SetFloat("glitchMag", 0.1f);
        yield return new WaitForSeconds(waitTime);
        
        continueButton.gameObject.SetActive(true);
        giveUpButton.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
        yield return new WaitForSeconds(waitTime);
        mat.SetFloat("glitchMag", 0);
        imageAnimator.RemoveMat();
        yield break;
    }
    public void Hide()
    {
        EnableObjects(false);
    }
    public void ContinueGame()
    {
        EventSystem.current.SetSelectedGameObject(null);
        animator.Play(fadeAnimName);
        imageAnimator.LocalFadeImages(0, 1);
        StartCoroutine(Continuing());
        OnContinue?.Invoke();
        //TrialDialogueManager.instance.PlayContinue();
    }
    IEnumerator Continuing()
    {
        yield return new WaitForSeconds(1);
        EnableObjects(false);
        continueButton.gameObject.SetActive(false);
        giveUpButton.gameObject.SetActive(false);
        yield break;
    }
    public void GiveUp() // Called from Give Up Button
    {
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(FadeToMenu());
    }
    IEnumerator FadeToMenu()
    {
        GlobalFade.instance.FadeTo(0.3f);
        yield return new WaitForSecondsRealtime(0.3f);

        UIHandler.CallToTitle();
        
        GameSaver.FirstTimeLoaded = true;
        if(RoomLoader.instance)
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
}
