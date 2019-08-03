using System;
using UnityEngine;
using UnityEngine.Events;

namespace EventObjects
{

    [Serializable]
    [CreateAssetMenu(menuName = "EventObjects/Transform", fileName = "New Transform")]
    public class TransformWithEvent : ValueWithEvent<PosRot, TransformEvent>, IConditional
    {
        public bool Resolve()
        {
            return true;
        }
    }


    [Serializable]
    public class PosRot
    {
        public Vector3 Pos;
        public Quaternion Rot;

        public PosRot(Vector3 pos, Quaternion rot)
        {
            Pos = pos;
            Rot = rot;
        }
    }
}