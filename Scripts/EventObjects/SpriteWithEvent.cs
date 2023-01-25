using System;
using UnityEngine;

namespace DREditor.EventObjects
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