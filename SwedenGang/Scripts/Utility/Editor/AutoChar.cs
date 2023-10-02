using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using DREditor.Characters;
using DREditor.Dialogues;
using System;
using System.IO;
using DREditor.Gates;
/// <summary>
/// This Tool is intended to take in the bare minimum assets and
/// set up a character for use in dialogue gameplay.
/// Perfect for testing out the engine.
/// </summary>
public class AutoChar : ScriptableWizard
{
    [SerializeField] string firstName = string.Empty;
    [SerializeField] string lastName = string.Empty;
    
    [SerializeField] Sprite neutralSprite = null;
    [SerializeField] Texture2D nameplate = null;

    [Header("Basic Settings")]
    public string targetDirectory = "Resources/Characters";
    [SerializeField] CharacterDatabase charDatabase = null;
    public Shader shader;
    public Shader blackShader;
    [SerializeField] GameObject actorPrefab = null;
    Material blackMat = null;
    //[SerializeField] List<Sprite> sprites = new List<Sprite>();
    //[Header("For Debug Visibility")]
    //[SerializeField] List<Material> materials = new List<Material>();

    [MenuItem("Tools/DREditor/Characters/Auto Setup Character")]
    public static void ShowExample()
    {
        ScriptableWizard.DisplayWizard<AutoChar>("Auto Char", "Create");
    }
    private void OnEnable()
    {
        shader = Shader.Find("Universal Render Pipeline/Lit");
        //shader = Shader.Find("Shader Graphs/BlackCutOut");
        blackShader = Shader.Find("Shader Graphs/BlackCutOut");
        actorPrefab = SetAssetByType<GameObject>("Actor");
        nameplate = SetAssetByType<Texture2D>("Transparent Image");
        charDatabase = SetAssetByType<CharacterDatabase>();
    }
    private void OnWizardCreate()
    {
        Execute();
    }
    T SetAssetByType<T>(string s = "") where T : UnityEngine.Object
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
    public void Execute()
    {
        try
        {
            AssetDatabase.StartAssetEditing();

            Student stu = CreateInstance<Student>();
            if (firstName != "")
            {
                SpawnStudentAsset(stu);
                FillOutStudentData(stu);
                try
                {
                    if (charDatabase == null && SetAssetByType<CharacterDatabase>() == null)
                    {
                        string folder = CreateIntermediateFolders(targetDirectory);
                        CharacterDatabase d = CreateInstance<CharacterDatabase>();
                        AssetDatabase.StartAssetEditing();

                        string stuPath = folder + "/CharacterDatabase.asset";
                        AssetDatabase.CreateAsset(d, stuPath);
                        AssetDatabase.SaveAssets();
                        charDatabase = d;
                    }

                    charDatabase.Characters.Add(stu);
                    EditorUtility.SetDirty(charDatabase);
                }
                catch
                {
                    Debug.LogWarning("Couldn't add Character to Database, If you make one this " +
                        "issue will resolve.");
                }
            }
            

            var mats = CreateMats(shader);

            Material neutralMat = null;
            if (neutralSprite != null)
            {
                int i = 0;
                foreach (var x in mats)
                {
                    if(x.name == neutralSprite.name)
                    {
                        Debug.Log("FOUND NEUTRAL SPRITE");
                        i = mats.IndexOf(x);
                    }
                }
                neutralMat = mats[i];
                mats.Remove(neutralMat);
                mats.Insert(0, neutralMat);
                
                blackMat = CreateMats(blackShader, new List<Texture>() { mats[0].mainTexture })[0];

                if (actorPrefab != null)
                {
                    GameObject g = PrefabUtility.InstantiatePrefab(actorPrefab) as GameObject;
                    Actor a = g.GetComponentInChildren<Actor>();
                    if(a != null)
                    {
                        
                        g.name = firstName;
                        a.character.material = mats[0];
                        a.characterb.material = blackMat;
                        a.sprite.sprite = neutralSprite;
                        a.spriteb.sprite = neutralSprite;
                        a.sprite.transform.position = new Vector3(0, 1.5f, 0);
                        EditorUtility.SetDirty(a);
                        EditorUtility.SetDirty(g);
                    }
                }
            }
            

            if (firstName != "")
            {
                ImportMaterials(stu, mats);
                ImportSprites(stu);
            }

            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            Selection.activeObject = stu;
        }
        catch
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }
        Debug.Log("Complete");
    }

    void FillOutStudentData(Student stu)
    {
        AssetDatabase.StartAssetEditing();
        stu.FirstName = firstName;
        stu.LastName = lastName;
        stu.Nameplate = nameplate;
        
        AssetDatabase.StopAssetEditing();
        AssetDatabase.SaveAssets();
    }
    void SpawnStudentAsset(Student stu)
    {



        string folder = CreateIntermediateFolders(targetDirectory);
        
        AssetDatabase.StartAssetEditing();
        
        string stuPath = folder + "/" + firstName + ".asset";
        AssetDatabase.CreateAsset(stu, stuPath);
        AssetDatabase.StopAssetEditing();
        AssetDatabase.SaveAssets();
    }
    private string CreateIntermediateFolders(string folder)
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
    /// <summary>
    /// Original Functions in StudentEditor.cs
    /// If this Editor window works, please edit the above mentioned scripts two functions
    /// to public static and use a student parameter
    /// </summary>
    /// <param name="stu"></param>
    private void ImportMaterials(Student stu, List<Material> matList = null)
    {
        var materials = Selection.GetFiltered(typeof(Material), SelectionMode.Assets).Cast<Material>();
        var mats = materials.ToList();
        if(matList != null)
        {
            mats = matList;
        }
        /*if(materials.Count() >= 0)
        {
            Debug.LogWarning("Sweden has Disabled this function, Refer to him and " +
                "don't touch this code.");
            return;
        }*/
        Debug.Log("Mats has " + mats.Count);
        try
        {
            foreach (var mat in mats)
            {
                Expression e = new Expression
                {
                    Sprite = mat,
                    Name = mat.name
                };
                stu.Expressions.Add(e);
                stu.Sprites.Add(new Unlit() { Name = e.Name});//*
            }
        }
        finally
        {

        }
    }
    private void ImportSprites(Student stu, List<Sprite> spriteList = null)
    {
        
        var sprites = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets).Cast<Texture2D>();
        var spr = sprites.ToList();
        if(spriteList != null)
        {
            spr = spriteList.Cast<Texture2D>().ToList();
            Debug.Log("Used SPRITE LIST");
        }
        /*if (sprites.Count() >= 0)
        {
            Debug.LogWarning("Sweden has Disabled this function, Refer to him and " +
                "don't touch this code.");
            return;
        }*/
        List<Sprite> spri = new List<Sprite>();
        Debug.Log("spr has " + spr.Count);
        try
        {
            for (int i = 0; i < spr.Count; i++)
            {
                spri.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(spr[i])));
            }

            for (int i = 0; i < stu.Sprites.Count; i++)
            {
                Debug.Log(spri[i].name);
                //stu.Sprites[i].Sprite = spri[i];
                for (int x = 0; x < spri.Count; x++)
                {
                    if (stu.Sprites[i].Name == spri[x].name)
                    {
                        stu.Sprites[i].Sprite = spri[x];
                        Debug.Log("Succsessfully Loaded " + spri[x].name);
                        break;
                    }
                }
            }
        }
        finally
        {

        }
        
    }

    public List<Material> CreateMats(Shader shader, List<Texture> spriteList = null)
    {
        List<Material> mats = new List<Material>();
        Debug.Log(shader.name);
        try
        {
            AssetDatabase.StartAssetEditing();
            var textures = Selection.GetFiltered(typeof(Texture), SelectionMode.Assets).Cast<Texture>();
            if(spriteList != null)
            {
                textures = spriteList;
            }
            foreach (var tex in textures)
            {
                string path = AssetDatabase.GetAssetPath(tex);
                path = path.Substring(0, path.LastIndexOf(".")) + ".mat";

                var mat = new Material(shader);
                if (shader.name == "Universal Render Pipeline/Lit")
                {
                    mat.SetFloat("_Smoothness", 0);
                    mat.SetFloat("_AlphaClip", 1);
                    mat.SetFloat("_Cutoff", 0.5f);
                    mat.EnableKeyword("_ALPHATEST_ON");
                }
                //"Shader Graphs/BlackCutOut"
                //"Universal Render Pipeline/Unlit"
                if (shader.name == "Shader Graphs/BlackCutOut")
                {
                    mat.SetColor("_Color", new Color(0,0,0));
                    path = path.Replace(tex.name, tex.name + "b");
                    mat.name += "b";
                }
                mat.mainTexture = tex;
                //mat.color = Color.white;
                
                if (AssetDatabase.LoadAssetAtPath(path, typeof(Material)) != null)
                {
                    Debug.LogWarning("Can't create material, it already exists: " + path);
                    continue;
                }
                AssetDatabase.CreateAsset(mat, path);
                mats.Add(mat);
            }
        }
        catch
        {
            Debug.LogWarning("Something went wrong creating materials");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }
        return mats;
    }
}
