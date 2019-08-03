using System;
using UnityEngine;

namespace EventObjects
{
    [CreateAssetMenu(menuName = "EventObjects/Sprite", fileName = "New SpriteWithEvent")]
    public class SpriteWithEvent : ValueWithEvent<Sprite, SpriteEvent>, IConditional
    {
        public bool Resolve()
        {
            return Value; 
        }
    }

    [Serializable]
    public class SpriteVariable : CachedVariable<SpriteWithEvent, Sprite, SpriteEvent> {}
}