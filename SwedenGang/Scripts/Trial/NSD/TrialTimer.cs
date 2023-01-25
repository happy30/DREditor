//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class TrialTimer : MonoBehaviour
{
    [SerializeField] RawImage rawImage = null;
    public delegate void voidDel();
    public static event voidDel TimeUp;
    public TextMeshProUGUI TimerText => GetComponent<TextMeshProUGUI>();
    float startTime = 2 * 60;
    float timeRemaining;
    bool timerOn = false;
    //float timeWhenReduced = 3;
    //float timeWhenAdded = 2;
    //[SerializeField] int milliDecimalAmount = 3;
    float minutes;
    float seconds;
    float milliseconds;
    TextMeshPro tex = null;
    private void Awake()
    {
        if(TimerText == null)
            tex = GetComponent<TextMeshPro>();
    }
    void Update()
    {
        if (timerOn)
        {
            if (timeRemaining >= 0)
            {
                timeRemaining -= Time.unscaledDeltaTime;// if speed up not alter then unscaledDeltaTime
                minutes = Mathf.FloorToInt(timeRemaining / 60);
                seconds = Mathf.FloorToInt(timeRemaining % 60);
                milliseconds = (float)Math.Round(timeRemaining % 1 * 1000, 3);
                if (minutes < 0 || seconds < 0 || milliseconds < 0)
                {
                    timeRemaining = -1;
                    return;
                }
                if(tex != null)
                    tex.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
                else
                    TimerText.text = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);

            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                if(tex != null)
                    tex.text = "00:00.000";
                else
                    TimerText.text = "00:00.000";
                //TimerText.text = "00:00.000";
                timerOn = false;
                TimeUp?.Invoke();
                ResetTimer();
            }
        }
    }
    public void SetTimer(float startingTime)
    {
        timeRemaining = startingTime;
        startTime = startingTime;
    }
    public void StartTimer(float startingTime = 2 * 60)
    {
        timeRemaining = startingTime;
        startTime = startingTime;
        timerOn = true;
    }
    public void StopTimer()
    {
        timerOn = false;
        if (tex != null)
            tex.text = "";
        else
            TimerText.text = "";
        //TimerText.text = "";
    }
    public void PauseTimer() => timerOn = false;
    public void ResumeTimer() => timerOn = true;
    public void ResetTimer() => timeRemaining = startTime;
    public void RemoveTime(float lostTime) => timeRemaining -= lostTime;
    public void AddTime(float timeToAdd) => timeRemaining += timeToAdd; // Base of 5 Seconds but manipulate based on stat
    public void Anim(float to, float time)
    {
        if (rawImage != null)
            rawImage.DOFade(to, time).SetUpdate(true);
        TimerText.DOFade(to, time).SetUpdate(true);
    }
}
