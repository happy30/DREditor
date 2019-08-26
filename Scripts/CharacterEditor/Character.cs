using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    [System.Serializable]
    public class Character : ScriptableObject
    {
        public string LastName = "";
        public string FirstName = "";
        public List<Expression> Expressions = new List<Expression>();
    }
}
