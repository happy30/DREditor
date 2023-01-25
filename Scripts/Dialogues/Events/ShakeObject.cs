using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DREditor.Dialogues.Events;
using DREditor.Characters;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
    [Serializable]
    public struct SOTuple
    {
        public SOEnum type;
        public float duration;
        public float strength;
        public int vibrato;
        public float randomness;
        public bool snapping;
        public bool fadeOut;
        public bool protag;
    }
    public enum SOEnum
    {
        Camera, CG
    }
    /// <summary>
    /// Event to shake a Camera or CG in 
    /// CGPlayer and DiaCamEvents
    /// </summary>
    [Serializable]
    public class ShakeObject : IDialogueEvent
    {
        
        public SOTuple data;
        public ShakeObject()
        {
            
            data.duration = 0.5f;
            data.strength = 4;
            data.vibrato = 10;
            data.randomness = 5;
            data.snapping = false;
            data.fadeOut = true;
        }
        public void TriggerDialogueEvent()
        {
            switch (data.type)
            {
                case SOEnum.Camera:
                    DialogueEventSystem.TriggerEvent("ShakeCamera", data);
                    break;
                case SOEnum.CG:
                    DialogueEventSystem.TriggerEvent("ShakeCG", data);
                    break;
            }
        }

#if UNITY_EDITOR
        public bool _ShowHelp = false;
        public void EditorUI(object value = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                data.type = (SOEnum)EditorGUILayout.EnumPopup(data.type);
                if (GUILayout.Button("Test", GUILayout.Width(70)))
                {
                    if (!Application.isPlaying)
                    {
                        EditorUtility.DisplayDialog("Must be in Play Mode", "You must be in play mode to test!", "Ok");
                        return;
                    }
                    switch (data.type)
                    {
                        case SOEnum.Camera:
                            DialogueEventSystem.TriggerEvent("ShakeCamera", data);
                            break;
                        case SOEnum.CG:
                            DialogueEventSystem.TriggerEvent("ShakeCG", data);
                            break;
                    }
                }
            }
            
            data.duration = FloatField("Duration: ", data.duration);
            data.strength = FloatField("Strength: ", data.strength);
            data.vibrato = IntField("Vibrato: ", data.vibrato);
            data.randomness = FloatField("Randomness: ", data.randomness);
            data.snapping = Option("Snapping: ", data.snapping);
            data.fadeOut = Option("Fade Out: ", data.fadeOut);
            data.protag = Option("Protag: ", data.protag);
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
        public static bool Option(string label, bool option, float labelWidth = 60)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(labelWidth));
                return EditorGUILayout.Toggle(option, GUILayout.Width(15));
            }
            
        }
        public void ShowHelpBox()
        {
            if (_ShowHelp) EditorGUILayout.HelpBox("Shake either the Camera or CG/Video using DOTween aspects.", MessageType.Info, true);
        }

        public void ToggleHelpBox()
        {
            _ShowHelp = !_ShowHelp;
        }
#endif
    }
}
