using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MasterDatabase : MonoBehaviour { // Code by Willy Bee

    private static MasterDatabase instance;
    public RegulationsDatabase regulations;

    private void Awake() { 
        if (instance == null) {
            instance = this;
            regulations.loadRegulations(); //load up all the existing regulations
            DontDestroyOnLoad(gameObject); //makes sure it doesn't get destroyed moving between scenes
        } else {
            Destroy(gameObject);
        }
    }

    public static Regulation GetRegulation(int num) {
        return instance.regulations.GetRegulation(num);
    }

    public static Regulation AddRegulation(Sprite img, Sprite desc) {
        return instance.regulations.AddRegulation(img, desc);
    }

    public static List<Regulation> GetRegulations() {
        return instance.regulations.GetRegulations();
    }
}