//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

[System.Serializable]
public class BaseSave
{
    public string SceneName;
    public TimeSave Date;
    public BaseSave()
    {
        Date = new TimeSave();
        SceneName = SceneManager.GetActiveScene().name;
    }
    [Serializable]
    public class TimeSave
    {
        public int Month;
        public int Day;
        public int Year;
        public int Hour;
        public int Minute;
        public int Second;
        public TimeSave()
        {
            DateTime s = DateTime.Now;
            Month = s.Month;
            Day = s.Day;
            Year = s.Year;

            Hour = s.Hour;
            Minute = s.Minute;
            Second = s.Second;
        }
    }
}
