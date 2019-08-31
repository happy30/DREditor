using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.CharacterEditor
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Characters/Student", fileName = "New Student")]
    public class Student : Character
    {
        //public Character Character;
        public StudentCard StudentCard;

        //void OnEnable()
        //{
        //    if (Character == null)
        //    {
        //        Character = CreateInstance<Character>();
        //    }
        //}
    }
}

