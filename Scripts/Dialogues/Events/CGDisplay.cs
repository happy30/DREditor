//Show CG Dialogue Event script by SeleniumSoul for DREditor. Heavily modified by Sweden#6386 for Eden's Garden

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	using Debug = UnityEngine.Debug;
	[Serializable]
	public struct SCGTuple
	{
		public Sprite CG;
		public bool transitionInstant;
		public AudioClip eventName;
		public bool ScreenFadeOut;
		public bool toDeadly;
		public GameObject prefab;
		public bool playOnly;
		//public float FadeOutTime;
	}
	/// <summary>
	/// For Displaying CG's 
	/// Script has the Editor Field Class
	/// </summary>
	[Serializable]
	public class CGDisplay : IDialogueEvent
	{
		public SCGTuple SCGValue;
		

		public void TriggerDialogueEvent()
		{
            if (SCGValue.CG != null || SCGValue.prefab != null)
				DialogueEventSystem.TriggerEvent("ShowCG", SCGValue);
			else if(!SCGValue.CG && !SCGValue.prefab)
				DialogueEventSystem.TriggerEvent("HideCG", SCGValue);
		}

#if UNITY_EDITOR
		private bool _ShowHelp = false;
		public void EditorUI(object value = null)
		{
			SerializedProperty c = (SerializedProperty)value;
			c = c.FindPropertyRelative("SCGValue").FindPropertyRelative("eventName");

			SCGValue.CG = EditorFields.SpriteField("CG: ", SCGValue.CG, 60, 60);
			SCGValue.transitionInstant = EditorFields.Option(SCGValue.transitionInstant, "Instant Set:", 70);
			SCGValue.ScreenFadeOut = EditorFields.Option(SCGValue.ScreenFadeOut, "Use Full Fade:", 90);
			EditorGUILayout.PropertyField(c, new GUIContent(""), GUILayout.MaxWidth(270));
			SCGValue.toDeadly = EditorFields.Option(SCGValue.toDeadly, "To Deadly:", 70);
			SCGValue.prefab = EditorFields.UnityField(SCGValue.prefab, 120, 35);
			SCGValue.playOnly = EditorFields.Option(SCGValue.playOnly, "Play Only:", 70);
		}

		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
        {
			if (_ShowHelp) EditorGUILayout.HelpBox("Show CG pictures. \n\nNote: You'll need the CG Player.", MessageType.Info, true);
		}
#endif
	}
#if UNITY_EDITOR
	public static class EditorFields
    {
		public static Sprite SpriteField(string name, Sprite sprite, int width = 120, int height = 120)
		{
			GUILayout.Label(name, GUILayout.Width(80));
			Sprite result;
			using (new EditorGUILayout.VerticalScope())
			{
				result = UnityField(sprite, width, height);
			}
			return result;
		}

		public static T UnityField<T>(T data, int width = 120, int height = 120) where T : UnityEngine.Object
		{
			T result;
			using (new EditorGUILayout.VerticalScope())
			{
				result = (T)EditorGUILayout.ObjectField(data, typeof(T), false, GUILayout.Width(width), GUILayout.Height(height));
			}
			return result;
		}
		public static bool Option(bool option, string label, float labelWidth = 60)
		{
			using (new EditorGUILayout.HorizontalScope())
            {
				GUILayout.Label(label, GUILayout.Width(labelWidth));
				return EditorGUILayout.Toggle(option, GUILayout.Width(15));
			}
		}
		public static string StringField(string name, string value, string label = null, int textWidth = 200)
		{
			GUI.backgroundColor = Color.white;
			string result;
			using (new EditorGUILayout.HorizontalScope())
			{
				if (label == null)
					GUILayout.Label(name, GUILayout.Width(80));
				else
					GUILayout.Label(label, GUILayout.Width(80));
				result = EditorGUILayout.TextField(value, GUILayout.Width(textWidth));
			}
			GUILayout.FlexibleSpace();
			return result;
		}
		public static int IntField(string name, int value, int textBoxWidth = 200, int labelBoxMulti = 6)
		{
			GUI.backgroundColor = Color.white;
			int result;
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.Label(name, GUILayout.Width(name.Length * labelBoxMulti));
				result = EditorGUILayout.IntField(value, GUILayout.Width(textBoxWidth));
			}
			GUILayout.FlexibleSpace();
			return result;
		}

		public static float FloatField(string label, float value, int textBoxWidth = 200, int labelBoxMulti = 6)//*Added int field functionality
		{
			GUI.backgroundColor = Color.white;
			float result;
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.Label(label, GUILayout.Width(label.Length * labelBoxMulti));
				result = EditorGUILayout.FloatField(value, GUILayout.Width(textBoxWidth));
			}
			GUILayout.FlexibleSpace();
			return result;
		}
		public static Vector3 Vector3Field(string label, Vector3 value, int textBoxWidth = 200, int labelBoxMulti = 6)//*
		{
			GUI.backgroundColor = Color.white;
			Vector3 result;
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.Label(label, GUILayout.Width(label.Length * labelBoxMulti));
				result = EditorGUILayout.Vector3Field("", value, GUILayout.Width(textBoxWidth));
			}
			GUILayout.FlexibleSpace();
			return result;
		}
	}
#endif
}