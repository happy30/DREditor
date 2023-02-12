using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CSharpVitamins;

namespace DREditor.Localization.Editor
{
    [CustomEditor(typeof(TranslatableTextDatabase))]
    public class TranslatableDatabaseEditor : UnityEditor.Editor
    {
        TranslatableTextDatabase database;

        private void OnEnable()
        {
            database = (TranslatableTextDatabase)target;
        }

        public override void OnInspectorGUI()
        {
            if(database.translatables == null)
            {
                database.translatables = new List<TranslatableText>();
            }


            database.isTranslation = EditorGUILayout.ToggleLeft("is a Translation?", database.isTranslation, GUILayout.Width(180));

            if(database.isTranslation)
            {
                TranslatableTextDatabase auxDB = (TranslatableTextDatabase) EditorGUILayout.ObjectField("Original Database", database.original, typeof(TranslatableTextDatabase), false);
                if(auxDB != null && !auxDB.Equals(database))
                {
                    database.original = auxDB;
                }
            }

            if(database.isTranslation && (database.original == null || database.original.GetTexts() == null))
            {
                GUILayout.Label("You need to specify the original database for this translation and it must contain at least one item");
                return;
            }

            if(GUILayout.Button("Add New", GUILayout.Width(80)))
            {
                database.translatables.Add(new TranslatableText());
            }

            for(int i=0;i<database.translatables.Count;i++)
            {
                TranslatableText item = database.translatables[i];
                EditorGUILayout.LabelField(new GUIContent("Translation Key: " + item.translationKey, "Internal key used to match the original with the translations"));
                if (database.isTranslation)
                {
                    EditorGUILayout.LabelField("Original Translation Key: " + ((item.original != null)?item.original.translationKey:""));
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Original", GUILayout.Width(50));
                    int selected = database.original.GetIndexByTranslationKey(item.translationKey) + 1;
                    
                    string[] displayOpts = PrependOption("<None>", database.original.GetTexts());
                    selected = EditorGUILayout.IntPopup(selected, displayOpts, GetIndexes(displayOpts.Length), GUILayout.Width(100));
                    if(selected > 0)
                    {
                        item.original = database.original.translatables[selected - 1];
                        item.translationKey = item.original.translationKey;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (string.IsNullOrEmpty(item.translationKey) && !database.isTranslation)
                {
                    item.translationKey = "text-" + ShortGuid.NewGuid();
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Text", GUILayout.Width(50));
                item.Text = EditorGUILayout.TextField(item.Text, GUILayout.Width(300));
                bool removed = false;
                if(GUILayout.Button("X", GUILayout.Width(20)))
                {
                    database.translatables.RemoveAt(i);
                    removed = true;
                }
                EditorGUILayout.EndHorizontal();
                if(!removed)
                {
                    database.translatables[i] = item;
                }
            }

            EditorUtility.SetDirty(database);
        }

        string[] PrependOption(string option, string[] options)
        {
            string[] newOptions = new string[options.Length + 1];
            newOptions[0] = option;
            for(int i=1;i<newOptions.Length;i++)
            {
                newOptions[i] = options[i - 1];
            }
            return newOptions;
        }

        int[] GetIndexes(int size)
        {
            int[] result = new int[size];
            for(int i=0;i<size;i++)
            {
                result[i] = i;
            }
            return result;
        }
    }
}
