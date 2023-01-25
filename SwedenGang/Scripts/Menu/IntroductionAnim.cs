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
public class IntroductionAnim : MonoBehaviour
{
    [SerializeField] AudioSource opening = null;
    [SerializeField] TMP_Text introText = null;
    [SerializeField] string Scene = "";
    private void Start()
    {
        opening.Play();
        StartCoroutine(IntroAnim());
        Cursor.lockState = CursorLockMode.Locked;
    }
    IEnumerator IntroAnim()
    {
        introText.DOFade(1, 1.5f);

        yield return new WaitForSeconds(7);

        introText.DOFade(0, 1.5f);

        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(Scene);
        yield break;
    }
}
