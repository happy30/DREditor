/**
 * Present Editor for DREditor
 * Original Author: KHeartz
 */

using UnityEditor;
using UnityEngine;

using DRUtility = DREditor.Utility.Editor.HandyFields;

namespace DREditor.Presents.Editor
{
    [CustomEditor(typeof(Present))]
    public class PresentEditor : UnityEditor.Editor
    {
        private Present pres;

        private SerializedProperty propName;
        private SerializedProperty propDescription;
        private SerializedProperty propIndex;
        private SerializedProperty propImage;

        private void OnEnable()
        {
            pres = target as Present;
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
                        ShowStudent(reaction);
                        SelectReaction(reaction);

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

        private void ShowStudent(StudentReactions reaction)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUIStyle expr = new GUIStyle();
                expr.normal.background = reaction.character.DefaultSprite;
                EditorGUILayout.LabelField(GUIContent.none, expr, GUILayout.Width(100), GUILayout.Height(100));
                reaction.character = DRUtility.UnityField(reaction.character, 120, 20);
            }
        }

        private void SelectReaction(StudentReactions reaction)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.FlexibleSpace();
                reaction.reactionLevel = (PresentReactionLevel)EditorGUILayout.EnumPopup(reaction.reactionLevel);
                GUILayout.FlexibleSpace();
            }
        }
    }
}

namespace UnityEditor
{
    [CustomPropertyDrawer(typeof(PreviewSpriteAttribute))]
    public class PreviewSpriteDrawer : PropertyDrawer
    {
        const float imageHeight = 100;

        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                (property.objectReferenceValue as Sprite) != null)
            {
                return EditorGUI.GetPropertyHeight(property, label, true) + imageHeight + 10;
            }
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        static string GetPath(SerializedProperty property)
        {
            string path = property.propertyPath;
            int index = path.LastIndexOf(".");
            return path.Substring(0, index + 1);
        }

        public override void OnGUI(Rect position,
                                    SerializedProperty property,
                                    GUIContent label)
        {
            //Draw the normal property field
            EditorGUI.PropertyField(position, property, label, true);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                var sprite = property.objectReferenceValue as Sprite;
                if (sprite != null)
                {
                    position.y += EditorGUI.GetPropertyHeight(property, label, true) + 5;
                    position.height = imageHeight;
                    //EditorGUI.DrawPreviewTexture(position, sprite.texture, null, ScaleMode.ScaleToFit, 0);
                    GUI.DrawTexture(position, sprite.texture, ScaleMode.ScaleToFit);
                }
            }
        }
    }
}
