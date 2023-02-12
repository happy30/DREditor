﻿// CreateMaterialsForTextures.cs
// C#
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

namespace DREditor.Utility
{
    public class CreateMaterialsForTextures : ScriptableWizard
    {
        public Shader shader;

        [MenuItem("Tools/CreateMaterialsForTextures")]
        static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard<CreateMaterialsForTextures>("Create Materials", "Create");

        }

        void OnEnable()
        {
            //shader = Shader.Find("Diffuse");
        }

        void OnWizardCreate()
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                var textures = Selection.GetFiltered(typeof(Texture), SelectionMode.Assets).Cast<Texture>();
                foreach (var tex in textures)
                {
                    string path = AssetDatabase.GetAssetPath(tex);
                    path = path.Substring(0, path.LastIndexOf(".")) + ".mat";
                    if (AssetDatabase.LoadAssetAtPath(path, typeof(Material)) != null)
                    {
                        Debug.LogWarning("Can't create material, it already exists: " + path);
                        continue;
                    }
                    var mat = new Material(shader);
                    mat.mainTexture = tex;
                    mat.color = Color.white;
                    AssetDatabase.CreateAsset(mat, path);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }
        }
    }
}