//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.Events;
/// <summary>
/// Handler Singleton to detect everytime a selected object has been changed
/// References: SoundManager.cs
/// </summary>
public class UIHandler : MonoBehaviour
{
    public static UIHandler instance = null;
    [HideInInspector] public GameObject current;
    public UnityEvent OnChange;
    public delegate void UIDel();
    public static event UIDel ToTitle;
    private static bool ToTitleScreen = false;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        OnChange.AddListener(PlaySelectSound);
    }
    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null && current != EventSystem.current.currentSelectedGameObject)
        {
            OnChange?.Invoke();
            current = EventSystem.current.currentSelectedGameObject;
        }
    }
    void PlaySelectSound() => SoundManager.instance.PlaySelect();
    /// <summary>
    /// Invokes the Event that resets the Pause Menu
    /// </summary>
    public static void CallToTitle()
    {
        ToTitleScreen = true;
        ToTitle?.Invoke();
    }
    public static bool GoingToTitle() => ToTitleScreen;
    //Called from MainMenu.cs to let the game know we've finished ToTitle Functionality
    public static void ReachedTitleScreen() => ToTitleScreen = false;
}
