
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;


namespace DREditor.CharacterEditor.Editor
{
    [CustomEditor(typeof(CharacterDatabase))]
    public class CharacterDatabaseEditor : UnityEditor.Editor
    {
        private CharacterDatabase cdb;
        public Material BaseExpression;

        private void OnEnable()
        {
            cdb = (CharacterDatabase) target;
        }


        public override void OnInspectorGUI()
        {
            Label("Character Database");

            if (GUILayout.Button("Add New Character"))
            {
                cdb.Characters.Add(CreateInstance<Character>());
            }

            if (cdb.Characters != null)
            {
                for (int i = 0; i < cdb.Characters.Count; i++)
                {
                    GUILayout.BeginHorizontal("Box");

                    
                    
                    if (cdb.Characters[i] != null)
                    {


                        GUIStyle expr = new GUIStyle();


                        if (cdb.Characters[i].Expressions.Count > 0 && cdb.Characters[i].Expressions[0].Sprite && cdb.Characters[i].Expressions.Count > 0)
                        {
                            var tex = cdb.Characters[i].Expressions[0].Sprite.GetTexture("_BaseMap") as Texture2D;
                            if(tex)
                            {
                                expr.normal.background = tex;
                            }
                        }

                        EditorGUILayout.LabelField(GUIContent.none, expr, GUILayout.Width(100), GUILayout.Height(100));
                    }

                    
                    GUILayout.BeginVertical("Box");
                    
                    var bigLabelStyle = new GUIStyle();
                    bigLabelStyle.fontSize = 25;
                    bigLabelStyle.fontStyle = FontStyle.Bold;
                    if (cdb.Characters[i] != null) GUILayout.Label(cdb.Characters[i].LastName + " "+ cdb.Characters[i].FirstName, bigLabelStyle);
                    
                    GUILayout.FlexibleSpace();
                    
                    if (cdb.Characters[i] != null) cdb.Characters[i] = (Character)EditorGUILayout.ObjectField(cdb.Characters[i], typeof(Character), false, GUILayout.Width(100));

                    if (GUILayout.Button("Remove", GUILayout.Width(100)))
                    {
                        cdb.Characters.Remove(cdb.Characters[i]);
                    }
                    
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            
            EditorUtility.SetDirty(cdb);
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
        

        private static void Label(string label)
        {
            GUI.backgroundColor = Color.white;
            var labelStyle = new GUIStyle();
            labelStyle.fontSize = 10;
            
            
            GUILayout.Label(label, labelStyle);
        }
        
    }
}
