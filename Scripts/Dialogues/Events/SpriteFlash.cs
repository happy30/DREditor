using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public class SpriteFlash : IDialogueEvent
	{
		public bool _ShowHelp = false;

		public void TriggerDialogueEvent()
		{
			DialogueEventSystem.TriggerEvent("SpriteFlash");
		}

		#if UNITY_EDITOR
		public void EditorUI()
		{
			EditorGUILayout.HelpBox("The current character's sprite will flash at this point.", MessageType.Info, true);
		}
		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Make the character's sprite flash.", MessageType.Info, true);
		}
		#endif
	}
}
