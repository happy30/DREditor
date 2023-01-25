//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorHelper : MonoBehaviour
{
    [SerializeField] List<string> boolStrings = new List<string>();


    Animator animator = null;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void SetAll(bool to)
    {
        foreach (string s in boolStrings)
            animator.SetBool(s, to);
    }
}
