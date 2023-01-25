//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DREditor.Camera;
using DREditor.EventObjects;

public class ResetPlayer : MonoBehaviour
{
    [SerializeField] BoolWithEvent inTPFD = null;
    [SerializeField] BoolWithEvent inMenu = null;// Added 11-14
    void Start()
    {
        Debug.LogWarning("RESET PLAYER HAS ACTIVATED");
        PnCCamera cam = FindObjectOfType<PnCCamera>();
        if (cam)
            cam.enabled = false;
        if (PlayerManager.instance != null)
            PlayerManager.instance.DisableMovement();
        CharacterController c = FindObjectOfType<CharacterController>();
        if(c != null)
        {
            c.enabled = false;// Added 11-14
            c.gameObject.transform.position = new Vector3(0, 1.5f, 0);
            c.enabled = true; // Added 11-14
        }
        if(GameManager.instance != null)
            GameManager.instance.ChangeMode(GameManager.Mode.ThreeD);
        if (PlayerManager.instance != null)
            PlayerManager.instance.EnableControlMono(false); // If any Movement issues happen after 11-16 this might be the cause
        if (inTPFD != null)
            inTPFD.Value = false;
        if(inMenu != null)
            inMenu.Value = false;// Added 11-14
        
    }
}
