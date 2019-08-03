using System;
using UnityEngine;
using UnityEngine.Events;

namespace EventObjects
{

    [Serializable]
    [CreateAssetMenu(menuName = "EventObjects/Int", fileName = "New Int")]
    public class IntWithEvent : ValueWithEvent<int, IntEvent>, IConditional
    {
        public bool Resolve()
        {
            return true;
        }
    }
}