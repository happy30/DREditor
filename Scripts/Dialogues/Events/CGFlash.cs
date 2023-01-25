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
    /// <summary>
    /// Flashes the CG White
    /// Function found in CGPlayer.cs
    /// </summary>
    [Serializable]
    public class CGFlash : IDialogueEvent
    {
        public bool _ShowHelp = false;
        public void TriggerDialogueEvent()
        {
            DialogueEventSystem.TriggerEvent("CGFlash");
        }

#if UNITY_EDITOR
        public void EditorUI(object value = null)
        {
            EditorGUILayout.LabelField("CG will flash white", GUILayout.Width(135));
        }
        
        public void ShowHelpBox()
        {
            if (_ShowHelp) EditorGUILayout.HelpBox("Flashes the CG White", MessageType.Info, true);
        }

        public void ToggleHelpBox()
        {
            _ShowHelp = !_ShowHelp;
        }
#endif
    }
}
