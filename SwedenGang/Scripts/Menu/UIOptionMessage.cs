//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DREditor.PlayerInfo;

public class UIOptionMessage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI box = null;
    private IEnumerator coroutine;
    // This stuff is set up like this in case all messages will be displayed over time and not just 
    // pasted to the screen
    private void Awake()
    {
        if (!box)
        {
            Debug.LogError("UI Option Message does not have a TMP Box!");
        }
        
    }
    public void PlayTextSpeedMessage(string message)
    {
        ClearBox();
        coroutine = DisplayText(message, PlayerInfo.instance.settings.TextSpeed);
        StartCoroutine(coroutine);
    }
    public void PlayMessage(string message)
    {
        if (coroutine != null)
        {
            ClearBox();
            box.text = message;
        }
        else
            box.text = message;
    }
    
    public void ClearBox()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        box.text = "";
    }

    IEnumerator DisplayText(string message, float speed)
    {
        char[] letters = message.ToCharArray();
        foreach(char let in letters)
        {
            box.text += let;
            yield return new WaitForSecondsRealtime(speed * 0.01f);
        }
        yield break;
    }
}
