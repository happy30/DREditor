//Dialogue Handler script by SeleniumSoul for DR:Distrust

using System;

/// <summary>
/// Houses all the global parameters in a dialogue that can be used by multiple classes.
/// </summary>
namespace DREditor.Dialogues
{
	static public class DialogueHandler
	{
		static public bool InDialog;
		static public bool InMenu;
		static public bool Advance;
		static public bool Skippable;
		static public bool IsWriting;
		static public bool OnTransition;

		static public bool _skip;
		static public bool _auto;

		static public Backlog BLFile;

		[NonSerialized] static public Dialogue DialogueAsset;
		[NonSerialized] static public int _dialogueLength;
		[NonSerialized] static public int _currentLineNum;
		[NonSerialized] static public Line _currentLine;

		static public void EndDialogue()
        {
			InDialog = false;
			Advance = false;
			Skippable = false;
			IsWriting = false;
			OnTransition = false;
        }
	}
}