//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIToggleFade : MonoBehaviour
{
    [Header("The starting sprite of the image will be the first so you just need to input the second")]
    [SerializeField] RawImage image = null;
    [SerializeField] Texture tex = null;
    [SerializeField] float transitionTime = 0.5f;
    Texture first = null;
    private void Start()
    {
        first = image.texture;
    }
    public void Toggle()
    {
        StartCoroutine(Anim());
    }
    IEnumerator Anim()
    {
        image.DOFade(0, transitionTime/2).SetUpdate(true);
        yield return new WaitForSecondsRealtime(transitionTime / 2);
        image.texture = image.texture == first ? tex : first;
        yield return new WaitForSecondsRealtime(transitionTime / 2);
        image.DOFade(1, transitionTime / 2).SetUpdate(true);
        yield break;
    }
}
