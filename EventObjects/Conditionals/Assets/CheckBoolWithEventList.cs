using System;
using System.Collections;
using System.Collections.Generic;
using EventObjects;
using UnityEngine;

namespace Conditionals
{
    
    public class CheckBoolWithEventList : ConditionalAsset<BoolWithEventList> { }

    [Serializable]
    public struct BoolWithEventList : IConditional
    {
        [Serializable]
        public struct BoolWithEventCondition : IConditional
        {
            public BoolWithEvent Condition;
            public bool Invert;

            public bool Resolve()
            {
                return Condition.Resolve() ^ Invert;
            }
        }
        
        public List<BoolWithEventCondition> Conditions;
        public bool Invert;
        
        public bool Resolve()
        {
            foreach (var condition in Conditions)
            {
                if (!condition.Resolve()) return Invert;
            }
            return !Invert;
        }
    }
}