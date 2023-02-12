//Change Character Focus Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public struct FTBTuple
	{
		public FadeChoices FadeChoice;
		public float FadeSpeed;
	}
	public enum FadeChoices { FadeIn, FadeOut }

	[Serializable]
	public class ScreenTransitions : IDialogueEvent
	{
		public bool _ShowHelp = false;
		public FTBTuple FTBValue;

		public void TriggerDialogueEvent()
		{
			switch (FTBValue.FadeChoice)
			{
				case FadeChoices.FadeIn:
					DialogueEventSystem.TriggerEvent("FadeOut", FTBValue.FadeSpeed);
					break;
				case FadeChoices.FadeOut:
					DialogueEventSystem.TriggerEvent("FadeToBlack", FTBValue.FadeSpeed);
					break;
				default:
					Debug.LogError("DREditor (ScreenTransitions): Unable to recognize transition! Please check the transition option in the Dialogue Event if it is empty.");
					break;
			}
		}

#if UNITY_EDITOR
		public void EditorUI()
		{
			FTBValue.FadeChoice = (FadeChoices)EditorGUILayout.EnumPopup(new GUIContent("Transition", "Choose which transition to do."), FTBValue.FadeChoice);
			FTBValue.FadeSpeed = EditorGUILayout.DelayedFloatField(new GUIContent("At what speed? (in seconds)", "Time in seconds in which the transition will play."), FTBValue.FadeSpeed);
		}
		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Make the whole screen fade to black.", MessageType.Info, true);
		}
#endif
	}
}
