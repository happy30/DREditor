using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Tutorial_Asset", menuName = "DREditor/TrialTutorialAsset")]
[Serializable]
public class TrialTutorialAsset : ScriptableObject
{
    public Texture2D title = null;
    public List<Texture2D> pages = new List<Texture2D>();
}
