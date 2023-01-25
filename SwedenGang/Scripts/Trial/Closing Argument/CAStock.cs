using System.Collections.Generic;
using UnityEngine;

public class CAStock : MonoBehaviour //just exists to store data
{
    public List<CAStock> nodeDirections;
    public string flavourText;
    public int remainingLocks;
    public bool selectable = true;
    public Sprite sprite;
    public Vector2 activePanelScale;
    public Vector2 activePanelPos;

}
