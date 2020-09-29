/**
 * Present Class for DREditor
 * Original Author: KHeartz
 */

using System.Collections.Generic;
using UnityEngine;

using DRStudent = DREditor.Characters.Student;

namespace DREditor.Presents
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
