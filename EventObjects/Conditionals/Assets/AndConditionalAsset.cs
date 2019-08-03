using System;
using System.Collections.Generic;
using UnityEngine;

namespace Conditionals.Operators
{
    
    public class AndConditionalAsset : ConditionalAsset<AndConditionalWithAsset>
    {
        
    }
    
    [Serializable]
    public class AndConditionalWithAsset : AndConditional<ConditionalAsset> { }
    
    
    public class AndConditional<T> : IConditional where T : IConditional
    {
        public bool Invert;
        public List<T> Conditions;
    
        public bool Resolve()
        {
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (!Conditions[i].Resolve()) return Invert;
            }
            return !Invert;
        }
    }
}
