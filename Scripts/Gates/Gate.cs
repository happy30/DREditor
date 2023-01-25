using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace DREditor.Gates
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Gateway", menuName = "DREditor/Gate")]
    public class Gate : ScriptableObject
    {
        [Tooltip("Make this the Scene Name")]
        public string toAreaName;
        public List<InvGate> invGates = new List<InvGate>();
        public bool isLocked = false;
        public bool isTPFD = false;
        public List<GateFrom> fromLocationList = new List<GateFrom>(); // Thank you Leo For the reminder ;)

        public GateFrom GetFromLocation(string roomName)
        {
            foreach (GateFrom from in fromLocationList)
            {
                if (from.gateFrom.toAreaName == roomName)
                    return from;
                foreach (InvGate g in from.gateFrom.invGates)
                    if (g.sceneName == roomName)
                        return from;
            }
            UnityEngine.Debug.Log("GateFrom not found in Gate " + toAreaName);
            return null;
        }
        public GateFrom GetFromLocation(Gate gate)
        {
            var x = fromLocationList.Where(n => n.gateFrom == gate);
            if (x.Count() > 0)
                return x.ElementAt(0);
            UnityEngine.Debug.Log("GateFrom not found in Gate");
            return null;
        }
        public string GetInvestigationRoom(int current)
        {
            foreach (InvGate invGate in invGates)
            {
                if (invGate.chapter == current)
                    return invGate.sceneName;
            }
            //UnityEngine.Debug.LogWarning("Investigation Room couldn't be found, returning to default area");
            return toAreaName;
        }

        [Serializable]
        public class InvGate
        {
            public int chapter;
            public string sceneName;
        }
    }
    [Serializable]
    public class GateFrom
    {
        public Gate gateFrom = null;
        public Vector3 position;
        public Vector3 rotation;
    }
}
