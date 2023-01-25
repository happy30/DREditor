using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterBoxCan : MonoBehaviour
{
    static LetterBoxCan Instance = null;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
}
