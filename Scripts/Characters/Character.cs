using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Characters
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
        public string translationKey;
        public string LastName = "";
        public string FirstName = "";
        public Texture2D DefaultSprite;
        public Texture2D TrialPortrait;
#if UNITY_EDITOR
        public List<Expression> Expressions = new List<Expression>();
#endif
        public List<Alias> Aliases = new List<Alias>();
        public Texture2D Nameplate;
        public Texture2D Headshot;
        public Texture2D TrialNameplate;
        public float TrialHeight = 7.88f;
    }
}
