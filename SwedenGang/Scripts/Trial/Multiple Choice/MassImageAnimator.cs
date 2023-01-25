using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MassImageAnimator : MonoBehaviour
{
    public delegate void del(float to, float time);
    public static event del FadeAll;

    [SerializeField] List<Image> images = new List<Image>();
    public static void Fade(float to, float time) => FadeAll?.Invoke(to, time);
    public void SetFade() => FadeAll += FadeImages;
    public void UnsetFade() => FadeAll -= FadeImages;
    void FadeImages(float to, float time)
    {
        foreach(Image image in images)
        {
            image.DOFade(to, time);
        }
    }
    public void LocalFadeImages(float to, float time)
    {
        foreach (Image image in images)
        {
            image.DOFade(to, time);
        }
    }
    public void RemoveMat()
    {
        foreach (Image image in images)
        {
            image.material = null;
        }
    }
    public void ApplyMat(Material mat)
    {
        foreach (Image image in images)
        {
            image.material = mat;
        }
    }
}
