using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Presents
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Presents/Present Database", fileName = "PresentDatabase")]
    public class PresentDatabase : ScriptableObject
    {
        public List<Present> presents;
    }
}

