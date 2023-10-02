using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DREditor.Dialogues;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System;
using System.Linq;

public static class TextFileTrim
{
    [MenuItem("Tools/Text File/Trim Comments on Text File")]
    public static void Trim()
    {
        string path = EditorUtility.OpenFilePanelWithFilters("Open dialogue text file", "", new string[] { "Text file", "txt" });
        if (path.Length != 0)
        {
            var linesList = File.ReadAllLines(path).ToList();
            for(int i = 0; i < linesList.Count; i++)
            {
                if (linesList[i].StartsWith("//"))
                {
                    linesList[i] = "\n";
                }
            }
            File.WriteAllLines(path, linesList.ToArray());
            


        }
        else
        {
            // If no file was selected cancel the process.
            Debug.LogWarning("DialogueImporter: No input file selected. Process aborted");
            return;
        }
    }
}
