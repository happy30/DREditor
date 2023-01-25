using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace DREditor.Dialogues.Events
{
    /// <summary>
    /// To Toggle blur visual
    /// Function found in DialogueAnimConfig.cs
    /// </summary>
    [Serializable]
    public class ToggleBlur : IDialogueEvent
    {
        public bool _ShowHelp = false;
        public bool to;
        // This could probably be turned into a vector3 or transform, just uses GameObject.find which isn't 
        // great, but implementation is easier, I'll change this eventually
        public void TriggerDialogueEvent()
        {
            DialogueEventSystem.TriggerEvent("ToggleBlur", to);
        }

#if UNITY_EDITOR
        public void EditorUI(object value = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                to = EditorFields.Option(to, "Toggle to:", 90);
            }
        }

        public void ShowHelpBox()
        {
            if (_ShowHelp) EditorGUILayout.HelpBox("Toggle Blurring by checking the box", MessageType.Info, true);
        }

        public void ToggleHelpBox()
        {
            _ShowHelp = !_ShowHelp;
        }
#endif


    }
}
