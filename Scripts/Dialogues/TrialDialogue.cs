using DREditor.Camera;
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
        public int skip = 0;
    }
    [System.Serializable]
    public class TrialLine : Line
    {
        public int vfxIdx;//*
        public int camAnimIdx;
        public bool edit;//*
        public TCO co = null;//*
    }
    /// <summary>
    /// Trial Camera Override:
    /// Overrides the DRTrialCamera Values 
    /// </summary>
    [System.Serializable]
    public class TCO
    {
        public bool enabled;
        public float seatFocus;
        public float height;
        public float distance;
        public TCO()
        {

        }
        public TCO(float s, float h, float d)
        {
            seatFocus = s;
            height = h;
            distance = d;
        }
        public void Clear()
        {
            enabled = false;
            seatFocus = 0;
            height = 0;
            distance = 0;
        }
    }
}