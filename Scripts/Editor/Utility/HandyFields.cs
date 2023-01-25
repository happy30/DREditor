#if UNITY_EDITOR
/**
 * Utility Functions for DREditor Editors
 * Original Author: Jordi
 * Contributing Author: KHeartz
 */

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
            Texture2D result;
            using (new EditorGUILayout.VerticalScope())
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    fixedWidth = 70
                };
                result = UnityField(texture, width, height);
            }
            return result;
        }

        public static int IntField(string name, int value, int textBoxWidth = 200, int labelBoxMulti = 6)
        {
            GUI.backgroundColor = Color.white;
            int result;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(name, GUILayout.Width(name.Length * labelBoxMulti));
                result = EditorGUILayout.IntField(value, GUILayout.Width(textBoxWidth));
            }
            GUILayout.FlexibleSpace();
            return result;
        }

        public static float FloatField(string label, float value, int textBoxWidth = 200, int labelBoxMulti = 6)//*Added int field functionality
        {
            GUI.backgroundColor = Color.white;
            float result;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(label.Length * labelBoxMulti));
                result = EditorGUILayout.FloatField(value, GUILayout.Width(textBoxWidth));
            }
            GUILayout.FlexibleSpace();
            return result;
        }

        public static Vector3 Vector3Field(string label, Vector3 value, int textBoxWidth = 200, int labelBoxMulti = 6)//*
        {
            GUI.backgroundColor = Color.white;
            Vector3 result;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(label.Length * labelBoxMulti));
                result = EditorGUILayout.Vector3Field("",value, GUILayout.Width(textBoxWidth));
            }
            GUILayout.FlexibleSpace();
            return result;
        }
        public static Vector4 Vector4Field(string label, Vector4 value, int textBoxWidth = 200, int labelBoxMulti = 6)//*
        {
            GUI.backgroundColor = Color.white;
            Vector3 result;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(label.Length * labelBoxMulti));
                result = EditorGUILayout.Vector4Field("", value, GUILayout.Width(textBoxWidth));
            }
            GUILayout.FlexibleSpace();
            return result;
        }
        public static int Popup(string label, int value, string[] arr, int textBoxWidth = 150, int labelBoxMulti = 7)//*
        {
            GUI.backgroundColor = Color.white;
            int result;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(label.Length * labelBoxMulti));
                result = EditorGUILayout.Popup(value, arr, GUILayout.Width(textBoxWidth));
            }
            GUILayout.FlexibleSpace();
            return result;
        }
        public static Sprite SpriteField(string name, Sprite sprite)
        {
            GUILayout.Label("Picture: ", GUILayout.Width(80));
            Sprite result;
            using (new EditorGUILayout.VerticalScope())
            {
                result = UnityField(sprite);
            }
            return result;
        }

        public static T UnityField<T>(T data, int width = 120, int height = 120) where T : Object
        {
            T result;
            using (new EditorGUILayout.VerticalScope())
            {
                result = (T)EditorGUILayout.ObjectField(data, typeof(T), false, GUILayout.Width(width), GUILayout.Height(height));
            }
            return result;
        }

        public static string StringField(string name, string value, string label = null, int textWidth = 200, float nameWidth = 80)
        {
            GUI.backgroundColor = Color.white;
            string result;
            using (new EditorGUILayout.HorizontalScope())
            {
                if(label == null)
                    GUILayout.Label(name, GUILayout.Width(nameWidth));
                else
                    GUILayout.Label(label, GUILayout.Width(80));
                result = EditorGUILayout.TextField(value, GUILayout.Width(textWidth));
            }
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

        public static Color ColorField(Color color)
        {
            GUI.backgroundColor = Color.white;
            Color result;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Color: ");
                result = EditorGUILayout.ColorField(color, GUILayout.Width(40));
            }
            return result;
        }

        public static string StringArea(string name, string value)
        {
            GUI.backgroundColor = Color.white;
            string result;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(name, GUILayout.Width(80));
                result = EditorGUILayout.TextArea(value, GUILayout.Width(200), GUILayout.Height(60));
            }
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
        public static Texture2D GetMaterialTexture(Material material)
        {
            return material.GetTexture(
#if DR_URP
                "_BaseMap"
#else
                "_MainTex"
#endif
                ) as Texture2D;
        }

        public static bool Option(bool option, string label, float labelWidth = 60)
        {
            GUILayout.Label(label, GUILayout.Width(labelWidth));
            return EditorGUILayout.Toggle(option, GUILayout.Width(15));
        }

        public static void UISplitter(Color color, int thickness = 1, int horizontalpadding = 5, int verticalpadding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(verticalpadding + thickness));
            r.height = thickness;
            r.y += verticalpadding / 2;
            r.x += horizontalpadding;
            r.width -= horizontalpadding * 2;
            EditorGUI.DrawRect(r, color);
        }
    }
}
#endif