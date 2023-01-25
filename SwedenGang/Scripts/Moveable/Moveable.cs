//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Moveable : MonoBehaviour
{
    [HideInInspector] MoveData place = new MoveData();
    public MoveData Save()
    {
        place.name = gameObject.name;
        place.position = transform.position;
        place.rotation = transform.eulerAngles;
        return (MoveData)place.Clone();
    }
    public void Load(MoveData place)
    {
        this.place = place;
        transform.position = place.position;
        transform.eulerAngles = place.rotation;
    }
}
[Serializable]
public class MoveData
{
    public string name;
    public Vector3 position;
    public Vector3 rotation;
    public object Clone() => MemberwiseClone();
}
