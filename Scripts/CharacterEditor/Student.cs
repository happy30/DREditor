using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DRSimulator/Character", fileName = "New Character")]
    public class Student : ScriptableObject
    {
        public Character Character;
        public List<Expression> Expressions = new List<Expression>();
        public StudentCard StudentCard;
    }
}

