using UnityEditor;
using UnityEngine;

namespace DREditor.Dialogues.Editor
{
    [CustomEditor(typeof(TrialCameraVFXDatabase))]
    public class TrialCameraVFXDatabaseEditor : UnityEditor.Editor
    {
        private TrialCameraVFXDatabase cdb;

        private void OnEnable() => cdb = target as TrialCameraVFXDatabase;

        public override void OnInspectorGUI()
        {
            CreateForm();
            EditorUtility.SetDirty(cdb);
        }
        private void CreateForm()
        {
            EditHeader();
            EditAddButton();

            if (cdb.vfxs == null)
            {
                return;
            }
            EditVFXs();
        }
        private void EditHeader()
        {
            Utility.Editor.HandyFields.Label("Trial Camera VFX Database");
        }
        private void EditAddButton()
        {
            if (GUILayout.Button("Add New Camera VFX"))
            {
                cdb.vfxs.Add(CreateInstance<TrialCameraVFX>());
            }
        }
        private void EditVFXs()
        {
            for (int i = 0; i < cdb.vfxs.Count; i++)
            {
                var camVFX = cdb.vfxs[i];
                using (new EditorGUILayout.HorizontalScope("Box"))
                {
                    if (camVFX == null)
                    {
                        continue;
                    }
                    EditVFX(camVFX, i);
                    GUILayout.FlexibleSpace();
                }
            }
        }
        private void EditVFX(TrialCameraVFX camVFX, int idx)
        {
            GUIStyle expr = new GUIStyle();
            EditorGUILayout.LabelField(GUIContent.none, expr, GUILayout.Width(100), GUILayout.Height(100));

            using (new EditorGUILayout.VerticalScope("Box"))
            {
                var bigLabelStyle = new GUIStyle()
                {
                    fontSize = 25,
                    fontStyle = FontStyle.Bold
                };
                GUILayout.Label(camVFX.vfxName, bigLabelStyle);

                GUILayout.FlexibleSpace();

                cdb.vfxs[idx] = Utility.Editor.HandyFields.UnityField(camVFX, 100);

                if (GUILayout.Button("Remove", GUILayout.Width(100)))
                {
                    cdb.vfxs.Remove(camVFX);
                }
            }
        }
    }
}
