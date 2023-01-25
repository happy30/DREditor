//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// For Confirming a selection for TBS and other Trial Minigames
/// </summary>
public class ConfirmUI : MonoBehaviour
{
    public MenuGroup choiceGroup = null;


    public void Show()
    {
        EventSystem.current.SetSelectedGameObject(null);
        choiceGroup.Reveal();
    }
    IEnumerator ShowRoutine()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        
        yield break;
    }
    public void Hide()
    {
        EventSystem.current.SetSelectedGameObject(null);
        choiceGroup.Hide();
    }
    IEnumerator HideRoutine()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        
        yield break;
    }
}
