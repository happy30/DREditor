using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DREditor.Gates
{
    /// <summary>
    /// Requires: Dialogues, Gateway
    /// Database to keep track of what areas have doors to other areas
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "GateDatabase", menuName = "DREditor/GateDatabase")]
    public class GateDatabase : ScriptableObject
    {
        public Gate ProgressionGate = null; // For when you need to only go to one area
        public List<Gate> Areas = new List<Gate>();
        public Gate GetArea(string scene)
        {
            foreach (Gate a in Areas)
            {
                if (a.toAreaName == scene)
                    return a;
            }
            UnityEngine.Debug.LogWarning("Gateway could not be found for " + scene + 
                "! Make sure that the name spelling is correct and that the scene" +
                " has been referenced in the unity builder! ");
            return null;
        }
        public bool Contains(string scene)
        {
            foreach (Gate a in Areas)
            {
                if (a != null && a.toAreaName == scene)
                    return true;
            }
            return false;
        }
        public string[] GetGateNames()
        {
            List<string> names = new List<string>();
            foreach(Gate g in Areas)
            {
                names.Add(g.toAreaName);
            }
            return names.ToArray();
        }
        public bool IsInvGate(Gate g)
        {
            foreach(Gate gate in Areas)
            {
                if(gate.invGates.Where(n => n.sceneName == g.toAreaName).ToArray().Length > 0)
                    return true;
            }
            return false;
        }
        public List<GateFrom> GetParentLocationList(Gate g)
        {
            foreach(Gate gate in Areas)
            {
                if (gate.invGates.Where(n => n.sceneName == g.toAreaName).ToArray().Length > 0)
                    return gate.fromLocationList;
            }
            return null;
        }
    }
}

