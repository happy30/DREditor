using DREditor.Characters;
using DREditor.Gates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GateUtility
{
    #region Spawn Gate
    public static string targetDirectory = "Resources/Gates";
    /// <summary>
    /// A gate is made of the current scene and adds it to an existing database
    /// </summary>
    [MenuItem("Tools/DREditor/Gates/Auto Setup Gate")]
    public static void AutoSetUpGate()
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            string gateName = SceneManager.GetActiveScene().name;
            var db = SetAssetByType<GateDatabase>();

            if(db != null && db.Contains(gateName))
            {
                Debug.LogWarning("The Gate " + gateName + " already exists in the current Gate Database!");
                return;
            }
            Gate gate = ScriptableObject.CreateInstance<Gate>();
            

            gate.toAreaName = gateName;

            string folder = CreateIntermediateFolders(targetDirectory);

            

            string gatePath = folder + "/G_" + gateName.Replace("GYM_", "") + ".asset";
            AssetDatabase.CreateAsset(gate, gatePath);



            
            if (db == null)
            {
                Debug.LogWarning("Couldn't find Gate Database, spawning new one");
                //string folder = CreateIntermediateFolders(targetDirectory);
                GateDatabase d = ScriptableObject.CreateInstance<GateDatabase>();

                string stuPath = folder + "/GateDatabase.asset";
                AssetDatabase.CreateAsset(d, stuPath);
                db = d;
            }

            db.Areas.Add(gate);
            EditorUtility.SetDirty(db);

            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            Debug.Log("Spawn Gate Complete");
            Selection.activeObject = gate;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            Debug.LogWarning("Gate Spawning Failed.");
        }
    }
    private static string CreateIntermediateFolders(string folder)
    {
        // Normalize path
        string folderPath = folder.Replace('\\', '/');
        string[] parts = folderPath.Split('/');
        string tempPath = "Assets";
        foreach (string p in parts)
        {
            if (p.Length == 0)
            {
                continue;
            }
            if (!Directory.Exists(tempPath + "/" + p))
            {
                AssetDatabase.CreateFolder(tempPath, p);
            }
            tempPath += "/" + p;
        }

        return tempPath;
    }
    static T SetAssetByType<T>(string s = "") where T : UnityEngine.Object
    {
        if (FindAssetsByType<T>(s) is List<T> databases && databases.Count > 0)
        {
            return databases[0];
        }
        return null;
    }
    public static List<T> FindAssetsByType<T>(string s = "") where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).Name) + " " + s);
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }
    #endregion
}
