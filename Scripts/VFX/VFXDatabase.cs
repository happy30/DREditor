using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.VFX
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/VFX/VFX Database", fileName = "VFXDatabase")]
    public class VFXDatabase : ScriptableObject
    {
        [SerializeField] 
        public List<AnimationClip> VFXClips = new List<AnimationClip>();

        public AnimationClip FindClip(string name)
        {
            foreach(AnimationClip clip in VFXClips)
            {
                if (clip.name.Equals(name))
                {
                    return clip;
                }
            }
            UnityEngine.Debug.Log("Cant Find that Animation!");
            return null;
        }

        public List<string> GetNames()
        {
            var names = new List<string>();

            foreach (var cha in VFXClips)
            {
                names.Add(cha.name);
            }
            return names;
        }
    }
}
