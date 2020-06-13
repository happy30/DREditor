using UnityEditor;
using UnityEngine;

using DRUtility = DREditor.Utility.Editor.HandyFields;

namespace DREditor.PresentEditor
{
    [CustomEditor(typeof(Present))]
    public class PresentEditor : Editor
    {
        private Present pres;

        SerializedProperty propName;
        SerializedProperty propDescription;
        SerializedProperty propIndex;
        SerializedProperty propImage;
        private void OnEnable()
        {
            pres = (Present)target;
            propName = serializedObject.FindProperty("Name");
            propDescription = serializedObject.FindProperty("Description");
            propIndex = serializedObject.FindProperty("index");
            propImage = serializedObject.FindProperty("image");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DRUtility.Label("Present");
            CreateForm();
            serializedObject.ApplyModifiedProperties();
        }

        private void CreateForm()
        {
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                DRUtility.LabeledPropertyField("Name: ", propName);
                DRUtility.LabeledPropertyField("No. ", propIndex);
                GUILayout.Label("Description:");
                propDescription.stringValue = EditorGUILayout.TextArea(propDescription.stringValue, GUILayout.Height(75), GUILayout.Width(Screen.width - 310));
                DRUtility.LabeledPropertyFieldGenericBG<Sprite>("Image: ", propImage);

                for (int i = 0; i < pres.CharacterReactions.Count; i++)
                {
                    var reaction = pres.CharacterReactions[i];

                    using (new EditorGUILayout.HorizontalScope("Box"))
                    {
                        using (new EditorGUILayout.VerticalScope())
                        {
                            GUIStyle expr = new GUIStyle();
                            expr.normal.background = reaction.character.DefaultSprite;
                            EditorGUILayout.LabelField(GUIContent.none, expr, GUILayout.Width(100), GUILayout.Height(100));
                            reaction.character = DRUtility.UnityField(reaction.character, 120, 20);
                        }

                        // Select Reaction
                        using (new EditorGUILayout.VerticalScope())
                        {
                            GUILayout.FlexibleSpace();
                            reaction.reactionLevel = (PresentReactionLevel)EditorGUILayout.EnumPopup(reaction.reactionLevel);
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }
                }
            }
            if (GUILayout.Button("Add New Character Reaction", GUILayout.Width(200)))
            {
                pres.CharacterReactions.Add(new StudentReactions());
            }
        }
    }
}