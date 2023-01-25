//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DREditor.Camera;
/// <summary>
/// Subsequent to move the GameObject Called player
/// </summary>
public class MovePlayerSubsequent : SubsequentBase, ISubsequent
{
    [SerializeField] Transform location = null;
    [SerializeField] bool isStatic = false;
    GameObject player;
    GameObject cam;
    private void Awake()
    {
        player = GameObject.Find("Player");
        cam = GameObject.Find("Main Camera");
        if (isStatic)
        {
            if (location)
            {
                Subsequent.position = location.position;
                Subsequent.rotation = location.rotation;
            }
            else
            {
                Subsequent.position = transform.position;
                Subsequent.rotation = transform.rotation;
            }
        }
    }
    public void Call()
    {
        //Debug.LogWarning("Camera Moved");
        // Code to move the player here
        Debug.LogWarning(Subsequent.position + " Camera and Player Moved");
        PnCCamera p = FindObjectOfType<PnCCamera>();
        p.enabled = false;
        player.transform.SetPositionAndRotation(Subsequent.position, Subsequent.rotation);
        cam.transform.SetPositionAndRotation(Subsequent.position, Subsequent.rotation);
    }

    public void Load(object ob)
    {
        Subsequent = (Subsequent)Convert.ChangeType(ob, typeof(Subsequent));

    }

    public object Save()
    {
        Subsequent s = (Subsequent)Subsequent.Clone();
        s.Type = GetType().ToString();
        if (location)
        {
            s.position = location.position;
            s.rotation = location.rotation;
        }
        else
        {
            s.position = transform.position;
            s.rotation = transform.rotation;
        }
        return s;
    }

}
