#if UNITY_EDITOR
//Music Playlist script by SeleniumSoul for DREditor
// Updated by Sweden/Zetsis For EG
using UnityEditor;
using UnityEngine;
using DREditor.Audio;

namespace DREditor.Audio.Editor
{
    [CustomEditor(typeof(Playlist))]
    public class PlaylistEditor : UnityEditor.Editor
    {
        Playlist playlist;
        SerializedProperty musicLines;
        public void OnEnable()
        {
            playlist = (Playlist)target;
            musicLines = serializedObject.FindProperty("Musics");
        }

        public override void OnInspectorGUI()
        {
            CreateForm();
            EditorUtility.SetDirty(playlist);
        }
        private void CreateForm()
        {
            if (playlist.Musics.Count != 0)
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

                for (int i = 0; i < musicLines.arraySize; i++)
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

                    playlist.Musics[i].Title = EditorGUILayout.TextField("Title", playlist.Musics[i].Title);
                    //playlist.Musics[i].BGM = (AudioClip)EditorGUILayout.ObjectField("Music File", playlist.Musics[i].BGM, typeof(AudioClip), false);
                    //var titleProperty = musicLines.GetArrayElementAtIndex(i).FindPropertyRelative("Title");
                    //EditorGUIUtility.labelWidth = 1;
                    //EditorGUILayout.PropertyField(titleProperty, new GUIContent(""), GUILayout.MinWidth(270));
                    if (i < playlist.Musics.Count)
                    {
                        //UnityEngine.Debug.Log("Index: " + i);
                        //UnityEngine.Debug.Log("Count: " + playlist.Musics.Count);

                        if (musicLines.arraySize > i && musicLines.GetArrayElementAtIndex(i) != null)
                        {
                            var sfxListProperty = musicLines.GetArrayElementAtIndex(i).FindPropertyRelative("BGM");
                            EditorGUIUtility.labelWidth = 1;
                            EditorGUILayout.PropertyField(sfxListProperty, new GUIContent(""), GUILayout.MinWidth(270));
                        }
                        
                    }
                    

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("-"))
                    {
                        playlist.Musics.Remove(playlist.Musics[i]);
                        musicLines.serializedObject.Update();
                        return;
                    }

                    if (i > 0)
                    {
                        GUI.enabled = true;

                        if (GUILayout.Button("ʌ") && i > 0)
                        {

                            var mus = playlist.Musics[i - 1];

                            playlist.Musics[i - 1] = playlist.Musics[i];
                            playlist.Musics[i] = mus;
                            musicLines.serializedObject.Update();
                            return;
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
                            musicLines.serializedObject.Update();
                            return;
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
                        musicLines.serializedObject.Update();
                        return;
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
                    playlist.Musics.Add(new Music());
                    musicLines.serializedObject.Update();
                }
            }
        }
    }
}
#endif