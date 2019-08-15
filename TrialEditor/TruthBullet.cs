using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrialEditor
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DRSimulator/Truth Bullet", fileName = "New Truth Bullet")]
    public class TruthBullet : ScriptableObject
    {
        public Sprite picture;
        public string title;
        public string description;
    }
}
