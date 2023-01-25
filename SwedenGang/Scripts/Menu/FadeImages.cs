//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeImages : MonoBehaviour
{
    [SerializeField] List<Image> images = new List<Image>();
    [Header("Options")]
    [SerializeField] float fadeDuration = 0.4f;

    public void FadeImage(float to)
    {
        foreach(Image i in images)
        {
            i.DOFade(to, fadeDuration);
        }
    }
    
}
