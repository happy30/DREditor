//Change Character Focus Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;
using DREditor.Audio;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public struct CMTuple
	{
		public int MusicNum;
		public float fadeOut;
	}

	[Serializable]
	public class ChangeMusic : IDialogueEvent
	{
		public bool _ShowHelp = false;
		public CMTuple CMValue;

		public void TriggerDialogueEvent()
		{
			DialogueEventSystem.TriggerEvent("PlayBGM", CMValue);
		}

		#if UNITY_EDITOR
		public void EditorUI()
		{
			Playlist MusicPlaylist;

			if (Resources.Load<Playlist>("Jukebox"))
			{
				MusicPlaylist = Resources.Load<Playlist>("Jukebox");
			}
			else
			{
				EditorGUILayout.HelpBox("Unable to find a Music Playlist!\nCreate a music playlist in the Resources folder as Jukebox.asset\nYou can make a Jukebox Asset using Create/DREditor/MusicPlaylist.", MessageType.Error, true);
				return;
			}

			string[] musicArray = PrependedList(MusicPlaylist.GetAudioTitles(), "<Stop Music>");
			CMValue.MusicNum = EditorGUILayout.IntPopup("BGM:", CMValue.MusicNum, musicArray, Iota(musicArray.Length, -1));
			if (CMValue.MusicNum == -1) CMValue.fadeOut = EditorGUILayout.FloatField("Fade Out (in sec):", CMValue.fadeOut);
		}

		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Change the music at this line.", MessageType.Info, true);
		}

		public static int[] Iota(int size, int value = 0)
		{
			int[] values = new int[size];
			for (int i = 0; i < values.Length; i++)
			{
				values[i] = value++;
			}
			return values;
		}
		public static T[] PrependedList<T>(T[] list, T firstElement)
		{
			T[] newList = new T[list.Length + 1];
			newList[0] = firstElement;
			for (int i = 0; i < list.Length; i++)
			{
				newList[i + 1] = list[i];
			}
			return newList;
		}
		#endif
	}
}