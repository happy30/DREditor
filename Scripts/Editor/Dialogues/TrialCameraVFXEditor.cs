#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using DREditor.Dialogues;

namespace DREditor.Dialogues.Editor
{
    [CustomEditor(typeof(TrialCameraVFX))]
    public class TrialCameraVFXEditor : UnityEditor.Editor
    {
        private TrialCameraVFX vfx;

        private void OnEnable() => vfx = target as TrialCameraVFX;

        public override void OnInspectorGUI()
        {
            CreateForm();
            EditorUtility.SetDirty(vfx);
        }
        private void CreateForm()
        {
        }
    }
}

#endif