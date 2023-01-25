//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SubAreaLoader
{
    public static List<SubAreaData> Save()
    {
        List<SubAreaData> areaData = new List<SubAreaData>();

        var areaList = UnityEngine.Object.FindObjectsOfType<SubAreaManager>();

        foreach (SubAreaManager manager in areaList)
            areaData.Add(manager.Save());

        return areaData;
    }
    
    public static void Load(List<SubAreaData> baseData, List<SubAreaData> loadedData = null)
    {
        if (loadedData != null) 
        {
            //bool done = false;
            for (int z = 0; z < baseData.Count; z++)
            {
                try
                {
                    SubAreaData subBase = baseData[z];
                    SubAreaData subIns = loadedData[z];
                    if (subBase.codeName != subIns.codeName)
                    {
                        Debug.Log("CALLED REVERSE FOR SUBAREAS");
                        loadedData.Reverse();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("MINOR ERROR: A BASE OR INSTANCE PIECE OF SUB AREA DATA COULDN'T BE FOUND \n" +
                        ex.ToString());
                }

            }
            Debug.Log("INSTANCE SUBAREAS is: " + loadedData.Count);
        }
        var areaList = UnityEngine.Object.FindObjectsOfType<SubAreaManager>();
        if(loadedData != null)
            for (int i = 0; i < baseData.Count; i++)
                areaList[i].Load(SubAreaData.MergeInstance(baseData[i], loadedData[i]));
        else
            for (int i = 0; i < baseData.Count; i++)
                areaList[i].Load(baseData[i]);
    }
}
