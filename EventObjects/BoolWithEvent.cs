using System;
using UnityEngine;

namespace EventObjects
{
    [CreateAssetMenu(menuName = "EventObjects/Bool", fileName = "New BoolWithEvent")]
    public class BoolWithEvent : ValueWithEvent<bool, BoolEvent>, IConditional
    {
        public bool Resolve()
        {
            return Value; 
        }
    }

    [Serializable]
    public class BoolVariable : CachedVariable<BoolWithEvent, bool, BoolEvent> {}
}
