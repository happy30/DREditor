//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OnLoadActuator : ActuatorBase, IActuator
{
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
            Debug.Log("Actuator is being Triggered");
            Actuator.Triggered = true;
            base.Call();
        }
    }
    public void Load(object ob)
    {
        Actuator = (Actuator)Convert.ChangeType(ob, typeof(Actuator));

        //Debug.Log(Actuator.Triggered);
        transform.position = Actuator.position;

    }

    object ITrack.Save()
    {
        Actuator a = (Actuator)Actuator.Clone();
        a.Type = GetType().ToString();
        a.Triggered = Actuator.Triggered;
        return a;
    }
}
