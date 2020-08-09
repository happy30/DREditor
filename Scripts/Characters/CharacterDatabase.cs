using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Characters
{
    
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Characters/Character Database", fileName = "CharacterDatabase")]
    public class CharacterDatabase : ScriptableObject
    {
        [SerializeField]
        public List<Character> Characters = new List<Character>();


        public List<string> GetNames()
        {
            var names = new List<string>();

            foreach (var cha in Characters)
            {
                switch (cha)
                {
                    case Protagonist _:
                        names.Add(cha.LastName + " " + cha.FirstName + " (Protagonist)");
                        break;
                    case Headmaster _:
                        names.Add(cha.LastName + " " + cha.FirstName + " (Headmaster)");
                        break;
                    case Student _:
                        names.Add(cha.LastName + " " + cha.FirstName);
                        break;
                }
            }
            return names;
        }
        
    }
}