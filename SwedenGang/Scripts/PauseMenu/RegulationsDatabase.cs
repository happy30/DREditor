using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "DREditor/Regulations/RegulationsDB", fileName = "RegulationsDatabase")]
public class RegulationsDatabase : ScriptableObject { // Code by Willy Bee

    private List<Regulation> allRegulations;

    public void loadRegulations() { //called by the Master Database when it wakes up
        Object[] resources = Resources.LoadAll("Regulations", typeof(Regulation));
        allRegulations = new List<Regulation>();
        foreach (Regulation k in resources)
            allRegulations.Add(k);
    }

    public Regulation GetRegulation(int num) {
        return allRegulations.Find(Regulation => Regulation.regulationNum == num);
    }

    public List<Regulation> GetRegulations() {
        return allRegulations;
    }

    public Regulation AddRegulation(Sprite img, Sprite desc) {
        Regulation newReg = new Regulation(allRegulations.Count, img, desc);
        allRegulations.Add(newReg);
        return newReg;
    }
}