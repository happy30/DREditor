//Truth Bullet Get Dialogue Event script by Sweden for DREditor.

using System;
using UnityEngine;
using DREditor.TrialEditor;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public struct TBGTuple
	{
		public DTBChoice TBChoice;
		public TruthBullet TB;
	}
	/// <summary>
	/// Found in InvestigationHandler.cs
	/// </summary>
	[Serializable]
	public class TruthBulletGet : IDialogueEvent
	{
		public bool _ShowHelp = false;
		public TBGTuple TBGValue;

		public void TriggerDialogueEvent()
		{
			DialogueEventSystem.TriggerEvent("TruthBulletGet", TBGValue);
		}

#if UNITY_EDITOR
		public void EditorUI(object value = null)
		{
			TBGValue.TB = (TruthBullet)EditorGUILayout.ObjectField(new GUIContent("Show what bullet?", "Choose which Truth bullet to display."), TBGValue.TB, typeof(TruthBullet), false);
		}
		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Adds the truth bullet to the players found truth bullets.", MessageType.Info, true);
		}
#endif
	}
}
