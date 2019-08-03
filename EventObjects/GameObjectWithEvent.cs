using System;
using UnityEngine;
using UnityEngine.Events;

namespace EventObjects
{

    [Serializable]
    [CreateAssetMenu(menuName = "EventObjects/GameObject", fileName = "New GameObject")]
    public class GameObjectWithEvent : ValueWithEvent<GameObject, GameObjectEvent>, IConditional
    {
        public bool Resolve()
        {
            return true;
        }
    }
}