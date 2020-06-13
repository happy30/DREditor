using System.Collections.Generic;
using UnityEngine;

using DRStudent = DREditor.CharacterEditor.Student;

namespace DREditor.PresentEditor
{
    [System.Serializable]
    public enum PresentReactionLevel
    {
        Love,
        Like,
        Neutral,
        Dislike
    }

    [System.Serializable]
    public class StudentReactions
    {
        public DRStudent character;
        public PresentReactionLevel reactionLevel;
    }

    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Presents/Present", fileName = "Present")]
    public class Present : ScriptableObject
    {
        public string Name;
        public string Description;
        public int index;
        [PreviewSprite] public Sprite image;
        public List<StudentReactions> CharacterReactions;
    }
}


namespace UnityEngine
{
    public class PreviewSpriteAttribute : PropertyAttribute
    {
        public PreviewSpriteAttribute() { }
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
