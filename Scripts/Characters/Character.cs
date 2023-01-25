using System.Collections.Generic;
using UnityEngine;
using DREditor.PlayerInfo;
namespace DREditor.Characters
{
	[System.Serializable]
	public class Alias
	{
		public string Name;
		public Texture2D Nameplate;
        public Texture2D TrialNameplate;
        public Texture2D TrialPortrait;
	}
    // Any properties marked //* were edits made by Benjamin "Sweden" Jillson : Sweden#6386
    [System.Serializable]
    public class Character : ScriptableObject
    {
        public string translationKey;
        public string LastName = "";
        public string FirstName = "";
        public Texture2D DefaultSprite;
        public Texture2D TrialPortrait;
        public Texture2D NSDPortrait;//*
        public GameObject ActorPrefab;//*
#if UNITY_EDITOR

        public bool showSprites = false; // For editor window's use
#endif
        public Material BlackExpression;//*
        public Material TrialMaterial;
        public Material MissingMat;//*
        public Sprite MissingTex;//*
        public List<Expression> Expressions = new List<Expression>();
        public List<Unlit> Sprites = new List<Unlit>();// for using sprite renderers

        public List<Alias> Aliases = new List<Alias>();
        public Texture2D Nameplate;
        public Texture2D Headshot;
        public Texture2D TrialNameplate;
        public float TrialHeight = 7.88f;
        public int TrialPosition = 0;
        public bool IsDead = false;

        // FTE Stuff below, might make a struct of it later
        public int FriendshipLvl = 1;
        
        public int[] getExpressionIntValues()
        {

            int[] values = new int[Expressions.Count + 1];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = i;
            }
            return values;
        }
        public Texture GetSpriteByName(string name)
        {
            foreach(Unlit u in Sprites)
            {
                if (u.Name == name)
                    return u.Sprite.texture;
            }
            UnityEngine.Debug.LogWarning("COULDN'T GET SPRITE BY NAME: " + name);
            return null;
        }
        public string GetSpriteLabelByTexName(string name)
        {
            foreach (Unlit u in Sprites)
            {
                if (u.Sprite.texture.name == name)
                    return u.Name;
            }
            UnityEngine.Debug.LogWarning("COULDN'T GET SPRITE LABEL BY TEX NAME: " + name);
            return null;
        }
    }
}
