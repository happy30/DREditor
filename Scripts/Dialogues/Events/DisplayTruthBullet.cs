//Show Truth Bullet Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;
using DREditor.Trial;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public struct DTBTuple
    {
		public DTBChoice TBChoice;
		public TruthBullet TB;
	}

	public enum DTBChoice
    {
		Show, Hide
    }

	[Serializable]
	public class DisplayTruthBullet : IDialogueEvent
	{
		public bool _ShowHelp = false;
		public DTBTuple DTBValue;

		public void TriggerDialogueEvent()
		{
			DialogueEventSystem.TriggerEvent("TruthBulletDisplay", DTBValue);
		}

#if UNITY_EDITOR
		public void EditorUI()
		{
			DTBValue.TBChoice = (DTBChoice)EditorGUILayout.EnumPopup(new GUIContent("Transition", "Choose which transition to do."), DTBValue.TBChoice);
			switch (DTBValue.TBChoice)
			{
				case DTBChoice.Show:
					DTBValue.TB = (TruthBullet)EditorGUILayout.ObjectField(new GUIContent("Show what bullet?", "Choose which Truth bullet to display."), DTBValue.TB, typeof(TruthBullet), false);
					break;
				case DTBChoice.Hide:
					break;
				default:
					EditorGUILayout.HelpBox("Unable to determine if you want to show or hide the bullet! Please set it up in the Transition dropdown above.", MessageType.Error, true);
					break;
			}
		}
		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Show the image of the Truth Bullet while in dialogue.", MessageType.Info, true);
		}
#endif
	}
}
