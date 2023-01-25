//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationTitle : MonoBehaviour
{
    [SerializeField] string title;
    private void Start()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SetLocation(title);
        }
    }
}
