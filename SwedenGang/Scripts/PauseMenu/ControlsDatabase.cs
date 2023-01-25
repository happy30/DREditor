using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "P:EG/Controls Database", fileName = "ControlDatabase")]
public class ControlsDatabase : ScriptableObject
{
    [SerializeField] List<ControlPair> controls = new List<ControlPair>();

    public ControlsUIPanel GetPanel(string name)
    {
        if(controls.Where(n => n.name == name).Count() == 0) return null;
        return controls.Where(n => n.name == name).ElementAt(0).panel;
    }



    [System.Serializable]
    public class ControlPair
    {
        public string name;
        public ControlsUIPanel panel;
    }
}
