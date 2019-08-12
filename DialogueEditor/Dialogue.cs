using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using EventObjects;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "DRSimulator/Dialogue", fileName = "New Dialogue")]
public class Dialogue : ScriptableObject
{
    public string DialogueName = "";
    public List<Line> Lines = new List<Line>();
    public List<Choice> Choices = new List<Choice>();
    public Color Color = Color.white;
    public Variable Variable = new Variable();
    public DirectTo DirectTo;

    [HideInInspector]
    public CharacterDatabase Speakers;
    

    // TO DO
    
    public string[] GetCharacterNames()
    {
        if (Speakers == null)
        {
            Speakers = Resources.Load<CharacterDatabase>("Characters/CharacterDatabase");
            
        }
        
        
        return Speakers?.GetNames().ToArray();
    }

    public int[] getNamesIntValues()
    {

        int[] values = new int[Speakers.GetNames().ToArray().Length];
        for(int i = 0; i < values.Length; i++)
        {
            values[i] = i;
        }
        return values;
    }
    
    public int[] getExpressionIntValues(Student stu)
    {

        int[] values = new int[stu.Expressions.Count + 1];
        for(int i = 0; i < values.Length; i++)
        {
            values[i] = i;
        }
        return values;
    }
    
}

[System.Serializable]
public class Line
{
    public Student Speaker;
    public int SpeakerNumber;
    public string Text;
    public List<AudioClip> SFX = new List<AudioClip>();
    public List<SceneEvent> Events = new List<SceneEvent>();
    public float TimeToNextLine;
    public bool AutomaticLine;
    public Expression Expression;
    public int ExpressionNumber;
}

[System.Serializable]
public class Choice
{
    public string ChoiceText;
    public Dialogue NextDialogue;
    public int NextIndexInDialogue;
}

[System.Serializable]
public class Variable
{
    public bool Enabled;
    public BoolWithEvent BoolVariable;
    public Dialogue NextDialogueTrue;
    public int NextIndexInDialogueTrue;
    public Dialogue NextDialogueFalse;
    public int NextIndexInDialogueFalse;
}

[System.Serializable]
public class DirectTo
{
    public bool Enabled;
    public Dialogue NewDialogue;
    public int NewDialogueIndex;
}
