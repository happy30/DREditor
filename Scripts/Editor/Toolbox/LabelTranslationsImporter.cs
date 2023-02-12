using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

using DREditor.Localization;

namespace DREditor.Toolbox
{

    public class LabelTranslationsImporter : ScriptableWizard
    {
        [Tooltip("The localization settings file")]
        public LocaleSettings localeSettings;
        [Tooltip("The target locale of this translation")]
        public Locale locale;
        [Tooltip("The database containing the texts for the default locale")]
        public TranslatableTextDatabase translatablesDatabase;
        [Tooltip("Indicates whether to lookup the original text in case-sensitive manner or not")]
        public bool caseSensitive;

        [MenuItem("Tools/DREditor/Import Label Translations")]
        public static void CreateWizard()
        {
            DisplayWizard<LabelTranslationsImporter>("Import Label Translations", "Import");
        }

        private void OnWizardCreate()
        {
            if(translatablesDatabase == null)
            {
                EditorUtility.DisplayDialog("Error", "You need to select the translatables database", "OK");
                return;
            }
            if(translatablesDatabase.isTranslation && translatablesDatabase.original != null)
            {
                // Instead of the original database, a translation was chosen, but we can set it to the right one.
                translatablesDatabase = translatablesDatabase.original;
            }
            TranslatableTextDatabase databaseToSave = null;
            string path = EditorUtility.OpenFilePanelWithFilters("Open label translation file", "", new string[] { "Text file", "txt" });
            if(path.Length != 0)
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    databaseToSave = CreateInstance<TranslatableTextDatabase>();
                    databaseToSave.original = translatablesDatabase;
                    databaseToSave.isTranslation = true;
                    databaseToSave.translatables = new List<TranslatableText>();
                    Regex regex = new Regex(@"^(.+?)(?: *):(?: *)(.*)$");
                    int lineCount = 0;
                    while(!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        lineCount++;
                        // Ignore comments and empty lines
                        if(line.StartsWith("//") || line.Trim().Length == 0)
                        {
                            continue;
                        }
                        if(regex.IsMatch(line))
                        {
                            Match m = regex.Match(line);
                            string originalContent = m.Groups[1].Value;
                            TranslatableText originalText = FindByOriginalContent(originalContent);
                            if(originalText == null)
                            {
                                EditorUtility.DisplayDialog("Error", "The original text for a label couldn't be found. Aborting", "OK");
                                return;
                            }
                            string translation = m.Groups[2].Value;
                            TranslatableText translatedText = new TranslatableText();
                            translatedText.original = originalText;
                            translatedText.translationKey = originalText.translationKey;
                            translatedText.Text = translation;
                            databaseToSave.translatables.Add(translatedText);
                            
                        } else
                        {
                            Debug.LogWarningFormat("LabelTranslationImporter: The line number {0} doesn't matches the format", lineCount);
                        }
                    }
                }
            } else
            {
                Debug.LogWarning("LabelTranslationImporter: No input file selected. Process aborted");
                return;
            }
            if(databaseToSave != null)
            {
                string baseFolder = "Resources/" + localeSettings.baseFolder + "/" + locale.langCode + "/Texts";
                string targetDirectory = CreateIntermediateFolders(baseFolder);
                AssetDatabase.CreateAsset(databaseToSave, targetDirectory + "/TranslatablesDatabase.asset");
                AssetDatabase.SaveAssets();
            }
        }

        private TranslatableText FindByOriginalContent(string original)
        {
            TranslatableText result = null;
            foreach(TranslatableText item in translatablesDatabase.translatables)
            {
                if(original.Equals(item.Text) || (!caseSensitive && original.ToLowerInvariant().Equals(item.Text.ToLowerInvariant())))
                {
                    result = item;
                    break;
                }
            }
            return result;
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
