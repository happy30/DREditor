using System.Collections;
using System.Collections.Generic;
using EventObjects;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "DRSimulator/Dialogues/Dialogue", fileName = "New Dialogue")]
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
    
    public void GetCharacters()
    {

        if (!Resources.Load("SpeakerDatabase"))
        {
            Debug.LogError("Couldn't find speaker database. Please create a speaker database asset and put it in the Resources folder as SpeakerDatabase.asset");
        }
      
    }

    // TO DO
    
    /*public string[] GetCharacterNames()
    {
        GetCharacters();

        return Speakers.Students.;
    }

    public int[] getIntValues()
    {

        GetCharacters();

        int[] values = new int[Speakers.Speakers.Length];
        for(int i = 0; i < values.Length; i++)
        {
            values[i] = i;
        }
        return values;
    }
    */
}

[System.Serializable]
public class Line
{
    public string Speaker;
    public int SpeakerNumber;
    public string Text;
    public AudioClip VoiceLine;
    public SceneEvent Event;
    public float TimeToNextLine;
    public bool AutomaticLine;
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
