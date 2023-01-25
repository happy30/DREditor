//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DREditor.Dialogues;
using DREditor.Progression;
/// <summary>
/// Serialize Actor data in the scene to be loaded later at runtime based on a progression value
/// </summary>
[Serializable][CreateAssetMenu(fileName = "Room", menuName = "RoomBuilder")]
public class RoomBuilder : ScriptableObject
{
    public List<RoomData> Shell = new List<RoomData>();
    public List<RoomSection> Sections = new List<RoomSection>();
    private void OnEnable()
    {
        try
        {
            ProgressionDatabase pDB = Resources.Load<ProgressionDatabase>("Progression/ProgressionDatabase");
            if (Sections.Count < pDB.Chapters.Count)
            {
                /* Intended to add a new section when a new chapter was added, to be done later I guess
                for(int i = 0; i < (pDB.Chapters.Count - Sections.Count); i++)
                {
                    Debug.LogWarning("An Additional Section was added to match the current # of chapters");
                    //Debug.LogWarning("Chapters Count: " + pDB.Chapters.Count);
                    //Debug.LogWarning("Sections Count: " + Sections.Count);
                    Sections.Add(new RoomSection(pDB.Chapters[i]));
                }
                */
            }
            
        }
        catch
        {
            Debug.LogError("Progression Asset is missing or in the wrong place! " +
                "Make sure it's in: Resources/Progression and named ProgressionDatabase");
        }
    }

    
}
[Serializable]
public class RoomSection
{
    public Chapter Chapter;
    public List<RoomData> Rooms = new List<RoomData>();
    public RoomSection(Chapter c)
    {
        Chapter = c;
    }
    public bool HasDataFor(Objective pValue)
    {
        foreach (RoomData r in Rooms)
        {
            if (pValue.Description == r.progressionValue.Description)
            {
                return true;
            }
        }

        return false;
    }
}
[Serializable]
public class RoomData
{
    public Objective progressionValue;
    public List<GameObject> spawnList = new List<GameObject>();
    public List<ActorData> actorList = new List<ActorData>();
    public List<ItemData> itemList = new List<ItemData>();
    public List<EventTrigger> triggers = new List<EventTrigger>();
    public List<SubAreaData> subAreas = new List<SubAreaData>();
    public List<DoorData> doorDatas = new List<DoorData>();
    public List<MoveData> moveDatas = new List<MoveData>();
}
/* - List the Objects you want to save in the RoomData class
 * - Below is usually done with a static Class to call Save() and Load()
 * - Go to RoomManager.cs and fill out the save process in the NewSaveRoom function
 * - Go to RoomManager.cs and fill out the load process in the LoadRoom function
 * - Go to RoomManager.cs and fill out the load process in the LoadRoomInstance function
 * - Go to RoomManager.cs and fill out the overwrite process in the Overwrite function
 * - Make sure you make the Save and Load functions on the class you're saving and loading to
 */
