using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Characters
{
    [System.Serializable]
    public class LocalizedCharacterDatabase : ScriptableObject 
    {
        public List<LocalizedCharacter> Characters;

        public Dictionary<string, LocalizedCharacter> GetLocalizationMap()
        {
            Dictionary<string, LocalizedCharacter> map = new Dictionary<string, LocalizedCharacter>();
            if(Characters != null && Characters.Count > 0)
            {
                foreach(LocalizedCharacter c in Characters)
                {
                    if(c != null)
                    {
                        map.Add(c.translationKey, c);
                    }
                }
            }
            return map;
        }
    }
}
