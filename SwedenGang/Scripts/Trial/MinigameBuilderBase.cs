//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class MinigameBuilderBase : ScriptableObject
{
    public TimerDiff[] times = new TimerDiff[3]
    {
        new TimerDiff(GameManager.Difficulty.Kind),new TimerDiff(GameManager.Difficulty.Normal),new TimerDiff(GameManager.Difficulty.Mean)
    };

    #region TimerDiff
    public void SetTimerBasedOnDifficulty(TrialTimer timer)
    {
        TimerDiff timed = GetTime();
        float time = (timed.min * 60) + timed.sec;
        timer.StartTimer(time);
    }
    TimerDiff GetTime()
    {
        return times.Where(n => n.difficulty == GameManager.instance.logicDifficulty).ElementAt(0);
    }
    #endregion

}
