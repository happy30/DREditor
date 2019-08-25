using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Characters/Headmaster", fileName = "New Headmaster")]
    public class Headmaster : ScriptableObject
    {
        public Character Character;
    }
}