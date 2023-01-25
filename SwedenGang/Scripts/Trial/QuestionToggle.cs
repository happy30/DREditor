//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class QuestionToggle : MonoBehaviour
{
    [Header("Animator that holds everything")]
    [SerializeField] Animator animator = null;

    [Header("Question Bar Holding the Question Text")]
    [SerializeField] Animator qAnimator = null;
    [SerializeField] TextMeshProUGUI questionText = null;

    [Header("Animator Names for Showing and Hiding States")]
    [SerializeField] string showName = "Show";
    [SerializeField] string hideName = "Hide";

    [Tooltip("How long the game waits till the player can toggle again")]
    [SerializeField] float toggleWaitTime = 1;
    [Header("Toggle Fade for Showing and hiding question text")]
    [SerializeField] UIToggleFade toggleFade = null;

    bool showing = false;
    #region Controls (Has Awake Func)
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
        _controls.UI.BackLog.started -= ToggleQuestion;
#endif
    }
    #endregion
    public void Activate()
    {
        animator.Play(showName);
        _controls.UI.BackLog.started += ToggleQuestion;
    }
    public void Deactivate()
    {
        _controls.UI.BackLog.started -= ToggleQuestion;
        HideVisuals();
    }
    public void SetQuestion(string text)
    {
        questionText.text = text;
    }
    public void HideVisuals()
    {
        _controls.UI.BackLog.started -= ToggleQuestion;
        if (showing)
        {
            qAnimator.Play(hideName);
            toggleFade.Toggle();
            showing = !showing;
        }
        animator.Play(hideName);
    }
    void ToggleQuestion(CallbackContext ctx)
    {
        _controls.UI.BackLog.started -= ToggleQuestion;
        toggleFade.Toggle();
        if (showing)
            qAnimator.Play(hideName);
        else
            qAnimator.Play(showName);
        showing = !showing;
        StartCoroutine(Wait());
    }
    IEnumerator Wait()
    {
        yield return new WaitForSecondsRealtime(toggleWaitTime);
        _controls.UI.BackLog.started += ToggleQuestion;
        yield break;
    }
}
