using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

namespace DREditor.DialogueEditor
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/CameraAnim/CameraAnim Database", fileName = "CameraAnimDatabase")]
    public class TrialCameraAnimDatabase : ScriptableObject
    {
        public AnimatorController controller;
        public List<AnimationClip> anims = new List<AnimationClip>();

        public List<string> GetNames()
        {
            var names = new List<string>();
            anims.ForEach(anim => names.Add(anim.name));
            return names;
        }
    }
}