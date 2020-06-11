using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DREditor.DialogueEditor.Editor
{
    [CustomEditor(typeof(TrialCameraAnimDatabase))]
    public class TrialCameraAnimDatabaseEditor : UnityEditor.Editor
    {
        private TrialCameraAnimDatabase cdb;

        private void OnEnable() => cdb = target as TrialCameraAnimDatabase;

        public override void OnInspectorGUI()
        {
            CreateForm();
            EditorUtility.SetDirty(cdb);
        }
        private void CreateForm()
        {
            EditHeader();
            EditAddButton();

            if (cdb.anims == null)
            {
                return;
            }
            EditVFXs();
        }
        private void EditHeader()
        {
            Utility.Editor.HandyFields.Label("Trial Camera Animation Database");
            EditorGUILayout.Space(30);
            Utility.Editor.HandyFields.Label("Trial Camera Animator Controller");
            cdb.controller = Utility.Editor.HandyFields.UnityField(cdb.controller, 200, 20);
            if (GUILayout.Button("Refresh Animator Controller"))
            {
                RefreshAnimatorController();
            }
            EditorGUILayout.Space(30);
        }
        private void EditAddButton()
        {
            if (GUILayout.Button("Add New Camera Animation"))
            {
                cdb.anims.Add(new AnimationClip());
            }
        }
        private void EditVFXs()
        {
            for (int i = 0; i < cdb.anims.Count; i++)
            {
                var camAnim = cdb.anims[i];
                if (camAnim == null)
                {
                    continue;
                }
                using (new EditorGUILayout.HorizontalScope("Box"))
                {
                    EditAnim(camAnim, i);
                    GUILayout.FlexibleSpace();
                }
            }
        }
        private void EditAnim(AnimationClip camAnim, int idx)
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
                //GUILayout.Label(camAnim.animClip.name, bigLabelStyle);

                GUILayout.FlexibleSpace();

                cdb.anims[idx] = Utility.Editor.HandyFields.UnityField(camAnim, 100);

                if (GUILayout.Button("Remove", GUILayout.Width(100)))
                {
                    cdb.anims.Remove(camAnim);
                }
            }
        }
        private void RefreshAnimatorController()
        {
            ClearAllStatesAndParameters();
            var rootStateMachine = cdb.controller.layers[0].stateMachine;
            foreach (var anim in cdb.anims)
            {
                if (anim == null)
                {
                    continue;
                }
                var animState = cdb.controller.AddMotion(anim);
                cdb.controller.AddParameter(anim.name, AnimatorControllerParameterType.Trigger);
                var transition = rootStateMachine.AddAnyStateTransition(animState);
                transition.AddCondition(AnimatorConditionMode.If, 0, anim.name);
                transition.duration = 0;
            }
            RefreshAnimationWindow();
        }
        private void ClearAllStatesAndParameters()
        {
            for (int i = cdb.controller.parameters.Length - 1; i >= 0; i--)
            {
                cdb.controller.RemoveParameter(i);
            }
            var rootStateMachine = cdb.controller.layers[0].stateMachine;
            for (int i = rootStateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
            {
                rootStateMachine.RemoveState(rootStateMachine.anyStateTransitions[i].destinationState);
            }
        }
        private void CopyDREditorDefaultAnimations()
        {

        }
        private void RefreshAnimationWindow()
        {
            Selection.activeObject = cdb.controller;
            EditorWindow.GetWindow<EditorWindow>("Animator").Close();
            EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
            Selection.activeObject = cdb;
        }
    }
}
