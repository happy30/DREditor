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
    public class OTuple
    {
        public string objectName;
        public bool localPos;
        public Vector3 position;
    }
    /// <summary>
    /// Event for moving an object to a position
    /// Function found in DialogueEventHandler.cs
    /// </summary>
    [Serializable]
    public class ObjectToPosition : IDialogueEvent
    {
        public bool _ShowHelp = false;
        public OTuple oTuple;
        // This could probably be turned into a vector3 or transform, just uses GameObject.find which isn't 
        // great, but implementation is easier, I'll change this eventually
        public void TriggerDialogueEvent()
        {
            DialogueEventSystem.TriggerEvent("ObjectToPosition", oTuple);
        }

#if UNITY_EDITOR
        public void EditorUI(object value = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("GameObject's Name: ", GUILayout.Width(135));
                oTuple.objectName = EditorGUILayout.TextField(oTuple.objectName);

            }
            oTuple.localPos = EditorFields.Option(oTuple.localPos, "Local Pos: ");
            oTuple.position = EditorFields.Vector3Field("Position: ", oTuple.position);
        }

        public void ShowHelpBox()
        {
            if (_ShowHelp) EditorGUILayout.HelpBox("Write the name of a game object in the scene to " +
                "use it's transform for the position to teleport the object.", MessageType.Info, true);
        }

        public void ToggleHelpBox()
        {
            _ShowHelp = !_ShowHelp;
        }
#endif


    }
}

