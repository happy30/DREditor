using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace DREditor.Dialogues.Events
{
    [Serializable]
    public struct CITuple
    {
        public string charName;
    }
    /// <summary>
    /// Event for moving the dialogue camera to a position
    /// Function found in DialogueAssetReader.cs
    /// </summary>
    [Serializable]
    public class CharIntro : IDialogueEvent
    {
        public bool _ShowHelp = false;
        public CITuple cITuple;
        // This could probably be turned into a vector3 or transform, just uses GameObject.find which isn't 
        // great, but implementation is easier, I'll change this eventually
        public void TriggerDialogueEvent()
        {
            DialogueEventSystem.TriggerEvent("CharIntro", cITuple);
        }

#if UNITY_EDITOR
        public void EditorUI(object value = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Char's First Name: ", GUILayout.Width(135));
                cITuple.charName = EditorGUILayout.TextField(cITuple.charName);

            }
            
        }

        public void ShowHelpBox()
        {
            if (_ShowHelp) EditorGUILayout.HelpBox("Calls for the Character Intro Animation", MessageType.Info, true);
        }

        public void ToggleHelpBox()
        {
            _ShowHelp = !_ShowHelp;
        }
#endif


    }
}
