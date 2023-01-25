using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DREditor.Progression
{
    /// <summary>
    /// Requires: Chapter
    /// List of chapters so progression can be referenced
    /// Put this in Resources Folder
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "ProgressionDatabase", menuName = "DREditor/Progression/Database")]
    public class ProgressionDatabase : ScriptableObject
    {
        public List<Chapter> Chapters = new List<Chapter>();
        
        public int GetChapterIndex(Chapter c)
        {
            for (int i = 0; i < Chapters.Count; i++)
                if (Chapters[i] == c)
                    return i;
            return -1;
        }
    }
}
