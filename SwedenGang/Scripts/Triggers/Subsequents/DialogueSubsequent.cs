//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DREditor.Dialogues;
using System;

public class DialogueSubsequent : SubsequentBase, ISubsequent, IDialogueHolder
{
    public List<Dialogue> dialogues = new List<Dialogue>();

    public void CheckIfSaveDialogue()
    {
        foreach(Dialogue d in dialogues)
        {
            DialogueData.CheckDialogue(d);
        }
    }

    public void Load(object ob)
    {
        Subsequent = (Subsequent)Convert.ChangeType(ob, typeof(Subsequent));
        dialogues.Add((Dialogue)Subsequent.ScriptableObject);
    }

    public object Save()
    {
        Subsequent s = (Subsequent)Subsequent.Clone();
        s.Type = GetType().ToString();
        //Debug.Log(s.Type);
        s.ScriptableObject = dialogues[0];

        return s;
    }
    void ISubsequent.Call()
    {
        GameObject tryActor = GameObject.Find(dialogues[0].Lines[0].Speaker.FirstName);
        Actor actor = null;
        if (tryActor)
            tryActor.GetComponentInChildren<Actor>();
        //Debug.LogWarning("Calling StartDialogue from Subsequent");
        DialogueAnimConfig.instance.StartDialogue(actor, dialogues.ToArray());
    }
}
