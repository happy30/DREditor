//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DREditor.Dialogues;
using System;

public class LDialogueSubsequent : SubsequentBase, ISubsequent, IDialogueHolder
{
    public LocalDialogue dialogue;

    public void CheckIfSaveDialogue()
    {
        DialogueData.CheckDialogue(dialogue.dialogue);
        DialogueData.CheckDialogue(dialogue.dialogueIfFlagNotMet);
    }

    public void Load(object ob)
    {
        Subsequent = (Subsequent)Convert.ChangeType(ob, typeof(Subsequent));
        dialogue = (LocalDialogue)Subsequent.localDialogue.Clone();
    }

    public object Save()
    {
        Subsequent s = (Subsequent)Subsequent.Clone();
        s.Type = GetType().ToString();
        //Debug.Log(s.Type);
        s.localDialogue = dialogue;

        return s;
    }
    void ISubsequent.Call()
    {
        LocalDialogue l = dialogue;
        Dialogue dia;
        dia = l.dialogue;
        if (!l.triggered)
        {
            if (l.dialogueBoolTrigger.Count > 0)
            {
                foreach (LocalDialogue.BoolTrigger trigger in l.dialogueBoolTrigger)
                {
                    if (trigger.actorName != "")
                    {
                        Actor a = GameObject.Find(trigger.actorName).GetComponentInChildren<Actor>();
                        if (a != null)
                        {
                            a.TriggerBool(trigger.convoNum);
                        }
                        ItemActor i = GameObject.Find(trigger.actorName).GetComponentInChildren<ItemActor>();
                        if (i != null)
                        {
                            i.TriggerBool(trigger.convoNum);
                        }
                    }
                }
            }

            if (l.requiredProgressionFlag != "")
            {
                if (ProgressionManager.instance.CheckFlag(l.requiredProgressionFlag))
                {
                    l.triggered = true;
                    dia = l.dialogue;
                }
                else
                    dia = l.dialogueIfFlagNotMet;
            }
            else
                dia = l.dialogue;
            l.triggered = true;
            
        }
        Dialogue[] dialogues = new Dialogue[1];
        dialogues[0] = dia;
        if(dia != null)
        {
            GameObject tryActor = GameObject.Find(dialogues[0].Lines[0].Speaker.FirstName);
            Actor actor = null;
            if (tryActor)
                tryActor.GetComponentInChildren<Actor>();
            DialogueAnimConfig.instance.StartDialogue(actor, dialogues);
        }
    }
}
