//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueIcon : MonoBehaviour
{
    //[SerializeField] Image image = null;
    [SerializeField] Animator animator = null;
    public bool on = false;
    private void OnEnable()
    {
        if (on)
            TurnOn();
        else
            TurnOff();
    }
    public void TurnOn()
    {
        animator.SetBool("On", true);
        on = true;
    }
    public void TurnOff()
    {
        animator.SetBool("On", false);
        on = false;
    }
    public void ResetAnimator()
    {
        if(animator != null)
            animator.Rebind();
    }

}
