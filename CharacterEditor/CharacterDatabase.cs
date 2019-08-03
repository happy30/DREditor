using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "DRSimulator/CharacterDatabase", fileName = "New CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    public Student[] Students;
}
