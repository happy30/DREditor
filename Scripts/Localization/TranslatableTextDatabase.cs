using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Localization
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Localization/Translatables Database", fileName = "TranslatablesDatabase")]
    public class TranslatableTextDatabase : ScriptableObject
    {
        public TranslatableTextDatabase original;
        public bool isTranslation;
        public List<TranslatableText> translatables;

        public Dictionary<string, TranslatableText> GetTranslatablesMap()
        {
            Dictionary<string, TranslatableText> map = new Dictionary<string, TranslatableText>();
            if(translatables != null && translatables.Count > 0)
            {
                foreach(TranslatableText text in translatables)
                {
                    map.Add(text.translationKey, text);
                }
            }
            return map;
        }

        public string[] GetTexts()
        {
            string[] texts = null;
            if(translatables != null && translatables.Count > 0)
            {
                texts = new string[translatables.Count];
                for(int i=0;i<translatables.Count;i++)
                {
                    texts[i] = translatables[i].Text;
                }
            }
            return texts;
        }

        public int GetIndexByTranslationKey(string translationKey)
        {
            int index = -1;
            if (!string.IsNullOrEmpty(translationKey) && translatables != null && translatables.Count > 0)
            {
                for (int i = 0; i < translatables.Count; i++)
                {
                    if(translationKey.Equals(translatables[i].translationKey))
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }
    }

    [System.Serializable]
    public class TranslatableText
    {
        public TranslatableText original;
        public string translationKey;
        public string Text;
    }
}
