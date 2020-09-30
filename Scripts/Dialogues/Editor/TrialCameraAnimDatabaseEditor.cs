/**
 * Trial Camera Animation Database Editor for DREditor
 * Original Author: KHeartz
 */

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DREditor.Dialogues.Editor
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
            //cdb.controller = Utility.Editor.HandyFields.UnityField(cdb.controller, 200, 20);
            if (GUILayout.Button("Refresh Animator Controller"))
            {
                RecreateAnimatorController();
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
        private AnimatorController GetAnimatorController()
        {
            var dir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(cdb));
            var controllerPath = System.IO.Path.Combine(dir, "animDatabaseController.controller");
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller != null)
            {
                ClearAllStatesAndParameters(controller);
                return controller;
            }
            return AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }
        private void RecreateAnimatorController()
        {
            var controller = GetAnimatorController();
            var rootStateMachine = controller.layers[0].stateMachine;
            rootStateMachine.AddEntryTransition(rootStateMachine.AddState("New State"));
            foreach (var anim in cdb.anims)
            {
                if (anim == null)
                {
                    continue;
                }
                var animState = controller.AddMotion(anim);
                controller.AddParameter(anim.name, AnimatorControllerParameterType.Trigger);
                var transition = rootStateMachine.AddAnyStateTransition(animState);
                transition.AddCondition(AnimatorConditionMode.If, 0, anim.name);
                transition.duration = 0;
            }
            RefreshAnimationWindow(controller);
        }
        private void ClearAllStatesAndParameters(AnimatorController controller)
        {
            for (int i = controller.parameters.Length - 1; i >= 0; i--)
            {
                controller.RemoveParameter(i);
            }
            var rootStateMachine = controller.layers[0].stateMachine;
            for (int i = rootStateMachine.entryTransitions.Length - 1; i >= 0; i--)
            {
                rootStateMachine.RemoveState(rootStateMachine.entryTransitions[i].destinationState);
            }
            for (int i = rootStateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
            {
                rootStateMachine.RemoveState(rootStateMachine.anyStateTransitions[i].destinationState);
            }
        }

        private void RefreshAnimationWindow(AnimatorController controller)
        {
            Selection.activeObject = controller;
            EditorWindow.GetWindow<EditorWindow>("Animator").Close();
            EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
            Selection.activeObject = cdb;
        }
    }
}
