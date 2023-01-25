//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

public static class SaveSystem
{
    

    public static void SaveToJSON<T>(List<T> toSave, string filename)
    {
        Debug.Log(GetPath(filename));
        string content = JsonHelper.ToJson<T>(toSave.ToArray());
        WriteFile(GetPath(filename), content);
    }

    public static void SaveToJSON<T>(T toSave, string filename)
    {
        string content = JsonUtility.ToJson(toSave);
        WriteFile(GetPath(filename), content);
    }

    public static List<T> ReadListFromJSON<T>(string filename)
    {
        string content = ReadFile(GetPath(filename));

        if (string.IsNullOrEmpty(content) || content == "{}")
        {
            return new List<T>();
        }

        List<T> res = JsonHelper.FromJson<T>(content).ToList();

        return res;

    }

    public static T ReadFromJSON<T>(string filename)
    {
        string content = ReadFile(GetPath(filename));
        //Debug.Log(GetPath(filename));
        if (string.IsNullOrEmpty(content) || content == "{}")
        {
            return default(T);
        }

        T res = JsonUtility.FromJson<T>(content);

        return res;

    }
    
    public static bool CheckSave(string filename)
    {
        string path = GetPath(filename);
        
        if (File.Exists(path))
        {
            string content = ReadFile(GetPath(filename));
            //Debug.Log(GetPath(filename));
            if (string.IsNullOrEmpty(content) || content == "{}")
            {
                return false;
            }
            else
                return true;
        }

        return false;
    }
    class CryptHold
    {
        public string Crypt;
    }
    public static void SaveEncrypt<T>(T toSave, string fileName)
    {
        CryptHold c = new CryptHold();
        c.Crypt = EncryptionJSON.EncryptJSON(JsonUtility.ToJson(toSave), "CursiveW");
        
        SaveToJSON(c, fileName);
    }
    public static T LoadEncrypt<T>(string fileName)
    {
        CryptHold crypt = ReadFromJSON<CryptHold>(fileName);
        string d = EncryptionJSON.DecryptJSON(crypt.Crypt, "CursiveW");
        
        return (T)JsonUtility.FromJson(d, typeof(T));
    }
    private static string GetPath(string filename)
    {
        return Application.persistentDataPath + "/" + filename;
    }

    private static void WriteFile(string path, string content)
    {
        FileStream fileStream = new FileStream(path, FileMode.Create);

        using (StreamWriter writer = new StreamWriter(fileStream))
        {
            writer.Write(content);
        }
    }

    private static string ReadFile(string path)
    {
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string content = reader.ReadToEnd();
                return content;
            }
        }
        return "";
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
