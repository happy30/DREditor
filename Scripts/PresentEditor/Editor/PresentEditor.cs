using UnityEditor;
using UnityEngine;

using DRUtility = DREditor.Utility.Editor.HandyFields;

namespace DREditor.PresentEditor
{
    [CustomEditor(typeof(Present))]
    public class PresentEditor : Editor
    {
        private Present pres;
        private void OnEnable()
        {
            pres = (Present)target;
        }
        public override void OnInspectorGUI()
        {
            Label("Present");

            CreateForm();
            EditorUtility.SetDirty(pres);
        }
        private void Label(string label)
        {
            GUI.backgroundColor = Color.white;
            var labelStyle = new GUIStyle();
            labelStyle.fontSize = 10;
            GUILayout.Label(label, labelStyle);
        }
        private void CreateForm()
        {
            EditorGUILayout.BeginVertical("Box");
            pres.Name = DRUtility.StringField("Name: ", pres.Name);
            pres.index = DRUtility.IntField("No. ", pres.index);
            GUILayout.Label("Description:");
            pres.Description = EditorGUILayout.TextArea(pres.Description, GUILayout.Height(75), GUILayout.Width(Screen.width - 310));
            pres.image = DRUtility.UnityField("Image: ", pres.image);

            for (int i = 0; i < pres.CharacterReactions.Count; i++)
            {
                var reaction = pres.CharacterReactions[i];

                EditorGUILayout.BeginHorizontal("Box");

                EditorGUILayout.BeginVertical();
                GUIStyle expr = new GUIStyle();
                expr.normal.background = reaction.character.DefaultSprite;
                EditorGUILayout.LabelField(GUIContent.none, expr, GUILayout.Width(100), GUILayout.Height(100));
                reaction.character = DRUtility.UnityField("Reaction: ", reaction.character, 120, 20);
                EditorGUILayout.EndVertical();

                // Select Reaction
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                reaction.reactionLevel = (PresentReactionLevel)EditorGUILayout.EnumPopup(reaction.reactionLevel);
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            if (GUILayout.Button("Add New Character Reaction", GUILayout.Width(200)))
            {
                pres.CharacterReactions.Add(new StudentReactions());
            }
        }
    }
}