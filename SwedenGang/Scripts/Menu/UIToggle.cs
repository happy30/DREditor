//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
/// <summary>
/// The purpose of this class is to emulate the Toggles in the option's menu in Danganronpa
/// While using the simplicity of Unity's toggle function with UI
/// </summary>
[RequireComponent(typeof(UIDisplayOption))]
public class UIToggle : Toggle
{
    [SerializeField] Selectable off = null;
    [SerializeField] UIOptionGroup optionGroup = null;
    private UIDisplayOption offOption;
    private UIDisplayOption toggleOption;
    protected override void Awake()
    {
        base.Awake();
        toggleOption = GetComponent<UIDisplayOption>();
        if (!off)
        {
            Debug.LogError("There's no off Selectable hooked or set up!");
            Debug.LogError("Remember to set these fields unity inspector must be in debug mode");
        }
        else
        {
            offOption = off.GetComponent<UIDisplayOption>();
            if (!offOption)
                Debug.LogError("Your Off Selectable must have a UIDisplayOption Component!");
        }
        if (!optionGroup)
        {
            Debug.LogError("There's no UIOptionGroup hooked or set up!");
            Debug.LogError("Remember to set these fields unity inspector must be in debug mode");
        }
        onValueChanged.AddListener(Eval);
        toggleOption.Selection.AddListener(ToggleTrue);
        offOption.Selection.AddListener(ToggleFalse);
    }
    protected override void Start()
    {
        base.Start();
        
    }
    public override void OnSubmit(BaseEventData eventData)
    {
        // left empty on purpose so that inputing won't change isOn
    }
    void ToggleTrue() => isOn = true;
    void ToggleFalse() => isOn = false;
    public void Eval(bool param)
    {
        try
        {
            if (EventSystem.current.currentSelectedGameObject != gameObject)
            {
                
            }
        }
        catch
        {
            return; // This only happens in the editor when you turn off play mode
        }
        if (param)
        {
            if (EventSystem.current.currentSelectedGameObject != gameObject)
            {
                toggleOption.InvokeSelect();
                //off.OnDeselect(null);
                UIDisplayChanger op = off.GetComponent<UIDisplayChanger>();
                if (op)
                {
                    op.Deselect();
                }
                //Select();
                optionGroup.RewireNavigation(this);
                //Debug.LogWarning("Called true");
            }
        }
        else
        {
            if (EventSystem.current.currentSelectedGameObject != off.gameObject)
            {
                offOption.InvokeSelect();
                UIDisplayChanger op = gameObject.GetComponent<UIDisplayChanger>();
                if (op)
                {
                    op.Deselect();
                }
                //off.Select();
                optionGroup.RewireNavigation(off);
                //Debug.LogWarning("Called false");
            }

        }
    }
    
    public void ChangeToggle(bool to) // in case needs to be changed from outside source
    {
        isOn = to;
    }
    protected override void OnDestroy()
    {
        //onValueChanged.RemoveListener(Eval);
        //toggleOption.Selection.RemoveListener(ToggleTrue);
        //offOption.Selection.RemoveListener(ToggleFalse);
        base.OnDestroy();
    }
}
