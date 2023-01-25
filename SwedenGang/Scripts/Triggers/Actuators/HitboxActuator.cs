//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxActuator : ActuatorBase, IActuator
{
    [SerializeField] BoxCollider Hitbox = null;

    //[SerializeField] BoxCollider Barrier = null;
    private void Awake()
    {
        RoomLoader.OnLoad += ActivateHitBox;
    }
    private void OnDestroy()
    {
        RoomLoader.OnLoad -= ActivateHitBox;
    }
    void ActivateHitBox()
    {
        if (Actuator.Triggered == false)
        {
            Hitbox.enabled = true;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!Actuator.Triggered)
        {
            Actuator.Triggered = !Actuator.Triggered;
            Hitbox.enabled = false;
            base.Call();
        }
    }

    public void Load(object ob)
    {
        Actuator = (Actuator)Convert.ChangeType(ob, typeof(Actuator));
        
        Debug.Log(Actuator.Triggered);
        transform.position = Actuator.position;
        Hitbox = gameObject.AddComponent<BoxCollider>();
        Hitbox.enabled = false;
        Hitbox.isTrigger = true;
        Hitbox.size = Actuator.HitBoxSize;

    }

    object ITrack.Save()
    {
        Actuator a = (Actuator)Actuator.Clone();
        a.Type = GetType().ToString();
        a.HitBoxSize = Hitbox.size;
        a.position = transform.position;
        return a;
    }
}
