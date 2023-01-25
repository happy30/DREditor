//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class PopUp : MenuGroup
{
    [Header("Put your mask here")]
    [SerializeField] Image Background = null;
    [SerializeField] float maskSizeX;
    [SerializeField] float maskSizeY;
    public TextMeshProUGUI Question = null;
    [SerializeField] TextMeshProUGUI ChoiceOne = null;
    [SerializeField] TextMeshProUGUI ChoiceTwo = null;
    //[SerializeField] MenuGroup backGroup = null;
    [SerializeField] GameObject First = null;
    [SerializeField] Selectable One;
    [SerializeField] Selectable Two;
    //[SerializeField] Animator animator = null;
    

    public override void Start()
    {
        One.enabled = false;
        Two.enabled = false;
        UIHandler.ToTitle += RemoveBackInput;
        
    }
    public MenuGroup GetBackGroup() => backGroup;
    public override void Reveal()
    {
        MenuGroup.GroupFinished += Check;
        if (animator)
        {
            animator.SetTrigger("Show");
            animator.ResetTrigger("Hide");
        }
        else
            Background.rectTransform.DOSizeDelta(new Vector2(maskSizeX, maskSizeY), 0.5f).SetUpdate(true);
        Question.DOFade(1, 1).SetUpdate(true);
        ChoiceOne.DOFade(1, 1).SetUpdate(true);
        ChoiceTwo.DOFade(1, 1).SetUpdate(true);
        One.enabled = true;
        Two.enabled = true;
        EventSystem.current.SetSelectedGameObject(First);
        UIHandler.instance.current = First;
        AddBackInput();
        
        SoundManager.instance.PlayPopUp();
        StartEvents?.Invoke();
    }
    public override void Hide()
    {
        One.enabled = false;
        Two.enabled = false;
        if (animator)
        {
            animator.SetTrigger("Hide");
            animator.ResetTrigger("Show");
        }
        else
            Background.rectTransform.DOSizeDelta(new Vector2(maskSizeX, 0), 0.5f).SetUpdate(true);
        Question.DOFade(0, 1).SetUpdate(true);
        ChoiceOne.DOFade(0, 1).SetUpdate(true);
        ChoiceTwo.DOFade(0, 1).SetUpdate(true);
        
        RemoveBackInput();

        if (backGroup)
        {
            backGroup.AddBackInput();
            backGroup.EvaluateSelect();
            //EventSystem.current.SetSelectedGameObject(backGroup.first);
        }
        
        EndEvents?.Invoke();
    }
    public override void BackGroup(InputAction.CallbackContext context)
    {
        //Debug.LogWarning("Calling Back Group for: " + gameObject.name);
        //backGroup.EvaluateSelect();
        //EventSystem.current.SetSelectedGameObject(backGroup.first);
        SoundManager.instance.PlayCancel();
        Hide();
    }
    public void Check(MenuGroup group) // If this causes a problem then you can just remove
    {
        GameObject g = EventSystem.current.currentSelectedGameObject;
        if (group == backGroup && (g == One.gameObject || g == Two.gameObject) && backGroup)
        {
            Debug.LogWarning("Check Called on Pop up");
            backGroup.QuickSelection();
        }
        MenuGroup.GroupFinished -= Check;
    }
    public override void RemoveBackInput()
    {
        if (backGroup)
            _controls.UI.Cancel.started -= BackGroup;
    }
    public override void AddBackInput()
    {
        if (backGroup)
            _controls.UI.Cancel.started += BackGroup;
    }
    public void PlaySubmitSFX() => SoundManager.instance.PlaySubmit();
}
