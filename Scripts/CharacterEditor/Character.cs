using System.Collections.Generic;
using UnityEngine;

namespace DREditor.CharacterEditor
{
	[System.Serializable]
	public class Alias
	{
		public string Name;
		public Texture2D Nameplate;
	}

    [System.Serializable]
    public class Character : ScriptableObject
    {
        public string LastName = "";
        public string FirstName = "";
        public List<Expression> Expressions = new List<Expression>();
        public List<Alias> Aliases = new List<Alias>();
        public Texture2D Nameplate;
    }
}
