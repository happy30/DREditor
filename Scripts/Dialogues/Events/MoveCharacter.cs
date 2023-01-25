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
    public struct MCTuple
    {
        public int charNum;
        public Vector3 position;
        public Vector3 rotation;
        public bool all;
        //public bool moveSprite;
    }
    /// <summary>
    /// Moves a character to a position
    /// Function found in DialogueEventHandler.cs
    /// </summary>
    [Serializable]
    public class MoveCharacter : IDialogueEvent
    {
        public bool _ShowHelp = false;
        public MCTuple data;
        public void TriggerDialogueEvent()
        {
            DialogueEventSystem.TriggerEvent("MoveCharacter", data);
        }

#if UNITY_EDITOR
        CharacterDatabase database;
        public void EditorUI(object value = null)
        {
            if (!database)
                database = GetDatabase();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Character to move: ", GUILayout.Width(135));
                data.charNum = EditorGUILayout.IntPopup(data.charNum, database.GetNames().ToArray(),
                    database.GetInts().ToArray(), GUILayout.Width(130));
                
            }
            data.position = Vector3Field("Position to go to: ", data.position);
            data.rotation = Vector3Field("Rotation to go to: ", data.rotation);
            using (new EditorGUILayout.HorizontalScope())
            {
                data.all = EditorFields.Option(data.all, "Move All: ");
            }
            //data.moveSprite = EditorFields.Option(data.moveSprite, "Move SubPiece: ");
        }
        CharacterDatabase GetDatabase()
        {
            string[] _databaseguids = AssetDatabase.FindAssets("t:CharacterDatabase");
            string _databasepath = AssetDatabase.GUIDToAssetPath(_databaseguids[0]);
            EditorGUILayout.LabelField($"Loading CharacterDatabase at {_databasepath}.");
            return AssetDatabase.LoadAssetAtPath<CharacterDatabase>(_databasepath);
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
        public void ShowHelpBox()
        {
            if (_ShowHelp) EditorGUILayout.HelpBox("Move a character in the scene to a position.", MessageType.Info, true);
        }

        public void ToggleHelpBox()
        {
            _ShowHelp = !_ShowHelp;
        }
#endif
    }
}
