//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GlobalFade : MonoBehaviour
{
    public static GlobalFade instance = null;
    //[SerializeField] Canvas Canvas = null;
    [SerializeField] Image Fade = null;
    public bool IsDark = false;
    public bool isTransitioning = false;
    /* Is Transitioning was to fix a bug where going from investigation to trial with
     * the last line being a CG or Video would fade an additional time, otherwise Fade works perfectly fine
     * so if the game has any fade issues after 10/20/22 then just remove it and find a new way to fix 
     * going from investigation to trial prep screen during CGs on the final line.
     * 
     */
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    /// <summary>
    /// Fades to black
    /// </summary>
    /// <param name="time"></param>
    public void FadeTo(float time, bool white = false) => StartCoroutine(FadeToRoutine(time, white));
    IEnumerator FadeToRoutine(float time, bool white)
    {
        if (isTransitioning)
        {
            //Debug.LogWarning("Failed Attempt to Fade TO");
            yield break;
        }
        isTransitioning = true;
        //Debug.LogWarning("Called Fade");
        SetColor(white);
        if (DialogueAssetReader.instance != null && DialogueAssetReader.instance.ffMode)
            time /= 2;
        IsDark = true;
        //Canvas.enabled = true;
        Fade.DOFade(1, time)
            .SetUpdate(true);
        yield return new WaitForSeconds(time);
        isTransitioning = false;
        yield break;
    }
    /// <summary>
    /// Fades out of black
    /// </summary>
    /// <param name="time"></param>
    public void FadeOut(float time, bool white = false) => StartCoroutine(FadeOutRoutine(time, white));
    IEnumerator FadeOutRoutine(float time, bool white)
    {
        if (isTransitioning)
        {
            //Debug.LogWarning("Failed Attempt to Fade OUT");
            yield break;
        }
        isTransitioning = true;
        SetColor(white);
        //Debug.LogWarning("FADE OUT WAS CALLED");
        if (DialogueAssetReader.instance != null && DialogueAssetReader.instance.ffMode)
            time /= 2;
        IsDark = false;
        //Canvas.enabled = true;
        Fade.DOFade(0, time)
            .SetUpdate(true);
        yield return new WaitForSeconds(time);
        //Canvas.enabled = false;
        isTransitioning = false;
        yield break;
    }
    void SetColor(bool white)
    {
        if (white)
            Fade.color = new Color(255, 255, 255, Fade.color.a);
        else
            Fade.color = new Color(0, 0, 0, Fade.color.a);
    }
    public void FadeOutOnLoad() // For simple fadeouts that you can add to an event with void delegate with no parameters
    {
        if (IsDark)
            FadeOut(1);
    }
}
