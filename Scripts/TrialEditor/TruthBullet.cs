using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.TrialEditor
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Trials/Truth Bullet", fileName = "New Truth Bullet")]
    public class TruthBullet : ScriptableObject
    {
        public Sprite Picture;
        public string Title;
        public string Description;
    }
}
