using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DREditor;
using JetBrains.Annotations;
using DREditor.Characters;
using DREditor.VFX;
using DREditor.EventObjects;
using UnityEngine.Video;
using DREditor.Dialogues.Events;

namespace DREditor.Dialogues
{
    /// <summary>
	/// Identifies if the dialogue will be used normally, CG, or in Trial. Used primarily for the game to know what type of dialogue screen to use.
	/// </summary>
	public enum BoxMode
    {
        Normal
    }

    public abstract class DialogueBase : ScriptableObject
    {
        public BoxMode DialogueMode;
        public string translationKey;
        public string DialogueName = "";
        public Color Color = Color.white;
        public bool ShowDialoguePanel = true;
        public Variable Variable = new Variable();
        public DirectTo DirectTo;
        public SceneTransition SceneTransition = new SceneTransition();
        public bool IsInstant = false;
        public TriggerFlag FlagTrigger = new TriggerFlag(); //*
        public bool EndVidEnabled = false; //*
        public VideoClip EndVideo; //*
        public bool ClearLock = false; //*
        

#if UNITY_EDITOR
        [HideInInspector]
        public CharacterDatabase Speakers;
        [HideInInspector]
        public VFXDatabase VFXDB; //*
        public string[] GetCharacterNames()
        {
            if (Speakers == null)
            {
                Speakers = Resources.Load<CharacterDatabase>("Characters/CharacterDatabase");

            }
            return Speakers?.GetNames().ToArray();
        }
        public string[] GetVFXNames() //*
        {
            if (VFXDB == null)
            {
                VFXDB = Resources.Load<VFXDatabase>("VFX/VFXDatabase");

            }
            return VFXDB?.GetNames().ToArray();
        }

        public Alias[] GetCharacterAliases(Character cha)
        {
            return cha.Aliases.ToArray();
        }

        public int[] getNamesIntValues()
        {
            int[] values = new int[Speakers.GetNames().ToArray().Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = i;
            }
            return values;
        }

        public int[] getExpressionIntValues(Character cha)
        {

            int[] values = new int[cha.Expressions.Count + 1];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = i;
            }
            return values;
        }

        public int[] getAliasesIntValues(Character cha)
        {
            int[] values = new int[cha.Aliases.Count + 1];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = i;
            }
            return values;
        }
#endif
    }

    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Dialogues/Dialogue", fileName = "New Dialogue")]
    public class Dialogue : DialogueBase
    {
        public bool UnlockPause = false;
        public bool UnlockPauseOption = false;
        public int PauseOptionNum = 0;
        public bool PauseOptionTo = true;
        public List<Line> Lines = new List<Line>();
        public List<Choice> Choices = new List<Choice>();
        public StartInvestigation StartInvestigation = new StartInvestigation();
        //public bool ShowEndEvents = false;
        public Dialogue Copy()
        {
            Dialogue dia = CreateInstance<Dialogue>();
            dia.Lines = Lines;

            
            return dia;
        }
    }

    [System.Serializable]
    public class Line
    {
        public string translationKey;
        public Character Speaker;
        public int SpeakerNumber;
        public string Text;
        public AudioClip VoiceSFX; //* Was AudioClip
        public AudioClip EnvSFX; //* Was AudioClip
        public List<AudioClip> SFX = new List<AudioClip>();
        public List<SceneEvent> Events = new List<SceneEvent>();
        public float TimeToNextLine;
        public bool AutomaticLine;
        public Expression Expression;
        public int ExpressionNumber;
        public int AliasNumber;
        // additions
        [SerializeReference] public List<IDialogueEvent> DiaEvents = new List<IDialogueEvent>();
        public bool MusicChange;
        public AudioClip Music;
        public AnimationClip VFX;
        public int VFXNumber;

        public bool ShowOptions; // For toggling list of Visual effects below
        public bool DontPan; // Don't change focus
        public bool Leave; // Previous character that spoke left
        public bool FlashWhite; // When the Screen flashes white 
        //public bool Faint; // Signifys to play Faint DR Anim
        public bool BlurVision; // a sudden blur of protags vision Anim
        public bool StopEnv;
        public bool StopSFX;

        public bool PanToChar;
        public int CharToPanNum;
        public Character CharToPan;

#if UNITY_EDITOR
        public bool ShowDiaEvents;
#endif
    }

    [System.Serializable]
    public class Choice
    {
        public string ChoiceText;
        public Dialogue NextDialogue;
        public int NextIndexInDialogue;
    }

    [System.Serializable]
    public class TriggerFlag
    {
        public bool Enabled;
        public int Chapter = 0; //*
        public int Objective; //*
        public List<int> Flags = new List<int>(); //*
        //public List<bool> IntendedValues = new List<bool>(); //*
    }

    [System.Serializable]
    public class Variable
    {
        public bool Enabled;
        public BoolWithEvent BoolVariable;
        public int Chapter = 0; //*
        public int Objective; //*
        public List<int> Flags = new List<int>(); //*
        //public List<bool> IntendedValues = new List<bool>(); //*
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
        public TrialDialogue NewTrialDialogue;
    }

    [System.Serializable]
    public class SceneTransition
    {
        public bool Enabled;
        public string Scene;
        public bool ToDark;
        public bool ToMenu;
        public Gates.Gate AtEnd;
        public bool OnLoadNoDark;
        public bool OnlySceneEnabled()
        {
            return Enabled && Scene != "" && !ToDark && !ToMenu && !OnLoadNoDark && AtEnd == null;
        }
        //transition
    }
    [System.Serializable]
    public class StartInvestigation
    {
        public bool Enabled;
        public bool NextObjective = true;
        public Dialogue NewDialogue;
    }
}
