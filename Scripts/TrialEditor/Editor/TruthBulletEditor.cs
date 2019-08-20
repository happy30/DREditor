using UnityEditor;
using UnityEngine;
using Utility.Editor;

namespace TrialEditor.Editor
{
    [CustomEditor(typeof(TruthBullet))]
    public class TruthBulletEditor : UnityEditor.Editor
    {
        private TruthBullet bullet;

        private void OnEnable()
        {
            bullet = (TruthBullet)target;
        }

        public override void OnInspectorGUI()
        {
            var titleStyle = new GUIStyle();
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label("Truth Bullet", titleStyle);
            GUILayout.BeginHorizontal(GUILayout.Width(170));
            bullet.picture = HandyFields.SpriteField("Picture: ", bullet.picture);
            if (GUILayout.Button("X", GUILayout.Width(18)))
            {
                bullet.picture = null;
            }
            GUILayout.EndHorizontal();
            bullet.title = HandyFields.StringField("Title: ", bullet.title);
            bullet.description = HandyFields.StringArea("Description: ", bullet.description);
            EditorUtility.SetDirty(bullet);
        }

        private static Texture2D TextureField(Texture2D texture)
        {
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label);

            style.fixedWidth = 70;
            var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(120), GUILayout.Height(120));
            GUILayout.EndVertical();
            return result;
        }

        private static string StringField(string name, string value)
        {
            GUI.backgroundColor = Color.white;
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(80));

            var result = EditorGUILayout.TextField(value, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            return result;
        }

        private static string StringArea(string name, string value)
        {
            GUI.backgroundColor = Color.white;
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(80));

            var result = EditorGUILayout.TextArea(value, GUILayout.Width(200), GUILayout.Height(60));
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            return result;
        }

        private static void Label(string label)
        {
            GUI.backgroundColor = Color.white;
            var labelStyle = new GUIStyle();
            labelStyle.fontSize = 10;


            GUILayout.Label(label, labelStyle);
        }
    }

}

