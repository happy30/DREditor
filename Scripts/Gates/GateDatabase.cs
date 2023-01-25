using System.Collections;
using System.Collections.Generic;
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
    }
}

