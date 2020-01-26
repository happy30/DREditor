
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace DREditor.CharacterEditor.Editor
{
    [CustomEditor(typeof(Student))]
    public class StudentEditor : UnityEditor.Editor
    {
        private Student stu;

        private void OnEnable()
        {
            stu = (Student) target;
        }


        public override void OnInspectorGUI()
        {
            Label("Character Editor");
            
            NameForm();
            StudentLabel();
            StudentCard();

            DefaultSprite();
            CharacterPortrait();

            Label("Sprites");

            Sprites();
            
            EditorUtility.SetDirty(stu);
        }
        
        private void NameForm()
        {
            EditorGUILayout.BeginVertical("Box");
            stu.LastName = StringField("Last Name: ", stu.LastName);
            stu.FirstName = StringField("First Name: ", stu.FirstName);
            stu.StudentCard.Color = ColorField(stu.StudentCard.Color);
            stu.Nameplate = TextureFieldLabeledHorizontal("Default Nameplate: ", stu.Nameplate);
            stu.TrialNameplate = TextureFieldLabeledHorizontal("Trial Nameplate: ", stu.TrialNameplate);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            AliasList(stu.Aliases);
            EditorGUILayout.EndVertical();
            
        }
        
        private void StudentLabel()
        {
            var bigLabelStyle = new GUIStyle();
            bigLabelStyle.fontSize = 25;
            bigLabelStyle.fontStyle = FontStyle.Bold;
            GUI.backgroundColor = stu.StudentCard.Color;
            GUILayout.Space(15);
            GUILayout.BeginHorizontal("box");
            GUILayout.FlexibleSpace();

            var labelText = stu.LastName == "" && stu.FirstName == "" ? "No Name" : "";
            
            GUILayout.Label(labelText + stu.LastName + " " + stu.FirstName, bigLabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            bigLabelStyle.fontSize = 15;
            
            GUILayout.BeginHorizontal("box");
            GUILayout.FlexibleSpace();
            GUILayout.Label("Ultimate " + stu.StudentCard.Talent, bigLabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            
            GUILayout.Space(15);
        }

        private void StudentCard()
        {
            var cardStyle = new GUIStyle();
            
            
            EditorGUILayout.BeginVertical("Box");
            
            
            
            stu.StudentCard.Talent = StringField("Talent: ", stu.StudentCard.Talent);
            stu.StudentCard.Height = StringField("Height: ", stu.StudentCard.Height);
            stu.StudentCard.Weight = StringField("Weight: ", stu.StudentCard.Weight);
            stu.StudentCard.Chest = StringField("Chest: ", stu.StudentCard.Chest);
            stu.StudentCard.BloodType = StringField("Blood Type: ", stu.StudentCard.BloodType);
            stu.StudentCard.DateOfBirth = StringField("D.O.B.: ", stu.StudentCard.DateOfBirth);
            stu.StudentCard.Likes = StringField("Likes: ", stu.StudentCard.Likes);
            stu.StudentCard.Dislikes = StringField("Dislikes: ", stu.StudentCard.Dislikes);
            stu.StudentCard.Notes = StringArea("Description: ", stu.StudentCard.Notes);
            
            EditorGUILayout.EndVertical();
        }

        private void DefaultSprite()
        {
            Label("Default Sprite");

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("Box", GUILayout.Width(140));
            GUILayout.BeginHorizontal();
            stu.DefaultSprite = TextureField(stu.DefaultSprite);
            if(GUILayout.Button("X", GUILayout.Width(18)))
            {
                stu.DefaultSprite = null;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void CharacterPortrait()
        {
            Label("Character Portrait");

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("Box", GUILayout.Width(140));
            GUILayout.BeginHorizontal();
            stu.TrialPortrait = TextureField(stu.TrialPortrait);
            if(GUILayout.Button("X", GUILayout.Width(18)))
            {
                stu.TrialPortrait = null;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void Sprites()
        {
            var count = 0;
            GUILayout.BeginHorizontal();
            
            for (var i = 0; i < stu.Expressions.Count; i++)
            {
                var expr = stu.Expressions[i];
                GUILayout.BeginVertical("Box", GUILayout.Width(140));
                GUILayout.BeginHorizontal();
                expr.Sprite = TextureField(expr.Sprite);
                if(GUILayout.Button("X",GUILayout.Width(18)))
                {
                    stu.Expressions.Remove(expr);
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
                stu.Expressions.Add(new Expression());
                
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

        private static Texture2D TextureFieldLabeledHorizontal(string label, Texture2D texture)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var result =
                (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
            EditorGUILayout.EndHorizontal();
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

        private static Color ColorField(Color color)
        {
            GUI.backgroundColor = Color.white;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Color: ");
            var result = EditorGUILayout.ColorField(color, GUILayout.Width(40));
            GUILayout.EndHorizontal();
            return result;
        }
        
        public static Texture2D ToTexture2D(Texture texture)
        {
            return Texture2D.CreateExternalTexture(
                texture.width,
                texture.height,
                TextureFormat.RGB24,
                false, false,
                texture.GetNativeTexturePtr());
        }

        private static void AliasList(List<Alias> aliasList)
        {
            if (GUILayout.Button("Add Alias", GUILayout.Width(120)))
            {
                aliasList.Add(new Alias());
            }
            if (aliasList != null)
            {
                for (int i = 0; i < aliasList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    aliasList[i].Name = StringField("Alias " + i + ": ",aliasList[i].Name);
                    var removed = GUILayout.Button("x", GUILayout.Width(20));
                    EditorGUILayout.EndHorizontal();
                    aliasList[i].Nameplate = TextureFieldLabeledHorizontal("Nameplate:", aliasList[i].Nameplate);
                    if (removed)
                    {
                        aliasList.RemoveAt(i);
                    }
                }
            }

            
        }
    }
}
