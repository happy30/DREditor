//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DREditor.Dialogues;
/// <summary>
/// The data that would be saved in the event the player 
/// saves during dialogue
/// </summary>
[Serializable]
public class DialogueData
{
    public string DialogueName = "";
    public int DirectToNum = 0;
    public List<bool> Conditional = new List<bool>();
    public int LineNum = 0;
    public int TransitionNum = 0;
    public static bool FoundDialogue = false;
    public static bool CheckDialogue(Dialogue dia)
    {
        if (dia != null)
        {
            bool found = CheckName(dia);

            if (!found && dia.DirectTo.NewDialogue != null)
            {
                return CheckDialogue(dia.DirectTo.NewDialogue);
            }
            if (found)
                return found;
            else
                return false;
        }
        else
            return false;
    }
    public static void CheckTrialDialogue(TrialDialogue dia)
    {
        if (dia != null)
        {
            if (!CheckTrialName(dia) && dia.DirectTo.NewDialogue != null)
            {
                CheckTrialDialogue(dia.DirectTo.NewTrialDialogue);
            }
        }
    }
    public static bool HasInnerDialogue(Dialogue dia)
    {
        return dia.DirectTo.NewDialogue != null || dia.DirectTo.NewTrialDialogue != null
            || (dia.Variable.Enabled && (dia.Variable.NextDialogueTrue != null
            || dia.Variable.NextDialogueFalse != null));
    }
    static bool CheckName(Dialogue dia)
    {
        if (dia != null)
        {
            if (dia.name == GameSaver.CurrentGameData.DialogueData.DialogueName)
            {
                Debug.Log("Dialogue Found: " + GameSaver.CurrentGameData.DialogueData.DialogueName);
                GameSaver.LoadDialogue = dia;
                FoundDialogue = true;
                return true;
            }
        }
        
        return false;
    }
    static bool CheckTrialName(TrialDialogue dia)
    {
        if (dia != null)
        {
            if (dia.name == GameSaver.CurrentGameData.DialogueData.DialogueName)
            {
                Debug.Log("Dialogue Found");
                GameSaver.LoadTrialDialogue = dia;
                FoundDialogue = true;
                return true;
            }
        }

        return false;
    }
}
/// <summary>
/// Found in DialogueData.cs
/// </summary>
public interface IDialogueHolder
{
    /// <summary>
    /// This function is to be used to call DialogueData's static function CheckDialogue
    /// So that the game can find the dialogue the player left off from.
    /// </summary>
    public void CheckIfSaveDialogue();
}
