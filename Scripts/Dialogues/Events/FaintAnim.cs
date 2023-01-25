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
    public struct FATuple
    {
        public bool faint;
        public float to;
    }
    /// <summary>
    /// Found in DialogueAnimConfig
    /// </summary>
    [Serializable]
    public class FaintAnim : IDialogueEvent
    {
        public bool _ShowHelp = false;
        public FATuple data;
        public void TriggerDialogueEvent()
        {
            DialogueEventSystem.TriggerEvent("FaintAnim", data);
        }

#if UNITY_EDITOR
        public void EditorUI(object value = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                data.faint = EditorFields.Option(data.faint, "Faint: ");
            }
            data.to = EditorFields.FloatField("To Y: ", data.to);
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

        public void ShowHelpBox()
        {
            if (_ShowHelp) EditorGUILayout.HelpBox("If faint is true then character will faint, " +
                "if faint is false then the character will get up.", MessageType.Info, true);
        }

        public void ToggleHelpBox()
        {
            _ShowHelp = !_ShowHelp;
        }
#endif
    }
}
