using DREditor.Gates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Progression
{
    /// <summary>
    /// Requires: Gateways
    /// Chapters hold objective's and their flags
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Chapter", menuName = "DREditor/Progression/Chapter")]
    public class Chapter : ScriptableObject
    {
        public string saveTitle;
        public string title;
        public List<Objective> Objectives = new List<Objective>();

        public string[] GetObjectives()
        {
            List<string> vs = new List<string>();
            foreach (Objective o in Objectives)
            {
                vs.Add(o.Description);
            }
            string[] x = vs.ToArray();
            return x;
        }
    }
    [System.Serializable]
    public class Objective
    {
        public string Description = "";
        public Gate ProgressionGate = null;
        //public Dialogue LockDialogue = null;
        public List<Gate> Gateways = new List<Gate>();
        public List<ProgressionFlag> Flags = new List<ProgressionFlag>();

        public int GetRequiredFlagCount() => GetRequiredFlagList().Count;
        public List<ProgressionFlag> GetRequiredFlagList()
        {
            List<ProgressionFlag> tmpFlags = new List<ProgressionFlag>();
            foreach (ProgressionFlag flag in Flags)
            {
                if (!flag.optional)
                {
                    tmpFlags.Add(flag);
                }
            }
            return tmpFlags;
        }
        public string[] GetFlagNames()
        {
            string[] tmp = new string[Flags.Count];

            for(int i = 0; i < Flags.Count; i++)
            {
                tmp[i] = Flags[i].name;
            }

            return tmp;
        }
        public object Clone()
        {
            Objective o = new Objective();
            o.Description = Description;
            o.ProgressionGate = ProgressionGate;
            for (int i = 0; i < Flags.Count; i++)
            {
                o.Flags.Add((ProgressionFlag)Flags[i].Clone());
            }
            return o;
        }
    }
    [System.Serializable]
    public class ProgressionFlag
    {
        public string name;
        public bool triggered;
        public bool optional = false;
        public object Clone()
        {
            ProgressionFlag newFlag = new ProgressionFlag();
            newFlag.name = name;
            newFlag.triggered = triggered;
            newFlag.optional = optional;
            return newFlag;
        }
    }
}