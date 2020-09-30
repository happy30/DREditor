//Backlog Scriptable Object script for DREditor by SeleniumSoul
using System.Collections.Generic;
using UnityEngine;
using DREditor.Characters;

namespace DREditor.Dialogues
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Dialogues/Backlog File", fileName = "New Backlog File")]
    public class Backlog : ScriptableObject
    {
        [Tooltip("Capacity of lines that the Backlog can hold.\nUses FIFO.")]
        public int Capacity = 150;
        public Queue<BacklogLines> Lines = new Queue<BacklogLines>();

        public void AddLine(Character charfile, string text, AudioClip voice = null)
        {
            BacklogLines _backline = new BacklogLines();
            _backline.Charfile = charfile;
            _backline.Text = text;
            _backline.Voice = voice;
            if (Lines.Count < Capacity)
            {
                Lines.Enqueue(_backline);
            }
            else
            {
                Lines.Enqueue(_backline);
                Lines.Dequeue();
            }
        }
    }

    [System.Serializable]
    public class BacklogLines
    {
        public string Text;
        public Character Charfile;
        public AudioClip Voice;
    }
}
