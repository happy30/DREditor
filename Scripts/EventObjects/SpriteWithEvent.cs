using System;
using UnityEngine;

namespace EventObjects
{
    [CreateAssetMenu(menuName = "DREditor/EventObjects/Sprite", fileName = "New SpriteWithEvent")]
    public class SpriteWithEvent : ValueWithEvent<Sprite, SpriteEvent>
    {
        public bool Resolve()
        {
            return Value; 
        }
    }

    [Serializable]
    public class SpriteVariable : CachedVariable<SpriteWithEvent, Sprite, SpriteEvent> {}
}