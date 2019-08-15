using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Utility.Editor
{
    /// <summary>
    /// Class with shorthand methods for Editor GUI layout.
    /// </summary>
    public class HandyFields
    {
        public static Texture2D TextureField(Texture2D texture)
        {
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label);

            style.fixedWidth = 70;
            var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(120), GUILayout.Height(120));
            GUILayout.EndVertical();
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
            var labelStyle = new GUIStyle();
            labelStyle.fontSize = 10;


            GUILayout.Label(label, labelStyle);
        }
    }
}
