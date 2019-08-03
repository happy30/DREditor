using System;
using UnityEngine;

namespace Conditionals
{
    public abstract class ConditionalAsset : ScriptableObject, IConditional
    {
        public abstract bool Resolve();
    }
    
    
    public abstract class ConditionalAsset<T> : ConditionalAsset, IConditional where T : IConditional
    {
        public T Conditional;
        
        public override bool Resolve()
        {
            return Conditional.Resolve();
        }
    }

    [Serializable]
    public class ConditionalAssetInput
    {
        public ConditionalAsset Input;
        public bool Invert;

        public bool Resolve()
        {
            if (Input == null) return !Invert;
            return Invert ^ Input.Resolve();
        }
    }

}
