using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace DREditor.Dialogues
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Dialogues/TrialDialogue", fileName = "New Trial Dialogue")]
    public class TrialDialogue : DialogueBase
    {
        public List<TrialLine> Lines = new List<TrialLine>();
    }
    [System.Serializable]
    public class TrialLine : Line
    {
        public int vfxIdx;
        public int camAnimIdx;
    }
}