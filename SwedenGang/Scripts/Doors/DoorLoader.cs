//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Gardens
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class DoorLoader
{
    public static List<DoorData> Save()
    {
        List<DoorData> doorData = new List<DoorData>();

        var doorList = Object.FindObjectsOfType<Door>();

        foreach (Door door in doorList)
            doorData.Add(door.Save());

        return doorData;
    }
    public static void Load(List<DoorData> loadedData, List<DoorData> instanceData = null)
    {
        var doorList = Object.FindObjectsOfType<Door>();

        for (int i = 0; i < loadedData.Count; i++)
            for (int x = 0; x < doorList.Count(); x++)
            {
                if (doorList[x].gameObject.name == loadedData[i].doorName)// Door has been found for base
                {
                    DoorData insDoor = GetInstanceData(doorList[x].gameObject.name, instanceData);
                    if (insDoor != null)
                        doorList[x].Load(DoorData.MergeInstance(loadedData[i], insDoor));
                    else
                        doorList[x].Load((DoorData)loadedData[i].Clone()); // if instance data wasn't found then load base data
                }
            }
    }
    static DoorData GetInstanceData(string doorName, List<DoorData> ins)
    {
        if (ins != null)
        {
            var x = ins.Where(n => n.doorName == doorName);
            if (x.Count() > 0)
                return x.ElementAt(0);
        }
        return null;
    }
}
