using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHolder : MonoBehaviour
{
    //public static MenuHolder instance = null;
    private void Awake()
    {
        
        DontDestroyOnLoad(this);
    }
}
