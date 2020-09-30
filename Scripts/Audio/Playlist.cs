//DRPlaylist script by SeleniumSoul for DREditor
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Audio
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/MusicPlaylist", fileName = "New Music Playlist")]
    public class Playlist : ScriptableObject
    {
        public List<Music> Musics = new List<Music>();

        public string[] GetAudioTitles()
        {
            string[] titles = new string[Musics.Count];

            for (int i = 0; i < Musics.Count; i++)
            {
                titles[i] = Musics[i].Title;
            }
            return titles;
        }

        public int[] GetAudioCount()
        {
            int[] musiccount = new int[Musics.Count];

            for (int i = 0; i < Musics.Count; i++)
            {
                musiccount[i] = i;

            }
            return musiccount;
        }
    }

    [System.Serializable]
    public class Music
    {
        public string Title;
        public AudioClip BGM;
    }
}