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
    public struct CISTuple
    {
        public string objectName;
        public bool isSelectable;
    }
    /// <summary>
    /// Event for moving the dialogue camera to a position
    /// Function found in DialogueAssetReader.cs
    /// </summary>
    [Serializable]
    public class ChangeItemSelect : IDialogueEvent
    {
        public bool _ShowHelp = false;
        public CISTuple cISTuple;
        // This could probably be turned into a vector3 or transform, just uses GameObject.find which isn't 
        // great, but implementation is easier, I'll change this eventually
        public void TriggerDialogueEvent()
        {
            DialogueEventSystem.TriggerEvent("ChangeItemSelect", cISTuple);
        }

#if UNITY_EDITOR
        public void EditorUI(object value = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("GameObject's Name: ", GUILayout.Width(135));
                cISTuple.objectName = EditorGUILayout.TextField(cISTuple.objectName);

            }
            cISTuple.isSelectable = EditorFields.Option(cISTuple.isSelectable, "Is Selectable: ", 150);
        }

        public void ShowHelpBox()
        {
            if (_ShowHelp) EditorGUILayout.HelpBox("Change an Item Actor in the rooms selectability!", MessageType.Info, true);
        }

        public void ToggleHelpBox()
        {
            _ShowHelp = !_ShowHelp;
        }
#endif


    }
}
