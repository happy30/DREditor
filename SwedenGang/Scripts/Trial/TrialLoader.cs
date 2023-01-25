//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialLoader : MonoBehaviour
{
    public static TrialData Save()
    {
        TrialData t = new TrialData();
        if (TrialManager.instance == null)
            return t;
        t.actorDatas = RoomManager.SaveTrialActors();
        return t;
    }
    public static void Load(TrialData data)
    {
        RoomManager.LoadTrialActors(data.actorDatas);
    }
    public static void FindDialogue()
    {
        TrialBuilder t = TrialManager.instance.GetCurrentTrial();
        for (int i = 0; i < t.TrialSequences.Count; i++)
        {
            TrialBuilder.TrialSequence s = t.TrialSequences[i];
            if (s.MinigameAsset.GetType() == typeof(TrialDiscussion))
            {
                TrialDiscussion d = (TrialDiscussion)s.MinigameAsset;
                d.CheckIfSaveDialogue();
            }
            if (s.MinigameAsset.GetType() == typeof(TrialDialogue))
            {
                TrialDialogue d = (TrialDialogue)s.MinigameAsset;
                DialogueData.CheckTrialDialogue(d);
            }
        }
    }
    public static void LoadTrialAtDialogueIndex()
    {
        TrialBuilder t = TrialManager.instance.GetCurrentTrial();
        for (int i = 0; i < t.TrialSequences.Count; i++)
        {
            TrialBuilder.TrialSequence s = t.TrialSequences[i];
            if (s.MinigameAsset.GetType() == typeof(TrialDiscussion))
            {
                TrialDiscussion d = (TrialDiscussion)s.MinigameAsset;
                foreach(TrialDialogue td in d.trialDialogues)
                {
                    if (td.name == GameSaver.LoadTrialDialogue.name)
                    {
                        Debug.LogWarning("Calling Load Trial at Index");
                        TrialManager.instance.StartTrialAtIndex(i);
                        return;
                    }
                }
            }
            if (s.MinigameAsset.GetType() == typeof(TrialDialogue))
            {
                TrialDialogue d = (TrialDialogue)s.MinigameAsset;
                if (d.name == GameSaver.LoadTrialDialogue.name)
                {
                    Debug.LogWarning("Calling Load Trial at Index");
                    TrialManager.instance.StartTrialAtIndex(i);
                    return;
                }
            }
        }
    }
}
