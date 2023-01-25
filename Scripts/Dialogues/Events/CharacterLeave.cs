using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public struct CLTuple
	{
		public bool appear;
		public bool current;
		public bool exit;
	}
	[Serializable]
	public class CharacterLeave : IDialogueEvent
	{
		
		public CLTuple data;
		public void TriggerDialogueEvent()
		{
			DialogueEventSystem.TriggerEvent("CharacterLeave", data);
		}

		#if UNITY_EDITOR
		private bool _ShowHelp = false;
		public void EditorUI(object value = null)
		{
			data.appear = EditorFields.Option(data.appear, "Appear: ");
			data.current = EditorFields.Option(data.current, "Current: ");
			data.exit = EditorFields.Option(data.exit, "Exit: ");
			//EditorGUILayout.HelpBox("Character sprite will fade out at this point.", MessageType.Info, true);
		}

		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Appear is for if the character is appearing. " +
                "Current is to fade the current person speaking on the line (this should be false " +
                "if the protag is the one speaking on the line)." +
                "\n Exit Sends the actor to coordinates 0,-10 to hide the character.", MessageType.Info, true);
		}
		#endif
	}
}
