//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DREditor.Dialogues.Events;

public class ShowItemImage : MonoBehaviour
{
    [SerializeField] Canvas canvas = null;
    [SerializeField] Animator animator = null;
    [SerializeField] RawImage image = null;
    [Header("Names of the Show and Hide Animations")]
    [SerializeField] string showString;
    [SerializeField] string hideString;
    [SerializeField] AudioClip showItemSound = null;
    [SerializeField] AudioClip hideItemSound = null;
    void Start()
    {
        DialogueEventSystem.StartListening("ShowItem", ShowItem);
    }
    void ShowItem(object value = null)
    {
        if (!animator)
            return;
        if (!(value != null))
            animator.Play(hideString);

        if (value != null)
        {
            Texture2D tex = (Texture2D)value;
            
            if (tex != null)
            {
                if (GameSaver.LoadingFile)
                    animator.Rebind();
                image.texture = tex;
                animator.Play(showString);
                if(!GameSaver.LoadingFile)
                    SoundManager.instance.PlaySFX(showItemSound);
            }
            else
            {
                if (GameSaver.LoadingFile)
                    animator.Rebind();
                animator.Play(hideString);
                if (!GameSaver.LoadingFile)
                    SoundManager.instance.PlaySFX(hideItemSound);
            }
        }
    }
    public void EnableCanvas(int i)
    {
        canvas.enabled = i != 0;
    }
}
