//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Monobehaviour to be attached to a Canvas, have that canvas hold the animations of the different char intros
/// </summary>
public class CharIntroAnim : MonoBehaviour
{
    public static CharIntroAnim instance = null;
    [SerializeField] Animator animator = null;
    [SerializeField] Canvas canvas = null;
    [SerializeField] AudioClip introSound = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(this);
    }
    private void Start()
    {
        DialogueEventSystem.StartListening("CharIntro", CallIntro);
    }
    public void CallIntro(object o)
    {
        CITuple data = (CITuple)o;
        
        Debug.Log("If this is called we would start the Intro Animation for character: " + data.charName);
        StartCoroutine(ShowIntro(data.charName));
    }
    IEnumerator ShowIntro(string name)
    {
        DialogueAnimConfig.instance.HideDialogueBox(true);
        yield return new WaitForSeconds(0.2f);
        if (introSound != null)
            SoundManager.instance.PlaySFX(introSound);
        animator.Play(name);
        canvas.enabled = true;
        yield break;
    }
    public void EndOfAnim()
    {
        //animator.StopPlayback();
        animator.Play("Empty");
        canvas.enabled = false;
        DialogueAnimConfig.instance.HideDialogueBox(false);
        DialogueAssetReader.instance.trigger = true;
    }
}
