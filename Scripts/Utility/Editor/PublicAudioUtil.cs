using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
 
public static class PublicAudioUtil {
   
    public static void PlayClip(AudioClip clip) {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "PlayClip",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new System.Type[] {
                typeof(AudioClip)
            },
            null
        );
        method.Invoke(
            null,
            new object[] {
                clip
            }
        );
    } // PlayClip()
 
} // class PublicAudioUtil