//Show CG Dialogue Event script by SeleniumSoul for DREditor. Heavily modified by Sweden#6386 for Eden's Garden

using System;
using UnityEngine;
using System.Collections.Generic;
using DREditor.Dialogues.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	using Debug = UnityEngine.Debug;
	[Serializable]
	public struct FBTuple
	{
		public List<Sprite> Flashbacks;
	}
	/// <summary>
	/// For Displaying Flashbacks
	/// </summary>
	[Serializable]
	public class Flashback : IDialogueEvent
	{
		public FBTuple FBValue;


		public void TriggerDialogueEvent()
		{
			DialogueEventSystem.TriggerEvent("Flashback", FBValue);
		}

#if UNITY_EDITOR
		private bool _ShowHelp = false;
		public void EditorUI(object value = null)
		{
			if(FBValue.Flashbacks == null)
            {
				FBValue.Flashbacks = new List<Sprite>();
            }
			for(int i = 0; i < FBValue.Flashbacks.Count; i++)
            {
				using (new EditorGUILayout.HorizontalScope())
                {
					FBValue.Flashbacks[i] = EditorFields.SpriteField("FB: ", FBValue.Flashbacks[i], 120, 30);
					if (GUILayout.Button("x", GUILayout.Width(20)))
					{
						FBValue.Flashbacks.RemoveAt(i);
					}
				}
				
			}

			GUILayout.Space(25);

			if (GUILayout.Button("Add Flashback", GUILayout.Width(120)))
            {
				FBValue.Flashbacks.Add(null);
            }
		}

		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("List of Flash Back CG's to input", MessageType.Info, true);
		}
#endif
	}
}
