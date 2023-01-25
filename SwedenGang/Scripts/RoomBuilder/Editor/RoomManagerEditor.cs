//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Characters;
using DREditor.Utility.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DREditor.Progression;
using System.Linq;

[CustomEditor(typeof(RoomManager))]
[CanEditMultipleObjects]
public class RoomManagerEditor : Editor
{
    RoomManager manager;
    RoomBuilder builder;
    ProgressionDatabase pDB;
    CharacterDatabase cDB;
    private void OnEnable()
    {
        manager = (RoomManager)target;
        try
        {
            pDB = (ProgressionDatabase)Resources.Load("Progression/ProgressionDatabase");
        }
        catch
        {
            Debug.LogError("Progression Asset is missing or in the wrong place! " +
                "Make sure it's in: Resources/Progression and named ProgressionDB");
        }
        try
        {
            cDB = (CharacterDatabase)Resources.Load("Characters/CharacterDatabase");
        }
        catch
        {
            Debug.LogError("Progression Asset is missing or in the wrong place! " +
                "Make sure it's in: Resources/Progression and named ProgressionDB");
        }
    }
    public override void OnInspectorGUI()
    {

        serializedObject.Update();
        CreateForm();
        if (manager.Builder)
        {
            EditorUtility.SetDirty(builder);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    private void CreateForm()
    {

        using (new EditorGUILayout.HorizontalScope())
        {
            manager.Builder = HandyFields.UnityField(manager.Builder, 200, 35);
            if (manager.Builder != null && builder == null)
            {
                builder = manager.Builder;

            }
            if (GUILayout.Button("Apply Changes"))
            {
                PrefabReferences();
                EditorUtility.SetDirty(manager);
            }
        }
        if (manager.ActorPrefabs.Count == 0)
            PrefabReferences();
        //PrefabReferences();
        manager.intChapter = HandyFields.IntField("Chapter: ", manager.intChapter, 30);
        if(manager.intChapter < 0 || manager.intChapter >= pDB.Chapters.Count)
        {
            GUILayout.Label("Please put a valid chapter number");
            return;
        }
        else
            manager.Chapter = pDB.Chapters[manager.intChapter];

        if (manager.Builder)
        {
            CreateRoomSave();
            ClearDropDown();
            LoadRoomSave();
        }
    }

    #region Prefab References
    private void PrefabReferences()
    {
        if (manager.ActorPrefabs.Count < cDB.Characters.Count-2)
        {
            List<GameObject> p = new List<GameObject>();
            /*for(int i = 0; i < cDB.Characters.Count; i++)
            {
                Character c = cDB.Characters[i];

            }*/
            foreach(Character c in cDB.Characters)
            {
                if (c.ActorPrefab != null)
                {
                    Debug.Log("Added " + c.ActorPrefab.name);
                    p.Add(c.ActorPrefab);
                }
            }
            manager.ActorPrefabs = p;
        }
    }
    #endregion

    #region Create Room Save
    int intObjective = 0;
    private void CreateRoomSave()
    {
        using (new EditorGUILayout.HorizontalScope())
        {

            intObjective = EditorGUILayout.Popup(intObjective, manager.Chapter.GetObjectives());

            if (GUILayout.Button("Create Room Save"))
            {
                manager.SaveRoom(pDB.Chapters[manager.intChapter].Objectives[intObjective]);
            }
            
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Shell", GUILayout.Width(100));
            if (builder.Shell.Count != 0)
            {
                if (GUILayout.Button("Load Shell")) // Bug where actors properties were swapped when loaded 
                {
                    if (FindObjectsOfType(typeof(Actor)).Length > 0)
                    {
                        manager.UnLoadActors(true);
                    }
                    manager.LoadRoom(builder.Shell[0]);
                }
                if (GUILayout.Button("Overwrite Shell"))
                {
                    if (EditorUtility.DisplayDialog("Overwrite Save", "Are you sure you want to overwrite the shell room data?",
                    "Yes", "No"))
                    {
                        builder.Shell[0] = manager.OverwriteRoom(builder.Shell[0]);
                    }
                }
                if (GUILayout.Button("Delete Shell"))
                {
                    if (EditorUtility.DisplayDialog("Clear Shell", "Are you sure you want to Clear this rooms Shell?",
                    "Yes", "No"))
                    {
                        builder.Shell.Clear();
                        EditorUtility.SetDirty(builder);
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Create Shell Save", GUILayout.Width(175)))
                {
                    builder.Shell.Add(manager.SaveShellRoom());
                }
            }
            
        }
    }
    #endregion

    #region Clear Drop Down
    bool clearFold = false;
    void ClearDropDown()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            clearFold = EditorGUILayout.Foldout(clearFold, "Clear Objects");
            if (GUILayout.Button("Clear All", GUILayout.Width(150)))
                if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to clear all Saveable Objects?",
                    "Yes", "No"))
                    ClearAll();

            
        }
        

        if (clearFold)
        {
            if (GUILayout.Button("Unload Spawnables", GUILayout.Width(150)))
                if (EditorUtility.DisplayDialog("Unload Spawnables", "Are you sure you want to destroy all Spawnables?",
                    "Yes", "No"))
                {
                    RoomManager.UnloadSpawnables(true);
                    EditorUtility.SetDirty(manager);
                }

            if (GUILayout.Button("Unload Actors", GUILayout.Width(150)))
                if (EditorUtility.DisplayDialog("Unload Actors", "Warning! All REFERENCED objects with the Actor component will be destroyed. Are you sure?",
                    "Yes", "No"))
                {
                    manager.UnLoadActors(true);
                    EditorUtility.SetDirty(manager);
                }

            if (GUILayout.Button("Unload Actuators", GUILayout.Width(150)))
                if (EditorUtility.DisplayDialog("Clear Actuators", "Are you sure you want to clear all Actuators?",
                    "Yes", "No"))
                {
                    RoomManager.UnloadEvents();
                    EditorUtility.SetDirty(manager);
                }

            if (GUILayout.Button("Clear Door Locks", GUILayout.Width(150)))
                if (EditorUtility.DisplayDialog("Clear Door Locks", "Are you sure you want to clear Door Locks?",
                    "Yes", "No"))
                {
                    RoomManager.UnloadDoors();
                    EditorUtility.SetDirty(manager);
                }

            if (GUILayout.Button("Clear Items", GUILayout.Width(150)))
                if (EditorUtility.DisplayDialog("Clear Items", "Are you sure you want to clear all Item Data?",
                    "Yes", "No"))
                {
                    RoomManager.UnloadItems();
                    EditorUtility.SetDirty(manager);
                }
        }
    }

    void ClearAll()
    {
        RoomManager.UnloadSpawnables(true);
        manager.UnLoadActors(true);
        RoomManager.UnloadItems();
        RoomManager.UnloadEvents();
        RoomManager.UnloadDoors();
        EditorUtility.SetDirty(manager);
    }
    #endregion

    #region Load Room Save

    bool roomFold = true;
    private void LoadRoomSave()
    {
        RoomSection current = builder.Sections[manager.intChapter];

        if (current.Rooms.Count > 0)
        {
            string foldTitle = "Saved Rooms";
            roomFold = EditorGUILayout.Foldout(roomFold, foldTitle);
            if (roomFold)
            {
                GUILayout.Label("On Load will remove REFERENCED Actors");
                for (int i = 0; i < current.Rooms.Count; i++)
                {
                    RoomData room = current.Rooms[i];
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(room.progressionValue.Description, GUILayout.Width(100));
                        if (GUILayout.Button("Load Room")) // Bug where actors properties were swapped when loaded 
                        {
                            if (FindObjectsOfType(typeof(Actor)).Length > 0)
                            {
                                manager.UnLoadActors(true);
                            }
                            manager.LoadRoom(room);
                        }
                        if (GUILayout.Button("Overwrite Room"))
                        {
                            if (EditorUtility.DisplayDialog("Overwrite Save", "Are you sure you want to overwrite the room data?",
                            "Yes", "No"))
                            {
                                current.Rooms[i] = manager.OverwriteRoom(room);
                            }
                        }
                        if (GUILayout.Button("Delete Room"))
                        {
                            if (EditorUtility.DisplayDialog("Delete Save", "Are you sure you want to delete the room data?",
                            "Yes", "No"))
                            {
                                current.Rooms.Remove(room);
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion
}
