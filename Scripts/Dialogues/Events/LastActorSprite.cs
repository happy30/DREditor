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
    public struct LASTuple
    {
        public int charNum;
        public int spriteNum;
    }
    /// <summary>
    /// Changes Last actor sprite in the middle of internal dialogue
    /// Function found in DialogueAnimConfig.cs
    /// </summary>
    [Serializable]
    public class LastActorSprite : IDialogueEvent
    {
        public bool _ShowHelp = false;
        public LASTuple data;
        public void TriggerDialogueEvent()
        {
            DialogueEventSystem.TriggerEvent("ChangeLast", data);
        }

#if UNITY_EDITOR
        CharacterDatabase database;
        public void EditorUI(object value = null)
        {
            if (!database)
                database = GetDatabase();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Character to Change: ", GUILayout.Width(135));
                data.charNum = EditorGUILayout.IntPopup(data.charNum, database.GetNames().ToArray(),
                    database.GetInts().ToArray(), GUILayout.Width(130));

            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Sprite: ", GUILayout.Width(135));
                Character c = database.Characters[data.charNum];
                var expressionNames = new string[c.Expressions.Count + 1];
                expressionNames[0] = "<No change>";

                for (int j = 1; j < c.Expressions.Count + 1; j++)
                {
                    expressionNames[j] = c.Expressions[j - 1].Name;
                }
                data.spriteNum = EditorGUILayout.IntPopup(data.spriteNum, expressionNames,
                    c.getExpressionIntValues(), GUILayout.Width(130));

            }
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
            if (_ShowHelp) EditorGUILayout.HelpBox("Changes Last actor sprite in the middle of internal dialogue.", MessageType.Info, true);
        }

        public void ToggleHelpBox()
        {
            _ShowHelp = !_ShowHelp;
        }
#endif
    }
}
