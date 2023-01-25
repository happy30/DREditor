//DRPlaylist script by SeleniumSoul for DREditor
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Audio
{
    using Debug = UnityEngine.Debug;
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
        public AudioClip GetAudioClip(string title)
        {
            foreach(Music music in Musics)
            {
                if (music.Title == title)
                    return music.BGM;
            }
            Debug.Log("Couldn't find song: " + title);
            return null;
        }
        public string GetTitleFromClip(AudioClip clip)
        {
            foreach (Music music in Musics)
            {
                if (music.BGM == clip)
                    return music.Title;
            }
            Debug.Log("Couldn't find Title: " + clip);
            return null;
        }
    }

    [System.Serializable]
    public class Music
    {
        public string Title;
        public AudioClip BGM;
    }
}