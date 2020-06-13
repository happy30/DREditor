using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace DREditor.PresentEditor
{
    [CustomEditor(typeof(PresentDatabase))]
    public class PresentDatabaseEditor : Editor
    {
        private PresentDatabase pdb;
        private void OnEnable()
        {
            pdb = target as PresentDatabase;
        }
        public override void OnInspectorGUI()
        {
            Label("Present Database");
            GUILayout.Label("Present Count: " + pdb.presents.Count);

            if (GUILayout.Button("Add New Present"))
            {
                if (pdb.presents == null)
                {
                    pdb.presents = new List<Present>();
                }
                pdb.presents.Add(CreateInstance<Present>());
            }

            if (pdb.presents == null)
            {
                return;
            }

            //foreach (var present in pdb.presents)
            for (int i = 0; i < pdb.presents.Count; i++)
            {
                var present = pdb.presents[i];
                GUILayout.BeginHorizontal("Box");

                //Show Present Image
                GUILayout.BeginVertical();
                GUILayout.Label(AssetPreview.GetAssetPreview(present.image));
                GUILayout.EndVertical();


                // Select Present asset
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                pdb.presents[i] = (Present)EditorGUILayout.ObjectField(present, typeof(Present), false, GUILayout.Width(400));
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();


                GUILayout.EndHorizontal();
            }
            EditorUtility.SetDirty(pdb);
        }
        private void Label(string label)
        {
            GUI.backgroundColor = Color.white;
            var labelStyle = new GUIStyle();
            labelStyle.fontSize = 10;
            GUILayout.Label(label, labelStyle);
        }
    }
}
