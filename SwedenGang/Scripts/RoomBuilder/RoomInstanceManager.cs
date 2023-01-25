//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using DREditor.Progression;
using DREditor.Camera;

public class RoomInstanceManager : MonoBehaviour
{
    public static RoomInstanceManager instance = null;
    public InstanceData data = null;
    GameObject mainCam;
    GameObject dialogueCam;
    GameObject blurCam;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        mainCam = GameObject.Find("Main Camera");
        dialogueCam = GameObject.Find("Dialogue Camera");
        blurCam = GameObject.Find("Blur Camera");
    }

    public void SaveRoom(string roomName)
    {
        if (data.CurrentObjective.Description == "")
        {
            data = new InstanceData();
            data.Chapter = ProgressionManager.instance.GetChapter();
            data.CurrentObjective = ProgressionManager.instance.CurrentObjective;
            data.DialoguePosition = dialogueCam.transform.position;
            data.DialogueRotation = dialogueCam.transform.eulerAngles;
            Debug.Log("Current Objective is: " + data.CurrentObjective.Description);
        }
        Debug.Log("Has Base for " + roomName + ": " + RoomManager.instance.HasBase(data.CurrentObjective));
        if (RoomManager.instance.HasBase(data.CurrentObjective))
        {
            if (HasRoom(roomName)) // Room already exists in the instance Data then replace it
            {
                data.Rooms[GetRoom(roomName)].Data = RoomManager.instance.SaveRoomInstance(data.CurrentObjective);
                //Debug.Log("Room Instance Data Saved Has room");
            }
            else
            {
                Room room = new Room();
                room.Name = roomName;
                room.Data = RoomManager.instance.SaveRoomInstance(data.CurrentObjective);
                data.Rooms.Add(room);
                Debug.Log("Room Instance Data Saved");
            }
        }
        
        /*
         * Make new Room
         * Get the current room we're in and save the name
         * Call Room Manager to get the current room data
         * Add it to the instance data
         */
    }
    public void LoadRoom(string roomName)
    {
        if (!GameSaver.LoadingFile && data.CurrentObjective.Description == "" || 
            !GameSaver.LoadingFile && data.CurrentObjective != ProgressionManager.instance.CurrentObjective)
        {
            data = new InstanceData();
            data.Chapter = ProgressionManager.instance.GetChapter();
            data.CurrentObjective = ProgressionManager.instance.CurrentObjective;
            Debug.Log("Current Objective is: " + data.CurrentObjective.Description);
        }
        //Debug.Log(GameSaver.CurrentGameData.RoomData.CurrentRoom.Name);
        if (GameSaver.CurrentGameData != null && GameSaver.CurrentGameData.RoomData.CurrentRoom.Name == roomName
            && GameSaver.LoadingFile) // if Loading save file
        {
            Debug.Log("CURRENT ROOM DATA WAS USED");

            ProgressionManager.instance.Load();
            data.CurrentObjective = ProgressionManager.instance.CurrentObjective;
            Debug.Log("Load File Objective is: " + data.CurrentObjective.Description);

            mainCam.transform.position = GameSaver.CurrentGameData.RoomData.MainPosition;
            mainCam.transform.eulerAngles = GameSaver.CurrentGameData.RoomData.MainRotation;
            dialogueCam.transform.position = GameSaver.CurrentGameData.RoomData.DialoguePosition;
            dialogueCam.transform.eulerAngles = GameSaver.CurrentGameData.RoomData.DialogueRotation;
            //blurCam.transform.position = GameSaver.CurrentGameData.RoomData.BlurPosition;
            //blurCam.transform.eulerAngles = GameSaver.CurrentGameData.RoomData.BlurRotation;

            //data.Rooms.Add(GameSaver.CurrentGameData.RoomData.CurrentRoom);
            RoomManager.instance.LoadRoomInstance(GameSaver.CurrentGameData.RoomData.CurrentObjective, data.CurrentRoom.Data);
            return;
        }
        if (HasRoom(roomName) && data != null)
        {
            Debug.Log(GetRoom(roomName));
            RoomManager.instance.LoadRoomInstance(data.CurrentObjective, data.Rooms[GetRoom(roomName)].Data);
        }
        else
        {
            RoomManager.instance.LoadRoomInstance(data.CurrentObjective, null);
        }
    }
    int GetRoom(string roomName)
    {
        for (int i = 0; i < data.Rooms.Count; i++)
        {
            if (data.Rooms[i].Name == roomName)
                return i;
        }
        
        return -1; // Shouldn't reach here
    }
    public bool HasRoom(string roomName)
    {
        for(int i = 0; i < data.Rooms.Count; i++)
        {
            if (data.Rooms[i].Name == roomName)
                return true;
        }
        return false;
    }
    public string SerializeRoomData() => JsonUtility.ToJson(data);
    public void ClearRoomData()
    {
        Debug.Log("Cleared Room Data");
        data.Rooms.Clear();
    }
    public void ClearData() => data = new InstanceData();
    public InstanceData SaveRoomProgress()
    {
        data.CurrentRoom = new Room();
        //if (data.CurrentObjective.Description == "")
        //data.CurrentObjective = ProgressionManager.instance.CurrentObjective;
        data.CurrentObjective = ProgressionManager.instance.CurrentObjective;
        data.CurrentRoom.Name = SceneManager.GetActiveScene().name;
        try
        {
            if (GameManager.instance.currentMode != GameManager.Mode.Trial)
                data.CurrentRoom.Data = RoomManager.instance.SaveRoomInstance(data.CurrentObjective);
        }
        catch (Exception e)
        {
            Debug.LogWarning("NOTIFY: The Room Instance data was not saved, this is usually due to " +
                "a RoomManager Instance not being available. \n Error: " + e.ToString());
        }
        Debug.Log(RoomManager.instance != null);
        GameObject system = GameObject.Find("Player");
        
        data.Position = system.transform.position;
        data.Rotation = system.transform.eulerAngles;
        SmoothMouseLook cam = GameObject.Find("Main Camera").GetComponent<SmoothMouseLook>();
        data.MouseAbs = cam.GetAbsolute();
        data.MainPosition = mainCam.transform.position;
        data.MainRotation = mainCam.transform.eulerAngles;

        data.DialoguePosition = dialogueCam.transform.position;
        data.DialogueRotation = dialogueCam.transform.eulerAngles;

        data.BlurPosition = blurCam.transform.position;
        data.BlurRotation = blurCam.transform.eulerAngles;

        Debug.Log("Current Room Data Saved");
        Debug.Log("Objective is: " + data.CurrentObjective.Description);
        return data;
    }

    [Serializable]
    public class InstanceData
    {
        public Chapter Chapter;
        public Objective CurrentObjective;
        public List<Room> Rooms = new List<Room>();
        public Room CurrentRoom;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 MainPosition;
        public Vector3 MainRotation;
        public Vector3 DialoguePosition;
        public Vector3 DialogueRotation;
        public Vector3 BlurPosition;
        public Vector3 BlurRotation;
        public Vector2 MouseAbs;

    }

    [Serializable]
    public class Room
    {
        public string Name;
        public RoomData Data;
    }
}
