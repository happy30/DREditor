using System;
using UnityEngine;
using UnityEngine.Events;

namespace EventObjects
{

    [Serializable]
    [CreateAssetMenu(menuName = "EventObjects/Float", fileName = "New Float")]
    public class FloatWithEvent : ValueWithEvent<float, FloatEvent>, IConditional
    {
        public bool Resolve()
        {
            return true;
        }
    }
    
    [Serializable]
    public class FloatVariable : CachedVariable<FloatWithEvent, float, FloatEvent> {}
}