//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using DREditor.Progression;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// Monobehavior to build rooms
/// NOTE: If you add a new character, you must click on every room manager so it's prefab references update
/// </summary>
public class RoomManager : MonoBehaviour
{
    public static RoomManager instance = null;
    public List<GameObject> ActorPrefabs = new List<GameObject>();
    public RoomBuilder Builder = null;
    public int intChapter;
    public Chapter Chapter;
    public static bool RoomLoaded = false;
    public static bool RoomSaved = false;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

    }

    public bool HasBase(Objective pValue) => Builder.Sections[intChapter].HasDataFor(pValue);
    public void SaveRoom(Objective pValue)
    {
        RoomData room = NewSaveRoom(pValue);

        Debug.Log(JsonUtility.ToJson(room));

        Builder.Sections[intChapter].Rooms.Add(room);
    }
    public RoomData SaveShellRoom()
    {
        return NewSaveRoom(null);
    }
    public RoomData SaveRoomInstance(Objective pValue)
    {
        RoomData room = NewSaveRoom(pValue);

        RoomSaved = true;
        Debug.Log(JsonUtility.ToJson(room));
        return room;
    }
    RoomData NewSaveRoom(Objective pValue)
    {
        RoomData room = new RoomData();

        room.progressionValue = pValue;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            room.spawnList = SaveSpawnables();
#endif
        room.actorList = SaveActors();
        room.itemList = SaveItems();
        room.triggers = TriggerLoader.Save();
        room.subAreas = SubAreaLoader.Save();
        room.doorDatas = DoorLoader.Save();
        room.moveDatas = MoveLoader.Save();
        return room;
    }
    public RoomData OverwriteRoom(RoomData room)
    {
#if UNITY_EDITOR
        room.spawnList = SaveSpawnables();
#endif
        room.actorList = SaveActors();
        room.itemList = SaveItems();
        room.triggers = TriggerLoader.Save();
        room.subAreas = SubAreaLoader.Save();
        room.doorDatas = DoorLoader.Save();
        room.moveDatas = MoveLoader.Save();
        //Builder.Sections[intChapter].Rooms.repl
        return room;
    }
    public void LoadRoom(RoomData room, RoomData instanceRoom = null)
    {
        if (room != null)
        {
            LoadSpawnables(room.spawnList);
            LoadActors(room, instanceRoom);
            LoadItems(room, instanceRoom);
            if (instanceRoom != null)
            {
                TriggerLoader.Load(room.triggers, instanceRoom.triggers);
                SubAreaLoader.Load(room.subAreas, instanceRoom.subAreas);
                DoorLoader.Load(room.doorDatas, instanceRoom.doorDatas);
            }
            else
            {
                TriggerLoader.Load(room.triggers);
                SubAreaLoader.Load(room.subAreas);
                DoorLoader.Load(room.doorDatas);
            }
            
            
            MoveLoader.Load(room.moveDatas);
        }

    }

    public void LoadRoomInstance(Objective pValue, RoomData data) // To Load Rooms in Game
    {
        RoomSection current = Builder.Sections[intChapter];
        RoomData baseRoom = null;
        for (int i = 0; i < current.Rooms.Count; i++)
        {
            RoomData room = current.Rooms[i];
            if (room.progressionValue.Description == pValue.Description)
            {
                baseRoom = room;
                break;
            }
        }
        if (baseRoom == null)
        {
            Debug.Log("Couldn't find Base Room data for Objective: " + pValue.Description +
                " , Checking for Room Shell.");
            if(Builder.Shell.Count != 0)
            {
                Debug.Log("Shell Data has been found! Loading Shell...");
                baseRoom = Builder.Shell[0];
            }
            else
                Debug.Log("No Shell Data Has been Filled, Loading Empty Shell");
        }

        if (data != null && baseRoom != null) // Has Instance Data
        {
            // Load Instance Room with data variable
            Debug.LogWarning("Room has Instance Data");
            LoadRoom(baseRoom, data);
            /*
            LoadActors(baseRoom, data);
            LoadItems(baseRoom, data);
            TriggerLoader.Load(baseRoom.triggers, data.triggers);
            SubAreaLoader.Load(baseRoom.subAreas, data.subAreas);
            DoorLoader.Load(baseRoom.doorDatas);
            MoveLoader.Load(baseRoom.moveDatas);
            */
        }
        else
            LoadRoom(baseRoom);

        RoomLoaded = true;

    }

    #region Actors
    List<ActorData> SaveActors()
    {

        List<ActorData> ActorListIn = new List<ActorData>();
        Actor[] x = (Actor[])FindObjectsOfType(typeof(Actor));
        if (x.Length == 0)
        {
            Debug.Log("There are no Actors in the scene!");
            return null;
        }
        for (int i = 0; i < x.Length; i++)
        {
            GameObject actor = x[i].gameObject.transform.parent.gameObject;
            for (int j = 0; j < ActorPrefabs.Count; j++) // Look through Actor references
            {
                if (ActorPrefabs[j] != null && ActorPrefabs[j].name == actor.name) // Actor Reference found Save actor data
                {
                    ActorData a = new ActorData(ActorPrefabs[j].name, actor, x[i]);
                    ActorListIn.Add((ActorData)a.Clone());
                }
            }

        }
        //Debug.Log(JsonHelper.ToJson(ActorListIn.ToArray()));
        return ActorListIn;
    }
    public static List<TActorData> SaveTrialActors()
    {
        List<TActorData> ActorListIn = new List<TActorData>();
        Actor[] x = (Actor[])FindObjectsOfType(typeof(Actor));
        if (x.Length == 0)
        {
            Debug.Log("There are no Actors in the scene!");
            return null;
        }
        for (int i = 0; i < x.Length; i++)
        {
            GameObject actor = x[i].gameObject.transform.parent.gameObject;
            MeshRenderer mesh = actor.GetComponentInChildren<MeshRenderer>();
            Character character = DialogueAssetReader.instance.FindChar(actor.name);
            TActorData a = new TActorData(actor.name, actor, x[i], character.GetSpriteLabelByTexName(mesh.material.mainTexture.name));
            ActorListIn.Add((TActorData)a.Clone());
        }
        //Debug.Log(JsonHelper.ToJson(ActorListIn.ToArray()));
        return ActorListIn;
    }
    public static void LoadTrialActors(List<TActorData> data)
    {
        Actor[] x = (Actor[])FindObjectsOfType(typeof(Actor));
        if (x.Length == 0)
        {
            Debug.Log("There are no Actors in the scene!");
            return;
        }
        for (int i = 0; i < x.Length; i++)
        {
            GameObject actor = x[i].gameObject.transform.parent.gameObject;
            for (int j = 0; j < data.Count; j++)
            {
                if (actor.name == data[j].aReference)
                {
                    LoadTrialActor(actor, x[i], data[j]);
                }
            }
        }
    }
    static void LoadTrialActor(GameObject parent, Actor a, TActorData data)
    {
        parent.transform.position = data.position;
        parent.transform.eulerAngles = data.rotation;
        MeshRenderer mesh = parent.GetComponentInChildren<MeshRenderer>();
        Character character = DialogueAssetReader.instance.FindChar(data.aReference);
        Texture t = character.GetSpriteByName(data.matName);
        mesh.material.mainTexture = t;
        a.characterb.material.mainTexture = t;
    }
    List<ItemData> SaveItems()
    {

        List<ItemData> itemList = new List<ItemData>();
        ItemActor[] x = (ItemActor[])FindObjectsOfType(typeof(ItemActor));
        if (x.Length == 0)
        {
            Debug.Log("There are no items in the scene! Just letting you know, not required or anything");
            return null;
        }
        for (int i = 0; i < x.Length; i++)
        {
            GameObject item = x[i].gameObject;
            if (item != null) 
            {
                ItemData a = new ItemData(item, x[i]);
                itemList.Add((ItemData)a.Clone());
            }

        }
        //Debug.Log(JsonHelper.ToJson(ActorListIn.ToArray()));
        return itemList;
    }
    public void LoadItems(RoomData room, RoomData instanceRoom = null)
    {
        List<ItemData> roomDataItems = room.itemList;
        
        
        ItemActor[] foundItems = (ItemActor[])FindObjectsOfType(typeof(ItemActor));

        if (foundItems.Length == 0)
        {
            Debug.Log("There are no items to be loaded in the scene!");
        }

        for (int i = 0; i < foundItems.Length; i++)
        {
            for(int j = 0; j < room.itemList.Count; j++)
            {
                if (foundItems[i].gameObject.name == roomDataItems[j].iReference)
                {
                    ItemActor item = foundItems[i];
                    ItemData n = (ItemData)roomDataItems[j].Clone();
                    item.RoomConvo = n.iData;
                    item.SetSelectable(n.iSelectable);
                    if (instanceRoom != null)
                    {
                        Debug.Log("FoundInstanceItem");
                        for(int l = 0; l < item.RoomConvo.Count; l++)
                        {
                            try
                            {
                                item.RoomConvo[l].triggered = instanceRoom.itemList[i].iData[l].triggered;
                            }
                            catch
                            {
                                Debug.LogWarning("COULDN'T FIND AN INSTANCE ROOMS DATA FOR WHETHER AN ITEM WAS TRIGGERED.");
                            }
                        }
                    }
                    break;
                }
            }

        }
    }
    public void LoadActors(RoomData room, RoomData instanceRoom = null)
    {
        //GameObject g = new GameObject();
        // when something is built itll take the object and convert it there
        if(room != null && room.actorList != null && room.actorList.Count != 0)
        {
            List<ActorData> actors = room.actorList;
            for (int i = 0; i < actors.Count; i++)
            {
                ActorData actor = (ActorData)actors[i].Clone();
                //Debug.Log("Actor prefab count: " + ActorPrefabs.Count);
                for (int j = 0; j < ActorPrefabs.Count; j++) // Look through Actor references
                {
                    //Debug.Log("Actor prefab: " + ActorPrefabs[j].name);
                    if (ActorPrefabs[j] != null && ActorPrefabs[j].name == actor.aReference) // Actor prefab matches Base room name
                    {
                        if (instanceRoom != null)
                        {
                            LoadActor(ActorPrefabs[j], actor, GetCorrectInst(instanceRoom.actorList, ActorPrefabs[j].name));
                            /*try
                            {
                                
                            }
                            catch
                            {
                                Debug.LogWarning("AN ERROR OCCURED LOADING INSTANCE DATA OF A CHARACTER");
                            }*/
                        }
                        else
                            LoadActor(ActorPrefabs[j], actor);

                        //Debug.Log("Actor Loaded");
                    }
                }

            }
        }
        
        Debug.Log("Finished Loading Actors");
    }
    ActorData GetCorrectInst(List<ActorData> instList, string nameLook)
    {
        for(int i = 0; i < instList.Count; i++)
        {
            if (instList[i].aReference == nameLook)
                return instList[i];
        }
        Debug.LogWarning("The instance actor data list couldn't find the character it was looking for? " + nameLook);
        return null;
    }
    void LoadActor(GameObject shell, ActorData actorData, ActorData instanceData = null)
    {
        GameObject loaded = Instantiate(shell);
        loaded.name = actorData.aReference;
        loaded.transform.position = actorData.position;
        loaded.transform.eulerAngles = actorData.rotation;
        Actor a = loaded.GetComponentInChildren<Actor>();
        a.RoomConvo = new List<LocalDialogue>();
        for(int i = 0; i < actorData.aData.Count; i++)
        {
            a.RoomConvo.Add((LocalDialogue)actorData.aData[i].Clone());
        }
        a.character.material = actorData.mat;
        Material m = new Material(a.characterb.sharedMaterial.shader);
        m.mainTexture = a.character.sharedMaterial.mainTexture;
        a.characterb.sharedMaterial = m;
        a.characterb.sharedMaterial.color = Color.black;
        a.characterb.sharedMaterial.renderQueue = 3000;
        a.identified = actorData.identified;

        // This is so the sprite is loaded correctly, but I need to add a way to reference
        // the character database, I'm too lazy rn lmao
        /*if (a.sprite.sprite.name != actorData.matName)
        {
            Character character = DialogueAssetReader.instance.FindChar(actorData.aReference);
            Debug.Log("MAT NAME WAS: " + actorData.matName);
            for (int i = 0; i < character.Expressions.Count; i++)
            {
                Expression exp = character.Expressions[i];

                if (actorData.matName.Contains(exp.Sprite.name))
                {
                    a.sprite.sprite = character.Sprites[i].Sprite;
                    break;
                }
            }
        }*/
        
        
        if (instanceData != null)
        {
            Character character = DialogueAssetReader.instance.FindChar(actorData.aReference);
            loaded.transform.position = instanceData.position;
            loaded.transform.eulerAngles = instanceData.rotation;
            a.identified = instanceData.identified;
            for (int i = 0; i < instanceData.aData.Count && i < actorData.aData.Count; i++)
            {
                Debug.Log(i);
                a.RoomConvo[i].triggered = instanceData.aData[i].triggered;
            }
            
            Debug.Log("MAT NAME WAS: " + instanceData.matName);
            for (int i = 0; i < character.Expressions.Count; i++)
            {
                Expression exp = character.Expressions[i];
                
                if (instanceData.matName.Contains(exp.Sprite.name))
                {
                    Debug.Log(exp.Sprite.name);
                    a.character.material = exp.Sprite;
                    Material ma = new Material(a.characterb.sharedMaterial.shader);
                    ma.mainTexture = a.character.sharedMaterial.mainTexture;
                    a.characterb.sharedMaterial = ma;
                    a.characterb.sharedMaterial.color = Color.black;
                    a.characterb.sharedMaterial.renderQueue = 3000;
                    a.sprite.sprite = character.Sprites[i].Sprite;
                    break;
                }
            }
        }
    }
    
    public void UnLoadActors(bool inEditor)
    {
        // Add pop up window that says "Are you sure you want to unload actors? this will remove ALL actors in the scene."
        // Destroy current Actor Objects in the scene

        Actor[] x = (Actor[])FindObjectsOfType(typeof(Actor));
        if (x.Length == 0)
        {
            Debug.Log("There are no Referenced Actors to Unload in the scene!");
        }
        for (int i = 0; i < x.Length; i++)
        {
            GameObject actor = x[i].gameObject.transform.parent.gameObject;
            for (int j = 0; j < ActorPrefabs.Count; j++) // Look through Actor references
            {
                if (ActorPrefabs[j] != null && ActorPrefabs[j].name == actor.name) // Found
                {
                    if (inEditor)
                        DestroyImmediate(actor);
                    else
                        Destroy(actor);
                    break;
                }
            }

        }
    }
    public static void UnloadEvents()
    {
        ActuatorBase[] x = (ActuatorBase[])FindObjectsOfType(typeof(ActuatorBase));
        foreach (ActuatorBase b in x)
#if UNITY_EDITOR
            DestroyImmediate(b.gameObject);
#else
            Destroy(b.gameObject);
#endif
    }
#endregion

    public List<GameObject> SaveSpawnables()
    {
        List<GameObject> objects = new List<GameObject>();
        // Find game object of type (Monobehavior to spawn)
        var spawns = FindObjectsOfType<Spawnable>();
#if UNITY_EDITOR
        foreach (Spawnable s in spawns)
        {
            var x = PrefabUtility.GetCorrespondingObjectFromSource(s.gameObject);
            if (x != null && !objects.Contains(x))
                objects.Add(x);
        }
        //PrefabUtility.
#endif
        return objects;
    }
    public void LoadSpawnables(List<GameObject> spawnables)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            foreach (GameObject g in spawnables)
            {
                if (g)
                {
                    GameObject i = Instantiate(g);
                    if (i.name.Contains("(Clone)"))
                        i.name = i.name.Replace("(Clone)", "");
                }
                else
                    Debug.LogWarning("There's a Missing Prefab Reference in this room save!");
            }
        }
        else
        {
            foreach (GameObject g in spawnables)
                if (g)
                    PrefabUtility.InstantiatePrefab(g);
                else
                    Debug.LogWarning("There's a Missing Prefab Reference in this room save!");
        }
#else
        foreach (GameObject g in spawnables)
        {
            if (g)
            {
                GameObject i = Instantiate(g);
                if (i.name.Contains("(Clone)"))
                    i.name = i.name.Replace("(Clone)", "");
            }
        }
#endif
    }
    public static void UnloadSpawnables(bool inEditor)
    {
        // Find game object of type (Monobehavior to spawn)
        // If in editor it should be destroy immediate otherwise destroy
        var spawns = FindObjectsOfType<Spawnable>();
        foreach(Spawnable s in spawns)
        {
            if (inEditor)
                DestroyImmediate(s.gameObject);
            else
                Destroy(s.gameObject);
        }
    }
    public static void UnloadDoors()
    {
        var doors = FindObjectsOfType<Door>();
        foreach (Door door in doors)
            door.ClearLockData();
    }
    public static void UnloadItems()
    {
        var items = FindObjectsOfType<ItemActor>();
        foreach (ItemActor i in items)
            i.ClearItemData();
    }
}
