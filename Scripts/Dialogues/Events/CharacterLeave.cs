using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public class CharacterLeave : IDialogueEvent
	{
		private bool _ShowHelp = false;

		public void TriggerDialogueEvent()
		{
			DialogueEventSystem.TriggerEvent("CharacterLeave");
		}

		#if UNITY_EDITOR
		public void EditorUI()
		{
			EditorGUILayout.HelpBox("Character will leave this conversation at this point. Please add [WAIT] at the textbox to make this work.", MessageType.Info, true);
		}

		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Oh hey, you clicked the help button for this event for some reason.\n If you're still confused, the character's sprite will leave the scene.", MessageType.Info, true);
		}
		#endif
	}
}
