//Backlog Scriptable Object script for DREditor by SeleniumSoul
// Edited by Sweden#6386 to use FMOD
using System.Collections.Generic;
using UnityEngine;
using DREditor.Characters;
using System;

namespace DREditor.Dialogues
{
    [System.Serializable]
    //[CreateAssetMenu(menuName = "DREditor/Dialogues/Backlog File", fileName = "New Backlog File")]
    public class Backlog
    {
        [Tooltip("Capacity of lines that the Backlog can hold.\nUses FIFO.")]
        public int Capacity = 50;
        public Queue<BacklogLines> Lines = new Queue<BacklogLines>();

        public void AddLine(Character charfile, string text, int aliasNum, Color color)
        {
            BacklogLines _backline = new BacklogLines();
            if (charfile)
            {
                _backline.Charfile = charfile;
                _backline.CharFirstName = charfile.FirstName;
            }
            _backline.Text = text;
            _backline.AliasNum = aliasNum;
            //_backline.Voice = voice;
            _backline.color = color;
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
        public BacklogData Save()
        {
            BacklogData data = new BacklogData();
            data.Lines = Lines.ToArray();
            return data;
        }
        public void Load(BacklogData data)
        {
            try
            {
                Lines.Clear();
                foreach (BacklogLines l in data.Lines)
                {
                    Lines.Enqueue(l);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("WARNING!: The Game could not load Backlog data due to: " + e.ToString());
            }
        }
    }
    [System.Serializable]
    public class BacklogData
    {
        public BacklogLines[] Lines;
    }
    [System.Serializable]
    public class BacklogLines
    {
        public string Text;
        public string CharFirstName;
        public int AliasNum;
        public Character Charfile;
        //[EventRef] public string Voice;
        public Color color = Color.white;
    }
}
