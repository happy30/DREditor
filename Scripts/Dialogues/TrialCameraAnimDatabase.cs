/**
 * Trial Camera Animation Database for DREditor
 * Original Author: KHeartz
 */

using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Dialogues
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/CameraAnim/CameraAnim Database", fileName = "CameraAnimDatabase")]
    public class TrialCameraAnimDatabase : ScriptableObject
    {
        public List<AnimationClip> anims = new List<AnimationClip>();
        public List<string> animNames = new List<string>();
        public List<bool> animValues = new List<bool>();
        public List<string> GetNames()
        {
            var names = new List<string>();
            animNames.ForEach(anim => names.Add(anim));
            return names;
        }
    }
}