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

        public List<string> GetNames()
        {
            var names = new List<string>();
            anims.ForEach(anim => names.Add(anim.name));
            return names;
        }
    }
}