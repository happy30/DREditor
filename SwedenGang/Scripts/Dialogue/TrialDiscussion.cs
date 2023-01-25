//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "DREditor/Dialogues/Trial Discussion", fileName = "TrialDiscussion")]
public class TrialDiscussion : ScriptableObject, IDialogueHolder
{
    public TrialDialogue[] trialDialogues;

    public void CheckIfSaveDialogue()
    {
        foreach (TrialDialogue d in trialDialogues)
            DialogueData.CheckTrialDialogue(d);
    }

    public int GetIndexOfDia(TrialDialogue d)
    {
        for(int i = 0; i < trialDialogues.Length; i++)
        {
            if (d.name == trialDialogues[i].name)
                return i;
        }
        return 0;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorUtility.SetDirty(this);
    }
#endif

}
