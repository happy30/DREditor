//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using CharTween;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using NSD;
using DREditor.Camera;
using System.Reflection;
using UnityEditor;
using Cinemachine;

public class WhiteNoiseText : MonoBehaviour
{
    [HideInInspector] public TMP_Text WNText => GetComponent<TMP_Text>();
    public Animation Anim => GetComponent<Animation>();
    BoxCollider Box => GetComponent<BoxCollider>();
    DRTrialCamera Cam => FindObjectOfType<DRTrialCamera>();
    TMProShatter TMPShatter => GetComponent<TMProShatter>();
    bool canShatter = false;
    public NSDBuilder.WhiteNoise data = null;
    public void InitializeWhiteNoise(NSDBuilder.WhiteNoise wn)
    {
        data = wn;
        canShatter = true;
        WNText.text = wn.text;
        WNText.fontSize = wn.size;
        
        transform.parent.rotation = new Quaternion(0, Cam.transform.rotation.y, 0, Cam.transform.rotation.w);
        transform.parent.position = new Vector3(0, Cam.transform.position.y, 0);
        WNText.DOColor(new Color(WNText.color.r, WNText.color.g, WNText.color.b, 1), 0);
        Box.enabled = true;
        WNText.autoSizeTextContainer = true;
        WNText.ForceMeshUpdate();
        //Animation
        AnimationClip clip = new AnimationClip
        {
            legacy = true
        };
        foreach(NSDBuilder.WNAnim wnAnim in wn.anim)
        {
            clip.SetCurve(wnAnim.path, typeof(RectTransform), wnAnim.propertyName, wnAnim.curve);
        }
        Anim.AddClip(clip, "transform");
        Anim.Play("transform");

    }
    public void ShatterWhiteNoise(Vector3 hitPoint)
    {
        if (!canShatter)
            return;
        canShatter = false;
        Box.enabled = false;
        SoundManager.instance.PlaySFX(NSDManager.instance.HitWNOSFX);
        Destroy(GetComponent<CharTweener>());
        Anim.Stop();
        TMPShatter.SetMesh();
        TMPShatter.Shatter(hitPoint - transform.position);
    }
    public void EmpathyFade()
    {
        Box.enabled = false;
        WNText.DOColor(new Color(WNText.color.r, WNText.color.g, WNText.color.b, 0), 1);
    }
    GameObject pathRef;
    /// <summary>
    /// This is for Animating with the Dolly Track on Cinemachine
    /// </summary>
    /// <param name="currentPhrase"></param>
    public void StartAnimation(NSDBuilder.WhiteNoise currentPhrase)
    {
        // Try catch loop to set a bool for safety

        try
        {
            if (currentPhrase.path.points.Length > 0)
            {
                //Debug.Log("Wasn't Null");
                TestCart cart;
                GameObject par = gameObject.transform.parent.gameObject;
                TestCart c = par.GetComponent<TestCart>();
                if (c != null)
                {
                    cart = c;
                    c.m_Path = null;
                    c.m_Speed = 0;
                    c.m_Position = 0;
                }
                else
                    cart = par.AddComponent<TestCart>();
                cart.m_Position = 0;
                cart.m_PositionUnits = CinemachinePathBase.PositionUnits.Normalized;
                pathRef = new GameObject();
                GameObject ch = new GameObject();
                ch.transform.parent = pathRef.transform;
                CinemachinePath p = ch.AddComponent<CinemachinePath>();

                p.gameObject.transform.localPosition = currentPhrase.path.position;
                Vector4 rot = currentPhrase.path.rotation;
                p.gameObject.transform.localEulerAngles = new Vector3(rot.x, rot.y, rot.z);

                pathRef.transform.rotation = new Quaternion(0, Cam.transform.rotation.y, 0, Cam.transform.rotation.w);
                pathRef.transform.position = new Vector3(0, Cam.transform.position.y, 0);
                transform.localPosition = new Vector3(0, -1, 0);

                cart.curve = currentPhrase.path.speedCurve;
                cart.useCurve = true;

                p.m_Waypoints = currentPhrase.path.points;
                cart.m_Path = p;
                cart.m_Speed = 1;
                cart.enabled = true;
            }
        }
        catch
        {

        }
        float count = 0;
        foreach (NSDBuilder.PhraseAnimation.AnimData data in currentPhrase.panim.animations)
        {
            if (data.currentAnimType == NSDBuilder.PhraseAnimation.AnimType.Fade)
            {
                PlayFade(data, data.animationCurves[0]);
                count += data.fStruct.duration;
            }
            /*
            if (data.currentAnimType == NSDBuilder.PhraseAnimation.AnimType.CharacterByCharacter)
            {
                StartCoroutine(data.cBCAnim.AnimateText(WNText, currentPhrase));
            }
            */
        }
    }

    #region Fade Animation
    public void PlayFade(NSDBuilder.PhraseAnimation.AnimData data, AnimationCurve curve)
    {
        StartCoroutine(Fade(data, curve));
    }

    IEnumerator Fade(NSDBuilder.PhraseAnimation.AnimData data, AnimationCurve curve)
    {
        if (data.fStruct.startTime != 0)
        {
            yield return new WaitForSeconds(data.fStruct.startTime);
            WNText.color = new Color(WNText.color.r, WNText.color.g, WNText.color.b, data.fStruct.startFade);
        }
        else
        {
            WNText.color = new Color(WNText.color.r, WNText.color.g, WNText.color.b, data.fStruct.startFade);
        }

        if (data.fStruct.endFade != 0)
        {
            WNText.DOFade(data.fStruct.endFade, data.fStruct.duration).SetEase(curve);
        }
        else
        {
            WNText.DOFade(data.fStruct.endFade, data.fStruct.duration);
        }

        yield return null;
    }
    #endregion

    public void Reset()
    {
        TMPShatter.ResetMesh();
        WNText.text = "";
        WNText.autoSizeTextContainer = false;
        GameObject par = gameObject.transform.parent.gameObject;
        TestCart c = par.GetComponent<TestCart>();
        if (c != null)
        {
            Destroy(c);
        }
        if (pathRef != null)
            Destroy(pathRef.gameObject);
        transform.position = new Vector3(0, 0, 0);
    }
}
