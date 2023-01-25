//Change Character Focus Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public struct CWPTuple
	{
		public string PatternName;
		public int _ParameterIndex;
	}

	[Serializable]
	public class ChangeWindowPattern : IDialogueEvent
	{
		public bool _ShowHelp = false;
		public CWPTuple CWPValue;

		public void TriggerDialogueEvent()
		{
			DialogueEventSystem.TriggerEvent("ChangeWindowPattern", CWPValue);
		}

	#if UNITY_EDITOR
		private AnimatorController PanelAnimator;
		private string[] _Parameters;

		public void EditorUI(object value = null)
		{
			if (PanelAnimator == null)
			{
				string _animPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("DialoguePanels t:AnimatorController")[0]);
				PanelAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(_animPath);
			}
			PanelAnimator = (AnimatorController)EditorGUILayout.ObjectField("Animator: ", PanelAnimator, typeof(AnimatorController), true);
			if (PanelAnimator)
			{
				_Parameters = GetAnimatorParameters();
				CWPValue._ParameterIndex = EditorGUILayout.Popup("Position: ", CWPValue._ParameterIndex, _Parameters);
				CWPValue.PatternName = _Parameters[CWPValue._ParameterIndex];
				EditorGUILayout.LabelField($"Showing {CWPValue.PatternName} at this line.");
			}
            else
            {
				CWPValue.PatternName = EditorGUILayout.DelayedTextField("Position: ", CWPValue.PatternName);
				EditorGUILayout.HelpBox("Please link the AnimatorController of your Dialogue Panel on the Animator Field. Or if unavailable, specify the parameter in the field above.", MessageType.Warning, true);
			}
		}

		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Change the positions of the panel windows when talking to somebody.", MessageType.Info, true);
		}

		private string[] GetAnimatorParameters()
		{
			string[] parameterlist = new string[PanelAnimator.parameters.Length - 2];

			for (int i = 0; i < PanelAnimator.parameters.Length - 2; i++)
            {
				parameterlist[i] = PanelAnimator.parameters[i + 2].name;
            }

			return parameterlist;
		}
	#endif
	}
}
