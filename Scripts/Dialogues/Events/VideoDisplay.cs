//Show CG Dialogue Event script by SeleniumSoul for DREditor. Heavily modified by Sweden#6386 for Eden's Garden

using System;
using UnityEngine;
using UnityEngine.Video;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	using Debug = UnityEngine.Debug;
	[Serializable]
	public struct SVTuple
    {
		public VideoClip iniClip;
		public VideoClip mainClip;
		public bool isLooping;
		public float speed;
		public bool ScreenFadeOut;
		public bool waitToEnd;
		public bool playOnly;
		public bool stopSound;
		public bool toDeadly;
		public AudioClip eventName;
		//[EventRef] public string startSound;
		
	}
	/// <summary>
	/// For Displaying CG's 
	/// The Event is used in the CGPlayer.cs
	/// Script has the Editor Field Class
	/// </summary>
	[Serializable]
	public class VideoDisplay : IDialogueEvent
	{
		public SVTuple SVValue;
		
		public VideoDisplay()
        {
			SVValue.speed = 1;
        }
		public void TriggerDialogueEvent()
		{
			if (SVValue.mainClip != null)
				DialogueEventSystem.TriggerEvent("ShowVideo", SVValue);
			else
				DialogueEventSystem.TriggerEvent("HideVideo", SVValue);
		}

#if UNITY_EDITOR
		private bool _ShowHelp = false;
		public void EditorUI(object value = null)
		{
			SerializedProperty c = (SerializedProperty)value;
			//SerializedProperty s = (SerializedProperty)value;
			c = c.FindPropertyRelative("SVValue").FindPropertyRelative("eventName");
			//s = s.FindPropertyRelative("SVValue").FindPropertyRelative("startSound");
			
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Main Clip: ", GUILayout.Width(70));
				SVValue.mainClip = EditorFields.UnityField(SVValue.mainClip, 160, 20);
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Initial Clip: ", GUILayout.Width(70));
				SVValue.iniClip = EditorFields.UnityField(SVValue.iniClip, 160, 20);
			}
			SVValue.isLooping = EditorFields.Option(SVValue.isLooping, "Is Looping:", 70);
			if (SVValue.isLooping && SVValue.waitToEnd)
            {
				SVValue.waitToEnd = false;
            }
			SVValue.speed = EditorFields.FloatField("Playback Speed: ", SVValue.speed, 30, 7);
			SVValue.ScreenFadeOut = EditorFields.Option(SVValue.ScreenFadeOut, "Use Full Fade:", 90);
			SVValue.waitToEnd = EditorFields.Option(SVValue.waitToEnd, "Wait to End:", 90);
			SVValue.playOnly = EditorFields.Option(SVValue.playOnly, "Play Only:", 90);
			SVValue.stopSound = EditorFields.Option(SVValue.stopSound, "Stop Sound:", 90);
			if (SVValue.isLooping && SVValue.waitToEnd)
			{
				SVValue.isLooping = false;
			}
			EditorGUILayout.PropertyField(c, new GUIContent(""), GUILayout.MaxWidth(270));
			//EditorGUILayout.PropertyField(s, new GUIContent(""), GUILayout.MaxWidth(270));
			SVValue.toDeadly = EditorFields.Option(SVValue.toDeadly, "To Deadly:", 70);
		}

		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Shows Videos. for animated CG's \n\nNote: You'll need the CG Player.", MessageType.Info, true);
		}
#endif
	}
}
