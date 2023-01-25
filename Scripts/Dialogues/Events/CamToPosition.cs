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
    public struct CTPTuple
    {
        public string objectName;
        public bool keepFocus;
    }
    /// <summary>
    /// Event for moving the dialogue camera to a position
    /// Function found in DiaCamEvent.cs
    /// </summary>
    [Serializable]
    public class CamToPosition : IDialogueEvent
    {
        public bool _ShowHelp = false;
        public CTPTuple cTPTuple;
        public string objectName;
        // This could probably be turned into a vector3 or transform, just uses GameObject.find which isn't 
        // great, but implementation is easier, I'll change this eventually
        public void TriggerDialogueEvent()
        {
            cTPTuple.objectName = objectName; // I fucked up so I'm doing this so I don't lose progress
            // lesson learned, always make a tuple when making these, so that fixing it is less ugly and can
            // be turned into a deprecated or obsolete code.
            DialogueEventSystem.TriggerEvent("CamToPosition", cTPTuple);
        }

#if UNITY_EDITOR
        public void EditorUI(object value = null)
        {
            using(new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("GameObject's Name: ", GUILayout.Width(135));
                objectName = EditorGUILayout.TextField(objectName);
                
            }
            cTPTuple.keepFocus = EditorFields.Option(cTPTuple.keepFocus, "Keep Focused character: ", 150);
        }

        public void ShowHelpBox()
        {
            if (_ShowHelp) EditorGUILayout.HelpBox("Write the name of a game object in the scene to " +
                "use it's transform for the position to move the camera.", MessageType.Info, true);
        }

        public void ToggleHelpBox()
        {
            _ShowHelp = !_ShowHelp;
        }
#endif
        

    }
}
