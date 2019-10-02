using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Localization
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Localization/Locale Settings", fileName = "LocaleSettings")]
    public class LocaleSettings : ScriptableObject
    {
        public Locale defaultLocale;
        public bool isDefaultEmbeded = true;
        public string baseFolder = "Locale";
        public List<Locale> alternativeLocales;

        public string[] GetLocaleNames()
        {
            List<string> names = null;
            if(alternativeLocales != null && alternativeLocales.Count > 0)
            {
                names = new List<string>(alternativeLocales.Count);
                for(int i=0;i<alternativeLocales.Count;i++)
                {
                    names.Add(alternativeLocales[i] ? alternativeLocales[i].langName : "No name");
                }
            }
            return names.ToArray();
        }
    }
}
