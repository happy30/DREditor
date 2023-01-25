//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DREditor.EventObjects;

/// <summary>
/// A singleton to display the name + visuals of an object the player
/// hovers over.
/// </summary>
public class ItemDisplayer : MonoBehaviour
{
    public static ItemDisplayer instance = null;

    public BoolWithEvent inDialogue = null;
    public TextMeshProUGUI displayText = null;
    public Animator animator = null;
    [Header("Bool names of animators animations for:")]
    [SerializeField] string displayBool = "DisplayName";

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        
    }
    private void Start()
    {
        if (!displayText)
            Debug.LogWarning("Display Text is null");
        if (!animator)
            Debug.LogWarning("Item Displayer's Animator is null");
        if (!inDialogue)
            Debug.LogError("Item Displayer's inDialogue has not been referenced!");
        DialogueAssetReader.OnDialogueStart += HideName;
    }
    private void OnDestroy()
    {
        DialogueAssetReader.OnDialogueStart -= HideName;
    }
    public void DisplayName(string name)
    {
        if (inDialogue.Value)
        {
            Debug.LogWarning("Setting Hide");
            HideName();
            return;
        }
        if(displayText != null)
        {
            displayText.text = name;
        }
        if(animator != null)
        {
            animator.SetBool(displayBool, true);
        }
    }
    public void HideName()
    {
        if (animator != null)
            animator.SetBool(displayBool, false);
    }
    public void ClearText() => displayText.text = ""; // To be called from an animatons event
}
