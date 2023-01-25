//Change Character Focus Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public struct CCFTuple
	{
		public int PanelNum;
		public Vector3 CamTrans;
	}

	[Serializable]
	public class ChangeCharacterFocus : IDialogueEvent
	{
		public bool _ShowHelp = false;
		public CCFTuple CCFValue;

		public void TriggerDialogueEvent()
		{
			(int, Vector3) values;
			values = (CCFValue.PanelNum, CCFValue.CamTrans);

			DialogueEventSystem.TriggerEvent("ChangeFocus", values);
		}

#if UNITY_EDITOR
		public void EditorUI(object value = null)
		{
			CCFValue.PanelNum = EditorGUILayout.IntPopup("Panel Camera:", CCFValue.PanelNum, new string[] { "Middle Panel", "Right Panel", "Left Panel" }, new int[] { 0, 1, 2 });
			CCFValue.CamTrans = EditorGUILayout.Vector3Field("Camera Position (Optional):", CCFValue.CamTrans);
		}

		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Choose which panel would change its focused character.\nNote: Changing the Camera Position values to non-zero would force its camera transform.", MessageType.Info, true);
		}
#endif
	}
}