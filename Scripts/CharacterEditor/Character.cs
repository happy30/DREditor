using System.Collections.Generic;

namespace CharacterEditor
{
    [System.Serializable]
    public class Character
    {
        public string LastName;
        public string FirstName;
        public List<Expression> Expressions = new List<Expression>();
    }
}
