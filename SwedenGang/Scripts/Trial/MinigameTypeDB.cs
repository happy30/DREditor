//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
//using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(menuName = "DREditor/Trials/Minigame Database", fileName ="MinigameTypeDB")]
public class MinigameTypeDB : ScriptableObject
{
    public List<TrialMinigame> MinigameList = new List<TrialMinigame>();
#if UNITY_EDITOR
    private void OnValidate()
    {
        int count = 0;
        foreach(TrialMinigame minigame in MinigameList)
        {
            MinigameList[count].TypeNumber = count;
            count += 1;
        }
        EditorUtility.SetDirty(this);
    }
#endif
    public string[] ToStringArray()
    {
        string[] sa = new string[MinigameList.Count];
        for(int i = 0; i < MinigameList.Count; i++)
        {
            sa[i] = MinigameList[i].TypeName;
        }
        return sa;
    }
    public int[] ToIntArray()
    {
        int[] ia = new int[MinigameList.Count];
        for (int i = 0; i < MinigameList.Count; i++)
        {
            ia[i] = MinigameList[i].TypeNumber;
        }
        return ia;
    }
    [Serializable]
    public class TrialMinigame
    {
        public string TypeName;
        public int TypeNumber;
        public string ControlsKey;
        public GameObject Prefab;
        public UnityEvent<ScriptableObject> manager;
        
    }
}
