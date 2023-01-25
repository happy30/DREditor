//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.EventObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DREditor.PlayerInfo;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.Video;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    Canvas Canvas => GetComponent<Canvas>();
    //bool isPaused = false;
    [SerializeField] AudioClip PauseSFX = null;
    [SerializeField] AudioClip UnPauseSFX = null;
    [SerializeField] BoolWithEvent InMenu;
    [SerializeField] BoolWithEvent InDialogue;
    //[SerializeField] GameObject FirstItem = null;
    [SerializeField] MenuGroup FirstGroup = null;
    [Header("Optional")]
    [Tooltip("For having an animation to lead into the first group." +
        " \n Called by Animators Triggers \"Show\" and \"Hide\" " +
        " \n Make sure you call the Show/HidePauseGroup function from an animation event if you" +
        " use the animator!")]
    [SerializeField] Animator animator = null;
    [Tooltip("Must fill out both player and the texture to work")]
    [SerializeField] VideoPlayer videoPlayer = null;
    [SerializeField] RawImage videoTexture = null;

    [Tooltip("The index of the Truth Bullet Menu corresponding to it's Pause Option Access index")]
    [SerializeField] int tBMIndex = 1;

    public static PauseMenu instance = null;

    public delegate void VoidDel();
    public static event VoidDel OnResumeEnd;
    public static bool inPause = false;
    DRControls _controls;

    private void Awake()
    {
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject.transform.parent.gameObject);
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
        UIHandler.ToTitle += ResetPauseMenu;
        _controls.UI.Pause.started += EvaluatePause;
        if (videoTexture)
            videoTexture.enabled = false;
        if (videoPlayer)
            videoPlayer.Prepare();
    }
    void ResetPauseMenu()
    {
        _controls.UI.Cancel.started -= UnPause;
        Canvas.enabled = false;
        if (animator)
        {
            animator.gameObject.GetComponent<Canvas>().enabled = false;
            animator.Rebind();
            animator.Update(0f);
        }
        inPause = false; // This might cause problems, added 11-14 to fix 
        // loading a file from the pause menu then pressing the back log disables the pause menu
    }
    void EvaluatePause(CallbackContext context)
    {
        //Debug.LogWarning("Evaluate Pause Called: ");
        if (PlayerInfo.instance.Info.pauseAccess && !Canvas.enabled && !GameManager.instance.cantBeInMenu
            && !Door.inLeaveProcess && !InMenu.Value && !Resuming && !ObserveManager.ChangingObserve)
        {
            inPause = true;

            StartPause();
            SoundManager.instance.StopAllButMusic();
            PlayPauseSound();
        }
        else
            DebugLocks();
    }
    public void PlayPauseSound()
    {
        SoundManager.instance.PlaySFX(PauseSFX);
    }
    void DebugLocks()
    {
        Debug.Log(!Canvas.enabled);
        Debug.Log(!GameManager.instance.cantBeInMenu);
        Debug.Log(!Door.inLeaveProcess);
        Debug.Log(!InMenu.Value);
        Debug.Log(!Resuming);
        Debug.Log(!ObserveManager.ChangingObserve);
    }
    public void StartPause()
    {
        InMenu.Value = true;
        inPause = true;
        SoundManager.instance.StopVoiceLine();
        if (!animator)
            Canvas.enabled = true;
        Time.timeScale = 0;
        EvaluatePauseOptions();

        

        if (animator)
            animator.SetTrigger("Show");
        else
            ShowPauseGroup();
    }
    void EvaluatePauseOptions()
    {
        PlayerInfo.Information info = PlayerInfo.instance.Info;
        
        for (int i = 0; i < FirstGroup.butts.Count && i < info.pauseOptions.Length; i++)
        {
            if (FirstGroup.butts[i] != null && 
                FirstGroup.butts[i].gameObject.activeSelf != info.pauseOptions[i])
            {
                FirstGroup.butts[i].gameObject.SetActive(info.pauseOptions[i]);
            }
            if (i == tBMIndex && info.foundBullets.Count != 0)
            {
                FirstGroup.butts[i].gameObject.SetActive(true);
                //Debug.LogWarning("To True");
            }

            if (i == tBMIndex && info.pauseOptions[i] && info.foundBullets.Count == 0)
            {
                FirstGroup.butts[i].gameObject.SetActive(false);
                //FirstGroup.butts[i].interactable = false;
                //Debug.LogWarning("To False");
            }
        }
    }
    /// <summary>
    /// Starts up the first group. Intended to be called from an animation event
    /// from the Pause Menus Optional Animator.
    /// </summary>
    public void ShowPauseGroup()
    {
        Canvas.enabled = true;
        if (videoPlayer && videoTexture)
        {
            videoPlayer.Play();
            videoTexture.enabled = true;
            // Could potentially make an option to fade the video texture rather than enabling it.
        }
        FirstGroup.Reveal();
        _controls.UI.Cancel.started += UnPause;
    }
    public static bool Resuming = false;
    void UnPause(CallbackContext context)
    {
        if (FirstGroup.isActive && !MenuGroup.Changing)
        {
            EventSystem.current.SetSelectedGameObject(null);
            MenuGroup[] groups = gameObject.transform.parent.GetComponentsInChildren<MenuGroup>();
            foreach(MenuGroup group in groups)
            {
                group.RemoveBackInput();
            }
            Debug.LogWarning("Unpausing");
            Resuming = true;
            _controls.UI.Cancel.started -= UnPause;
            HidePauseGroup();
        }
    }
    public void HidePauseGroup() => StartCoroutine(Resume());
    IEnumerator Resume()
    {
        
        FirstGroup.Hide();
        SoundManager.instance.PlaySFX(UnPauseSFX);
        if (animator)
        {
            animator.SetTrigger("Hide");

            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(animator.GetCurrentAnimatorStateInfo(0).length + 1);
            //Debug.LogWarning("Finished Resume");

            //Debug.LogWarning("Animator Found");
            if(animator.GetCurrentAnimatorStateInfo(0).length == float.PositiveInfinity)
                yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).length != float.PositiveInfinity);
            //yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).length > animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            //Debug.LogWarning("Animator Finished");
        }

        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime * 2);
        
        
        InMenu.Value = false;
        if(!animator)
            Canvas.enabled = false;
        Time.timeScale = 1;
        
        OnResumeEnd?.Invoke();
        Resuming = false;
        inPause = false;
        yield break;
    }
    
    public void ConfirmSelect()
    {
        Debug.Log("Clicked");
    }
    private void OnDestroy()
    {
        UIHandler.ToTitle -= ResetPauseMenu;
        _controls.UI.Pause.started -= EvaluatePause;
    }
}
