using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DREditor.Utility.Editor
{
    /// <summary>
    /// Class with shorthand methods for Editor GUI layout.
    /// </summary>
    public static class HandyFields
    {
        public static Texture2D TextureField(Texture2D texture, int width = 120, int height = 120)
        {
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label);

            style.fixedWidth = 70;
            var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(width), GUILayout.Height(height));
            GUILayout.EndVertical();
            return result;
        }

        public static int IntField(string name, int value, int textBoxWidth = 200)
        {
            GUI.backgroundColor = Color.white;
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(80));

            var result = EditorGUILayout.IntField(value, GUILayout.Width(textBoxWidth));
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            return result;
        }

        public static Sprite SpriteField(string name, Sprite sprite)
        {
            GUILayout.Label("Picture: ", GUILayout.Width(80));
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label);
            style.fixedWidth = 70;
            var result = (Sprite)EditorGUILayout.ObjectField(sprite, typeof(Sprite), false, GUILayout.Width(120), GUILayout.Height(120));
            GUILayout.EndVertical();
            return result;
        }

        public static T UnityField<T>(T data, int width = 120, int height = 120) where T : Object
        {
            GUILayout.BeginVertical();
            var result = (T)EditorGUILayout.ObjectField(data, typeof(T), false, GUILayout.Width(width), GUILayout.Height(height));
            GUILayout.EndVertical();
            return result;
        }

        public static string StringField(string name, string value)
        {
            GUI.backgroundColor = Color.white;
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(80));

            var result = EditorGUILayout.TextField(value, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            return result;
        }
        public static void LabeledPropertyField(string label, SerializedProperty prop)
        {
            GUI.backgroundColor = Color.white;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUIUtility.labelWidth = 80;
                EditorGUILayout.PropertyField(prop, new GUIContent(label), GUILayout.Width(200));
                EditorGUIUtility.labelWidth = 0;
            }
            GUILayout.FlexibleSpace();
        }
        public static void LabeledPropertyFieldGenericBG<T>(string name, SerializedProperty data, int width = 120, int height = 120) where T : Object
        {
            GUILayout.Label(name, GUILayout.Width(80));
            using (new EditorGUILayout.VerticalScope())
            {
                var result = (T)EditorGUILayout.ObjectField(data.objectReferenceValue as T, typeof(T), false, GUILayout.Width(width), GUILayout.Height(height));
            }
        }

        public static string StringArea(string name, string value)
        {
            GUI.backgroundColor = Color.white;
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(80));

            var result = EditorGUILayout.TextArea(value, GUILayout.Width(200), GUILayout.Height(60));
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            return result;
        }
        public static void Label(string label)
        {
            GUI.backgroundColor = Color.white;
            var labelStyle = new GUIStyle
            {
                fontSize = 10
            };
            GUILayout.Label(label, labelStyle);
        }
    }
}
