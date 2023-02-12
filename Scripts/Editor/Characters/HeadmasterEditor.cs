using UnityEditor;
using UnityEngine;

using DREditor.Utility.Editor;

namespace DREditor.Characters.Editor
{
    [CustomEditor(typeof(Headmaster))]
    public class HeadmasterEditor : UnityEditor.Editor
    {
        private Headmaster hms;

        private void OnEnable() => hms = target as Headmaster;


        public override void OnInspectorGUI()
        {
            Label("Headmaster Editor");

            HeadmasterLabel();
            DefaultSprite();

            NameForm();
            Label("Sprites");
            Sprites();

            EditorUtility.SetDirty(hms);
        }

        private void HeadmasterLabel()
        {
            var bigLabelStyle = new GUIStyle
            {
                fontSize = 25,
                fontStyle = FontStyle.Bold
            };
            GUILayout.Space(15);
            using (new EditorGUILayout.HorizontalScope("box"))
            {
                GUILayout.FlexibleSpace();

                var labelText = hms.LastName == "" && hms.FirstName == "" ? "No Name" : "";

                GUILayout.Label(labelText + hms.LastName + " " + hms.FirstName, bigLabelStyle);
                GUILayout.FlexibleSpace();
            }
            bigLabelStyle.fontSize = 15;
        }

        private void NameForm()
        {
            EditorGUILayout.BeginVertical("Box");
            //hms.LastName = StringField("Last Name: ", hms.LastName);
            hms.FirstName = StringField("Name: ", hms.FirstName);
            hms.Nameplate = TextureFieldLabeledHorizontal("Default Nameplate: ", hms.Nameplate);
            hms.Headshot = TextureFieldLabeledHorizontal("Default Headshot: ", hms.Headshot);
            hms.TrialNameplate = TextureFieldLabeledHorizontal("Trial Nameplate: ", hms.TrialNameplate);
            hms.TrialHeight = HandyFields.FloatField("Trial Height: ", hms.TrialHeight);
            EditorGUILayout.EndVertical();
            
        }

        private void DefaultSprite()
        {
            Label("Default Sprite");

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("Box", GUILayout.Width(140));
            GUILayout.BeginHorizontal();
            hms.DefaultSprite = TextureField(hms.DefaultSprite);
            if (GUILayout.Button("X", GUILayout.Width(18)))
            {
                hms.DefaultSprite = null;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void Sprites()
        {
            var count = 0;
            GUILayout.BeginHorizontal();

            
            
            for (var i = 0; i < hms.Expressions.Count; i++)
            {
                var expr = hms.Expressions[i];
                GUILayout.BeginVertical("Box", GUILayout.Width(140));
                GUILayout.BeginHorizontal();
                expr.Sprite = MaterialField(expr.Sprite);
                if(GUILayout.Button("X",GUILayout.Width(18)))
                {
                    hms.Expressions.Remove(expr);
                }

                GUILayout.EndHorizontal();
                expr.Name = EditorGUILayout.TextField(expr.Name, GUILayout.Width(120));
                GUILayout.EndVertical();


                count++;
                if (count > 1)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    count = 0;
                }

                
            }

            if (GUILayout.Button("Add \n Sprite", GUILayout.Width(100), GUILayout.Height(40)))
            {
                hms.Expressions.Add(new Expression());
                
            }
            
            
            
            GUILayout.EndHorizontal();
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

        private static Material MaterialField(Material mat)
        {
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label);
            style.fixedWidth = 70;
            var result = EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.Width(120), GUILayout.Height(120)) as Material;
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
        
        private static void Label(string label)
        {
            GUI.backgroundColor = Color.white;
            var labelStyle = new GUIStyle();
            labelStyle.fontSize = 10;
            
            
            GUILayout.Label(label, labelStyle);
        }

        private static Texture2D TextureFieldLabeledHorizontal(string label, Texture2D texture)
        {
            Texture2D result;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                result = HandyFields.UnityField(texture, 70, 70);
            }
            return result;
        }
    }
}
