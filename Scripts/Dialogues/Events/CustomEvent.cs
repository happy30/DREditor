//CustomEvent Dialogue Event script by SeleniumSoul for DREditor.
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public struct CETuple
	{
		public string EventName;
	}

	[Serializable]
	public class CustomEvent : IDialogueEvent
	{
		public bool _ShowHelp = false;
		public CETuple CEValue;


		public void TriggerDialogueEvent()
		{
			DialogueEventSystem.TriggerEvent(CEValue.EventName);
		}

#if UNITY_EDITOR
		public void EditorUI(object value = null)
		{
			CEValue.EventName = EditorGUILayout.DelayedTextField("Invoke what Event?", CEValue.EventName);
		}

		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Make the Dialogue Event System invoke a custom event using a string.\nDoes not accept custom parameters.\n\nCaution: This is intended to be used as a temporary debugging solution. It is highly recommended to make a new Dialogue Event Script to avoid having the event being forgotten.", MessageType.Info, true);
		}
#endif
	}
}
