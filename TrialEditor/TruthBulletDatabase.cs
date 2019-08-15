using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrialEditor
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DRSimulator/TruthBulletDatabase", fileName = "TruthBulletDatabase")]
    public class TruthBulletDatabase : ScriptableObject
    {
        public List<TruthBullet> bullets;
    }
}
