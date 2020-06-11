using UnityEngine;

namespace DREditor.CharacterEditor
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Characters/Student", fileName = "New Student")]
    public class Student : Character
    {
        public StudentCard StudentCard;
    }
}

