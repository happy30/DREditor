using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using DREditor.Localization;

using TMPro;

namespace DREditor.UI.Editor
{
    [CustomEditor(typeof(LabelMapper))]
    public class LabelMapperEditor : UnityEditor.Editor
    {
        private TranslatableTextDatabase database;

        LabelMapper mapper;

        private void OnEnable()
        {
            mapper = (LabelMapper)target;
        }

        public override void OnInspectorGUI()
        {
            database = Resources.Load<TranslatableTextDatabase>("Texts/TranslatablesDatabase");
            if(!database)
            {
                GUILayout.BeginVertical();
                EditorGUILayout.LabelField("TranslatablesDatabase is not set.");
                EditorGUILayout.LabelField("Create a TranslatablesDatabase in Resources/Texts/TranslatablesDatabase.asset");
                GUILayout.EndVertical();
                return;
            }


            SerializedObject mapperObj = new SerializedObject(mapper);
            SerializedProperty labelMapListProp = mapperObj.FindProperty("labelMapList");

            // Update the list
            mapperObj.Update();


            EditorGUILayout.Space();
            if(GUILayout.Button("Add New", GUILayout.Width(80)))
            {
                Undo.RecordObject(mapper, "Added new mapping");
                mapper.labelMapList.Add(new LabelAndText());
                Undo.FlushUndoRecordObjects();
            }
            if(labelMapListProp.arraySize > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Label", GUILayout.Width(200));
                GUILayout.Label("Text", GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();
            }
            for(int i=0;i<labelMapListProp.arraySize;i++)
            {
                EditorGUILayout.BeginHorizontal();
                SerializedProperty item = labelMapListProp.GetArrayElementAtIndex(i);
                SerializedProperty label = item.FindPropertyRelative("label");
                label.objectReferenceValue = EditorGUILayout.ObjectField(label.objectReferenceValue, typeof(TMP_Text), true, GUILayout.Width(200));
                
                string[] displayOpts = PrependOption("<None>", database.GetTexts());
                int oldIndex = 0;
                if(mapper.labelMapList[i].text != null)
                {
                    oldIndex = database.GetIndexByTranslationKey(mapper.labelMapList[i].text.translationKey) + 1;
                }
                int selected = EditorGUILayout.IntPopup(oldIndex, displayOpts, GetIndexes(displayOpts.Length), GUILayout.Width(200));
                if(oldIndex != selected)
                {
                    Undo.RecordObject(mapper, "Changed a mapping");
                    if(selected > 0)
                    {
                        mapper.labelMapList[i].text = database.translatables[selected - 1];
                    } else
                    {
                        mapper.labelMapList[i].text = null;
                    }
                    Undo.FlushUndoRecordObjects();
                }
                if(GUILayout.Button("X", GUILayout.Width(20)))
                {
                    labelMapListProp.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            mapperObj.ApplyModifiedProperties();
        }

        string[] PrependOption(string option, string[] options)
        {
            string[] newOptions = new string[options.Length + 1];
            newOptions[0] = option;
            for (int i = 1; i < newOptions.Length; i++)
            {
                newOptions[i] = options[i - 1];
            }
            return newOptions;
        }

        int[] GetIndexes(int size)
        {
            int[] result = new int[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = i;
            }
            return result;
        }
    }
}
