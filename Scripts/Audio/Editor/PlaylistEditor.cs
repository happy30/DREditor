//Music Playlist script by SeleniumSoul for DREditor

using UnityEditor;
using UnityEngine;
using DREditor.Audio;

namespace DREditor.Audio.Editor
{
    [CustomEditor(typeof(Playlist))]
    public class PlaylistEditor : UnityEditor.Editor
    {
        Playlist playlist;

        public void OnEnable()
        {
            playlist = (Playlist)target;
        }

        public override void OnInspectorGUI()
        {
            if (playlist.Musics != null)
            {
                GUIStyle Title = new GUIStyle
                {
                    fontSize = 25,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Soundtrack Jukebox", Title);
                EditorGUILayout.EndHorizontal();

                EditorStyles.textArea.wordWrap = true;
                EditorStyles.textField.wordWrap = true;

                for (int i = 0; i < playlist.Musics.Count; i++)
                {
                    EditorGUILayout.BeginVertical("Box");

                    EditorGUILayout.BeginHorizontal();
                    GUIStyle Num = new GUIStyle
                    {
                        fontSize = 25,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    };

                    EditorGUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(i.ToString(), Num, GUILayout.Width(50));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();

                    playlist.Musics[i].Title = EditorGUILayout.DelayedTextField("Title", playlist.Musics[i].Title);
                    playlist.Musics[i].BGM = (AudioClip)EditorGUILayout.ObjectField("Music File", playlist.Musics[i].BGM, typeof(AudioClip), false);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("-"))
                    {
                        playlist.Musics.Remove(playlist.Musics[i]);
                    }

                    if (i > 0)
                    {
                        GUI.enabled = true;

                        if (GUILayout.Button("ʌ") && i > 0)
                        {

                            var mus = playlist.Musics[i - 1];

                            playlist.Musics[i - 1] = playlist.Musics[i];
                            playlist.Musics[i] = mus;
                        }
                    }
                    else
                    {
                        GUI.enabled = false;
                        GUILayout.Button("ʌ");
                    }

                    GUI.enabled = true;

                    if (i < playlist.Musics.Count - 1)
                    {
                        GUI.enabled = true;
                        if (GUILayout.Button("v"))
                        {
                            var mus = playlist.Musics[i + 1];
                            playlist.Musics[i + 1] = playlist.Musics[i];
                            playlist.Musics[i] = mus;
                        }
                    }
                    else
                    {
                        GUI.enabled = false;
                        GUILayout.Button("v");
                    }

                    GUI.enabled = true;

                    if (GUILayout.Button("+"))
                    {
                        playlist.Musics.Insert(i + 1, new Music());
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                if (GUILayout.Button("Add Music"))
                {
                    playlist.Musics.Add(null);
                }
            }
        }
    }
}