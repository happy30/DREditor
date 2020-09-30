/**
 * Present Database Editor for DREditor
 * Original Author: KHeartz
 */

using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using DRUtility = DREditor.Utility.Editor.HandyFields;

namespace DREditor.Presents.Editor
{
    [CustomEditor(typeof(PresentDatabase))]
    public class PresentDatabaseEditor : UnityEditor.Editor
    {
        private PresentDatabase pdb;
        private void OnEnable() => pdb = target as PresentDatabase;
        public override void OnInspectorGUI()
        {
            DRUtility.Label("Present Database");
            GUILayout.Label("Present Count: " + pdb.presents.Count);
            CreateForm();
            EditorUtility.SetDirty(pdb);
        }

        private void CreateForm()
        {
            if (!AddNewPresent())
            {
                return;
            }

            for (int i = 0; i < pdb.presents.Count; i++)
            {
                var present = pdb.presents[i];
                using (new EditorGUILayout.HorizontalScope("Box"))
                {
                    ShowPresentImage(present);
                    SelectPresent(present, i);
                }
            }
        }

        private bool AddNewPresent()
        {
            if (GUILayout.Button("Add New Present"))
            {
                if (pdb.presents == null)
                {
                    pdb.presents = new List<Present>();
                }
                pdb.presents.Add(CreateInstance<Present>());
            }
            return pdb.presents == null;
        }

        private void ShowPresentImage(Present present)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label(AssetPreview.GetAssetPreview(present.image));
            }
        }

        private void SelectPresent(Present present, int idx)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.FlexibleSpace();
                pdb.presents[idx] = DRUtility.UnityField(present, 400);
                GUILayout.FlexibleSpace();
            }
        }
    }
}
