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
        [Tooltip("Currently used as a variable for Splash Arts")]
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

        /// <summary>
        /// Returns the full name of the character.
        /// </summary>
        /// <param name="jp">If true, return name in Japanese (Last Name, First Name) basis.</param>
        /// <returns></returns>
        public string FullName(bool jp)
        {
            string name;
            if (!string.IsNullOrWhiteSpace(LastName))
            {
                name = jp ? $"{LastName} {FirstName}" : $"{FirstName} {LastName}";
            }
            else
            {
                name = FirstName;
            }
            return name;
        }
    }
}
