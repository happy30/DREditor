//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DREditor.Progression;

/// <summary>
/// Checks if flags are equal to "callIfFlagsAre" and if they are then activates
/// </summary>
public class OLFlagActuator : ActuatorBase, IActuator
{
    [SerializeField] bool callIfFlagsAre = true;
    [SerializeField] List<string> flagList = new List<string>();
    private void Awake()
    {
        RoomLoader.OnLoad += OnLoadCall;
    }
    private void OnDestroy()
    {
        RoomLoader.OnLoad -= OnLoadCall;
    }
    void OnLoadCall()
    {
        if (Actuator.Triggered == false)
        {
            var flags = flagList.Where(n => ProgressionManager.instance.CheckFlag(n) == callIfFlagsAre);
            if (flags.Count() == flagList.Count)
            {
                //Debug.Log();
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
        callIfFlagsAre = Actuator.boolValue;
    }

    object ITrack.Save()
    {
        Actuator a = (Actuator)Actuator.Clone();
        a.Type = GetType().ToString();
        a.stringList = flagList;
        a.boolValue = callIfFlagsAre;
        return a;
    }
}
