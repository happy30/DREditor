using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Dialogues
{
    [System.Serializable]
    public class LocalizedDialogue : ScriptableObject
    {
        public string translationKey;
        public List<LocalizedDialogueLine> Lines;

        public Dictionary<string, LocalizedDialogueLine> GetLocalizationMap()
        {
            Dictionary<string, LocalizedDialogueLine> map = new Dictionary<string, LocalizedDialogueLine>();
            if(Lines != null && Lines.Count > 0)
            {
                foreach(LocalizedDialogueLine line in Lines)
                {
                    map.Add(line.translationKey, line);
                }
            }
            return map;
        }

    }

    [System.Serializable]
    public class LocalizedDialogueLine
    {
        public string translationKey;
        public Line original;

        [SerializeField]
        private string _Text;
        public string Text
        {
            set { _Text = value; }
            get
            {
                string retVal = _Text;
                if(string.IsNullOrEmpty(retVal) && original != null)
                {
                    retVal = original.Text;
                }
                return retVal;
            }
        }
    }
}
