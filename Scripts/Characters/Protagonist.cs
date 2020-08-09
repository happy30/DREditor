using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Characters
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Characters/Protagonist", fileName = "New Protagonist")]
    public class Protagonist : Character
    {
        public Color ProtagonistColor;
    }

}

