using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

using CSharpVitamins;

using DREditor.Dialogues;
using DREditor.Localization;

namespace DREditor.Toolbox
{

    public class DialogueTranslationsImporter : ScriptableWizard
    {

        [Tooltip("The localization settings file")]
        public LocaleSettings localeSettings;
        [Tooltip("The target locale of this translation")]
        public Locale locale;
        [Tooltip("The original dialogues for this translation. The order must match with the input file")]
        public List<Dialogue> originalDialogues = new List<Dialogue>();

        [MenuItem("Tools/DREditor/Import Dialogue Translations")]
        public static void CreateWizard()
        {
            DisplayWizard<DialogueTranslationsImporter>("Import Dialogue Translations", "Import");
        }

        private void OnWizardCreate()
        {
            // Validate the original dialogues
            if(originalDialogues == null || originalDialogues.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "You need to specify at least one original dialogue", "OK");
            }
            foreach(Dialogue dia in originalDialogues)
            {
                if(dia == null || dia.Lines == null || dia.Lines.Count == 0)
                {
                    EditorUtility.DisplayDialog("Error", "An empty dialogue was specified", "OK");
                    return;
                }
            }
            List<LocalizedDialogue> localizedDialogues = new List<LocalizedDialogue>();

            string path = EditorUtility.OpenFilePanelWithFilters("Open dialogue translations file", "", new string[] { "Text file", "txt" });
            if(path.Length != 0)
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    LocalizedDialogue currentDialogue = CreateInstance<LocalizedDialogue>();
                    currentDialogue.Lines = new List<LocalizedDialogueLine>();
                    localizedDialogues.Add(currentDialogue);
                    Regex regex = new Regex(@"^(.+?)(?: *):(?: *)(.*)$");
                    LocalizedDialogueLine lastLine = null;
                    int emptyLineCounter = 0;
                    while(!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line.StartsWith("//"))
                        {
                            emptyLineCounter = 0;
                            continue;
                        }

                        if (line.Trim().Length == 0)
                        {
                            emptyLineCounter++;
                        }
                        else
                        {
                            if(emptyLineCounter >= 2)
                            {
                                currentDialogue = CreateInstance<LocalizedDialogue>();
                                currentDialogue.Lines = new List<LocalizedDialogueLine>();
                                localizedDialogues.Add(currentDialogue);
                                lastLine = null;
                            }

                            if(regex.IsMatch(line))
                            {
                                Match m = regex.Match(line);
                                // We don't care about the character name because it will be taken from the original
                                string lineContent = m.Groups[2].Value;
                                LocalizedDialogueLine localizedLine = new LocalizedDialogueLine();
                                localizedLine.Text = lineContent;
                                lastLine = localizedLine;
                                currentDialogue.Lines.Add(localizedLine);
                            } else
                            {
                                if (lastLine == null)
                                {
                                    EditorUtility.DisplayDialog("Format incorrect", "The file didn't comply with the format", "OK");
                                    return;
                                }
                                // Trim whitespaces to ensure that there are none before or after the line break character
                                string textContent = lastLine.Text.Trim();
                                textContent += "\n";
                                textContent += line.Trim();
                                lastLine.Text = textContent;
                            }

                            emptyLineCounter = 0;
                        }
                    }
                }
            }

            // Validate the number of dialogues and lines
            if (originalDialogues.Count != localizedDialogues.Count)
            {
                EditorUtility.DisplayDialog("Error", "The number of dialogues in the input file mismatch with the original dialogues specified", "OK");
                return;
            }
            for(int i=0;i<originalDialogues.Count;i++)
            {
                if(originalDialogues[i].Lines.Count != localizedDialogues[i].Lines.Count)
                {
                    EditorUtility.DisplayDialog("Error", "The number of lines in the input file mismatch with the original dialogues specified", "OK");
                    return;
                }
            }
            // If the code reaches here it means that we can save the localizations now.
            string baseFolder = "Resources/" + localeSettings.baseFolder + "/" + locale.langCode + "/Dialogue";
            string targetDirectory = CreateIntermediateFolders(baseFolder);
            AssetDatabase.StartAssetEditing();
            for(int i=0;i<originalDialogues.Count;i++)
            {
                Dialogue dia = originalDialogues[i];
                LocalizedDialogue localized = localizedDialogues[i];
                if(string.IsNullOrEmpty(dia.translationKey))
                {
                    dia.translationKey = "dialogue-" + ShortGuid.NewGuid();
                }
                localized.translationKey = dia.translationKey;
                for(int x = 0;x < dia.Lines.Count;x++)
                {
                    if(string.IsNullOrEmpty(dia.Lines[x].translationKey))
                    {
                        dia.Lines[x].translationKey = "line-" + ShortGuid.NewGuid();
                    }
                    localized.Lines[x].original = dia.Lines[x];
                    localized.Lines[x].translationKey = dia.Lines[x].translationKey;
                }
                AssetDatabase.CreateAsset(localized, targetDirectory + "/" + localized.translationKey + ".asset");
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }

        private string CreateIntermediateFolders(string folder)
        {
            // Normalize path
            string folderPath = folder.Replace('\\', '/');
            string[] parts = folderPath.Split('/');
            string tempPath = "Assets";
            foreach (string p in parts)
            {
                if (p.Length == 0)
                {
                    continue;
                }
                if (!Directory.Exists(tempPath + "/" + p))
                {
                    AssetDatabase.CreateFolder(tempPath, p);
                }
                tempPath += "/" + p;
            }

            return tempPath;
        }
    }
}
