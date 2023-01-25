using DREditor.TrialEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
[CreateAssetMenu(menuName = "DREditor/Trials/Evidence", fileName = "Evidence")]
public class Evidence : ScriptableObject
{
    public List<TruthBullet> TruthBullets = new List<TruthBullet>();
    public string[] toStringArray()
    {
        string[] arr = new string[TruthBullets.Count];

        for(int i = 0; i < TruthBullets.Count; i++)
        {
            arr[i] = TruthBullets[i].Title;
        }

        return arr;
    }
    public int[] tointArray()
    {
        int[] arr = new int[TruthBullets.Count];

        for (int i = 0; i < TruthBullets.Count; i++)
        {
            arr[i] = i;
        }

        return arr;
    }
}
