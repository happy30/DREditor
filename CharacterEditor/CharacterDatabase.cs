using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "DRSimulator/CharacterDatabase", fileName = "New CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    public Student[] Students;


    public List<string> GetNames()
    {
        var names = new List<string>();
        
        foreach (var stu in Students)
        {
            names.Add(stu.Character.LastName + " " + stu.Character.FirstName);
        }
        return names;
    }
}
