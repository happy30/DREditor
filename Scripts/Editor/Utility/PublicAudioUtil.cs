﻿using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace DREditor.Utility
{
	public static class PublicAudioUtil
	{
        public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null
            );

            method?.Invoke( null, new object[] { clip, startSample, loop } );
        }
    }
}