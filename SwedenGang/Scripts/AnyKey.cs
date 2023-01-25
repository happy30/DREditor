//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

public class AnyKey : MonoBehaviour
{
    //bool m_ButtonPressed = false;
    [SerializeField] MenuGroup groupOn = null;
    [SerializeField] MenuGroup groupTo = null;
    AnyKeyControl any;
    public void Show()
    {
        InputSystem.onEvent += Test;
    }
    public void Test(InputEventPtr eventPtr, InputDevice device)
    {
        //bool m_ButtonPressed = false;
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            return;
        
        var controls = device.allControls;
        var buttonPressPoint = InputSystem.settings.defaultButtonPressPoint;
        for (var i = 0; i < controls.Count; ++i)
        {
            var control = controls[i] as ButtonControl;
            if(control != null)
            {
                control.ReadValueFromEvent(eventPtr, out var test);
                //Debug.Log("value: " + test);
                
            }
            if (control == null || control.synthetic || control.noisy)
                continue;
            if (control.ReadValueFromEvent(eventPtr, out var value) && value >= 1)
            {
                Debug.Log(value);
                //m_ButtonPressed = true;
                //Debug.Log(m_ButtonPressed);
                groupOn.ChangeGroup(groupTo);
                break;
            }
        }
        
        
    }
    public void Hide()
    {
        Debug.Log("Hide Called");
        InputSystem.onEvent -= Test;
    }
    private void OnDestroy()
    {
        InputSystem.onEvent -= Test;
    }
}
