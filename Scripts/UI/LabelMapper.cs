using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using DREditor.Localization;

namespace DREditor.UI
{
    [System.Serializable]
    public class LabelAndText
    {
        public TMP_Text label;
        public TranslatableText text;
    }

    public class LabelMapper : MonoBehaviour
    {

        public List<LabelAndText> labelMapList = new List<LabelAndText>();

        private Dictionary<TMP_Text, TranslatableText> labelTextMap;

        public TranslatableText GetTranslatableForLabel(TMP_Text label)
        {
            TranslatableText result = null;
            if(labelTextMap != null && labelTextMap.ContainsKey(label))
            {
                result = labelTextMap[label];
            }
            return result;
        }

        void Start()
        {
            InitMap();
        }

        private void InitMap()
        {
            if(labelMapList != null && labelMapList.Count > 0)
            {
                labelTextMap = new Dictionary<TMP_Text, TranslatableText>();
                foreach(var item in labelMapList)
                {
                    if(item != null && item.label != null && item.text != null)
                    {
                        labelTextMap[item.label] = item.text;
                    }
                }
            }
        }
    }
}

