//Show CG Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public struct SCGTuple
	{
		public GameObject CG;
		public CGChoices CGChoice;
		public bool ScreenFadeOut;
		public float FadeOutTime;
	}

	public enum CGChoices { Show, TriggerNextState, Hide }

	[Serializable]
	public class CGDisplay : IDialogueEvent
	{
		public SCGTuple SCGValue;
		private bool _ShowHelp = false;

		public void TriggerDialogueEvent()
		{
			switch (SCGValue.CGChoice)
			{
				case CGChoices.Show:
					DialogueEventSystem.TriggerEvent("ShowCG", SCGValue.CG);
					break;
				case CGChoices.TriggerNextState:
					DialogueEventSystem.TriggerEvent("CG_TriggerNextState");
					break;
				case CGChoices.Hide:
					DialogueEventSystem.TriggerEvent("CG_FadeOut");
					if (SCGValue.ScreenFadeOut) DialogueEventSystem.TriggerEvent("FadeOut", SCGValue.FadeOutTime);
					break;
				default:
					Debug.LogError("DREditor (CGDisplay): Unable to recognize transition! Please check the transition option in the Dialogue Event if it is empty.");
					break;
			}
		}

#if UNITY_EDITOR
		public void EditorUI()
		{
			SCGValue.CGChoice = (CGChoices)EditorGUILayout.EnumPopup(new GUIContent("Transition", "Choose which transition to do."), SCGValue.CGChoice);
			switch (SCGValue.CGChoice)
			{
				case CGChoices.Show:
					SCGValue.CG = (GameObject)EditorGUILayout.ObjectField(new GUIContent("What CG to display?", "Choose which CG to display."), SCGValue.CG, typeof(GameObject), false);
					break;
				case CGChoices.TriggerNextState:
					EditorGUILayout.LabelField("Note: The next state of the CG will be shown at this line.");
					break;
				case CGChoices.Hide:
					SCGValue.ScreenFadeOut = EditorGUILayout.Toggle(new GUIContent("Also fade out the black screen?", "Choose if want to fade out the ScreenTransitions Dialogue Event also."), SCGValue.ScreenFadeOut);
					if (SCGValue.ScreenFadeOut)
					{
						SCGValue.FadeOutTime = EditorGUILayout.FloatField(new GUIContent("How fast? (in seconds)", "Time in seconds in which the transition will play."), SCGValue.FadeOutTime);
						EditorGUILayout.LabelField("Note: The CG and black screen will fade out at this line.");
					}
					else EditorGUILayout.LabelField("Note: The CG only will fade out at this line.");
					break;
				default:
					Debug.LogError("DREditor (CGDisplay): Unable to recognize transition! Please check the transition option in the Dialogue Event if it is empty.");
					break;
			}
		}

		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
        {
			if (_ShowHelp) EditorGUILayout.HelpBox("Show CG pictures. \n\nNote: CGs should be a GameObject Prefab.", MessageType.Info, true);
		}
#endif
	}
}