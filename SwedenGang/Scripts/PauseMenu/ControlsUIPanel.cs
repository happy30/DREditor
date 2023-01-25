//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "P:EG/Control Panel", fileName = "new Control Panel")]
public class ControlsUIPanel : ScriptableObject
{
    public string PanelTitle;
    //public Sprite PanelTitle;
    public ControlPanelRow[] ControlRows;
    [TextArea(minLines:30, maxLines:30)]
    public string SystemDescription;
}
[System.Serializable]
public struct ControlPanelRow 
{
    public Sprite Left, Center, Right;
    public string Description;
}
