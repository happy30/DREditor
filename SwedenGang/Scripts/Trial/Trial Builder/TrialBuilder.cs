//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Gates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
[CreateAssetMenu(menuName = "DREditor/Trials/Trial Builder Asset", fileName = "Trial Asset")]
public class TrialBuilder : ScriptableObject
{
    public List<TrialSequence> TrialSequences = new List<TrialSequence>();
    public string EndSceneName = "";
    public Gate EndGate = null;
    [System.Serializable]
    public class TrialSequence
    {
        public string SequenceType; // For Editor, is minigame type
        public int SequenceTypeNumber; // For Editor, is minigame type
        public string SequenceDescription;
        public ScriptableObject MinigameAsset;
        public MinigameTypeDB.TrialMinigame MinigameType;
    }
}
