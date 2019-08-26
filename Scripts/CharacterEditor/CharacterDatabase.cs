using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "DREditor/Characters/Character Database", fileName = "CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    public List<Character> Characters = new List<Character>();


    public List<string> GetNames()
    {
        var names = new List<string>();
        
        foreach (var cha in Characters)
        {
            names.Add(cha.LastName + " " + cha.FirstName);
        }
        return names;
    }
}
