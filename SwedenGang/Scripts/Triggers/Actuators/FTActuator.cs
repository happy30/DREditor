//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DREditor.Progression;

/// <summary>
/// If a written string of a flag is triggered then this activates
/// </summary>
public class FTActuator : ActuatorBase, IActuator
{
    [SerializeField] List<string> flagList = new List<string>();
    private void Awake()
    {
        ProgressionManager.FlagTriggered += CheckFlag;
    }
    private void OnDestroy()
    {
        ProgressionManager.FlagTriggered -= CheckFlag;
    }
    void CheckFlag(string name)
    {
        if (Actuator.Triggered == false)
        {
            var flags = flagList.Where(n => n == name);
            if (flags.Count() > 0)
            {
                Actuator.Triggered = true;
                base.Call();
            }
        }
    }
    public void Load(object ob)
    {
        Actuator = (Actuator)Convert.ChangeType(ob, typeof(Actuator));

        //Debug.Log(Actuator.Triggered);
        transform.position = Actuator.position;
        flagList = Actuator.stringList;
    }

    object ITrack.Save()
    {
        Actuator a = (Actuator)Actuator.Clone();
        a.Type = GetType().ToString();
        a.stringList = flagList;
        return a;
    }
}