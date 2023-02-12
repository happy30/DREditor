using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DREditor.Dialogues.Events;

public class UIFade : MonoBehaviour
{
    public static bool OnScreenFade
    {
        get
        {
            return (_OnTransition || _ScreenCovered);
        }
    }
    public Image FadeCover;

    private static bool _ScreenCovered = false;
    private static bool _OnTransition = false;
    private float speed;

    private void OnEnable()
    {
        DialogueEventSystem.StartListening("FadeToBlack", FadeToBlack);
        DialogueEventSystem.StartListening("ScreenFlash", Flash);
        DialogueEventSystem.StartListening("FadeOut", FadeOut);
    }

    private void OnDisable()
    {
        DialogueEventSystem.StopListening("FadeToBlack", FadeToBlack);
        DialogueEventSystem.StopListening("ScreenFlash", Flash);
        DialogueEventSystem.StopListening("FadeOut", FadeOut);
    }

    public void FadeToBlack(object FTBValue)
    {
        _ScreenCovered = true;
        speed = (float)FTBValue;
        
        if (!_OnTransition) StartCoroutine(FadeAnimation(speed, 1f, Color.black));
        else
        {
            StopCoroutine("FadeAnimation");
            StartCoroutine(FadeAnimation(speed, 1f, Color.black));
        }
    }

    public void FadeOut(object FTBValue = null)
    {
        _ScreenCovered = false;
        speed = (float)FTBValue;
        if (!_OnTransition) StartCoroutine(FadeAnimation(speed, 0f, Color.black));
        else
        {
            StopCoroutine("FadeAnimation");
            StartCoroutine(FadeAnimation(speed, 0f, Color.black));
        }
    }

    public void Flash(object FLSValue)
    {
        StartCoroutine(FadeAnimation(10f, 1f, Color.white));
    }

    IEnumerator FadeAnimation(float speed, float endalpha, Color _color)
    {
        _OnTransition = true;
        float _startTime = 0f;
        float _startalpha = FadeCover.color.a;

        while (_startTime < speed)
        {
            if (speed == 0)
            {
                _startTime = speed;
                continue;
            }
            else
            {
                FadeCover.color = new Color(_color.r, _color.g, _color.b, Mathf.Lerp(_startalpha, endalpha, _startTime / speed));
                _startTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        FadeCover.color = new Color(_color.r, _color.g, _color.b, endalpha);
        _OnTransition = false;
    }
}
