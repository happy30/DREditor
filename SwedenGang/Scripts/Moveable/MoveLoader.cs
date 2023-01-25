//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class MoveLoader
{
    public static List<MoveData> Save()
    {
        List<MoveData> moveData = new List<MoveData>();

        var moveList = Object.FindObjectsOfType<Moveable>();

        foreach (Moveable move in moveList)
            moveData.Add(move.Save());

        return moveData;
    }
    public static void Load(List<MoveData> loadedData)
    {
        var moveList = Object.FindObjectsOfType<Moveable>();
        for (int i = 0; i < loadedData.Count; i++)
            for (int x = 0; x < moveList.Count(); x++)
                if (moveList[x].gameObject.name == loadedData[i].name)
                    moveList[x].Load(loadedData[i]);
    }
}
