using DREditor.Camera;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace DREditor.Dialogues
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Dialogues/TCOPresetDatabase", fileName = "TCOPresets")]
    public class TCODatabase : ScriptableObject
    {
        public List<TCOPreset> Presets = new List<TCOPreset>();
        public string[] GetNames()
        {
            List<string> names = new List<string>();
            foreach(TCOPreset p in Presets)
            {
                names.Add(p.presetName);
            }
            return names.ToArray();
        }
        public int[] GetInts()
        {
            int[] nums = new int[Presets.Count];
            for(int i = 0; i < Presets.Count; i++)
            {
                nums[i] = i;
            }
            return nums;
        }
    }
    [System.Serializable]
    public struct TCOPreset
    {
        public string presetName;
        public TCO Preset;
        public TCOPreset(string name, TCO preset)
        {
            presetName = name;
            Preset = preset;
        }
    }
}
