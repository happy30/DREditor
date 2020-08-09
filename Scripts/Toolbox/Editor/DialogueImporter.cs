using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using DREditor.Dialogues;
using DREditor.Characters;
using DREditor.Utility.Editor;


namespace DREditor.Toolbox
{

    public class DialogueImporter : ScriptableWizard
    {

        public enum NameMatchOption
        {
            FullName, FirstNameOnly, LastNameOnly
        }

        public enum DialogueKeywords
        {
            Name, Expression, SoundFX, Voice, DialogueText
        }
        
        private class DialogueLineInfo
        {
            public Character character;
            public Expression expression;
            public AudioClip soundFX;
            public AudioClip voice;
            public int speakerIndex;
            public string textContent;
            public string speakerName;
            public int expressionNumber;
        }


        [Tooltip("The character database to use for searching the characters")]
        public CharacterDatabase database;
        [Tooltip("Indicates the criteria to match a character name")]
        public NameMatchOption nameMatch;
        [Tooltip("The output directory for imported dialogues. Relative to Assets/")]
        public string targetDirectory = "Dialogues";
        [Tooltip("Prefix used for the generated Dialogue assets.")]
        public string dialoguePrefix = "Dialogues";
        [Tooltip("The first number used when generating the imported dialogues file names. This allows to load the dialogues in chunks.")]
        public int startingNumber = 1;
        [Tooltip("The maximimum amount of lines, in terms of the dialogue objects")]
        public int maxLinesPerDialogue = 25;
        [Tooltip("The maximimum amount of text characters allowed in a line")]
        public int maxCharactersPerLine = 100;
        [Tooltip("Custom Regex string")]
        public string regexString = @"^(.+?)(?: *)/(?: *)(.+?)(?: *):(?: *)(.*)$";
        [Tooltip("Capturing groups")]
        public List<DialogueKeywords> keyList = new List<DialogueKeywords>() { DialogueKeywords.SoundFX, DialogueKeywords.Name, DialogueKeywords.DialogueText };

        // Time to wait before showing the progress
        private double waitForProgress = 0.5;
        private bool lineBroke = false;

        [MenuItem("Tools/DREditor/Import Dialogues")]
        public static void CreateWizard()
        {
            DisplayWizard<DialogueImporter>("Import Dialogues", "Import");
        }

        private void OnWizardCreate()
        {
            if (database)
            {
                if (database.Characters == null)
                {
                    EditorUtility.DisplayDialog("CharacterDatabase is empty", "Add at least one character in the CharacterDatabase.", "OK");
                    return;
                }

                if (database.Characters.Count == 0)
                {
                    EditorUtility.DisplayDialog("CharacterDatabase is empty", "Add at least one character in the CharacterDatabase.", "OK");
                    return;
                }

                if (database.Characters.Count > 0)
                {
                    foreach (var stu in database.Characters)
                    {
                        if (stu == null)
                        {
                            EditorUtility.DisplayDialog("CharacterDatabase is empty", "Nullref in CharacterDatabase. Is an element empty?", "OK");
                            return;
                        }
                    }
                }

                List<Dialogue> dialoguesToSave = new List<Dialogue>();
                float progress = 0;
                bool progressBarShown = false;
                bool showWarnCharactersNotFound = false;
                string path = EditorUtility.OpenFilePanelWithFilters("Open dialogue text file", "", new string[] { "Text file", "txt" });
                if (path.Length != 0)
                {
                    double startTime = EditorApplication.timeSinceStartup;
                    long totalBytes = new FileInfo(path).Length;
                    long bytesRead = 0;
                    using (StreamReader reader = new StreamReader(path, System.Text.Encoding.UTF8))
                    {
                        
                        Dialogue currentDialogue = CreateInstance<Dialogue>();
                        currentDialogue.Speakers = database;
                        currentDialogue.Lines = new List<Line>();
                        dialoguesToSave.Add(currentDialogue);
                        Regex regex = new Regex(regexString);
                        Line lastLine = null;
                        int emptyLineCounter = 0;
                        int currentLine = 0;
                        while (!reader.EndOfStream)
                        {
                            DialogueLineInfo lineInfo = new DialogueLineInfo();
                            string line = reader.ReadLine();
                            bytesRead += Encoding.UTF8.GetByteCount(line);
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
                                if (emptyLineCounter >= 2 || currentDialogue.Lines.Count + 1 > maxLinesPerDialogue)
                                {
                                    currentDialogue = CreateInstance<Dialogue>();
                                    currentDialogue.Speakers = database;
                                    currentDialogue.Lines = new List<Line>();
                                    dialoguesToSave.Add(currentDialogue);
                                    lastLine = null;
                                }

                                if (regex.IsMatch(line))
                                {
                                    Match m = regex.Match(line); // creates string array based on the expression
                                    for (int i = 1; i < m.Groups.Count; i++)
                                    {
                                        var group = m.Groups[i];
                                        var keyword = keyList[i - 1];
                                        
                                        switch (keyword)
                                        {
                                            case DialogueKeywords.Name:
                                                lineInfo.character = FindCharacterInDB(group.Value, out lineInfo.speakerIndex);
                                                break;
                                        }
                                        
                                    }
                                    for (int i = 1; i < m.Groups.Count; i++)
                                    {
                                        var group = m.Groups[i];
                                        var keyword = keyList[i - 1];

                                        switch (keyword)
                                        {
                                            case DialogueKeywords.Expression:
                                                int exNum = 0;
                                                lineInfo.expression = FindExpression(group.Value, lineInfo.character, out exNum);
                                                lineInfo.expressionNumber = exNum;
                                                break;
                                            case DialogueKeywords.DialogueText:
                                                lineInfo.textContent = group.Value;
                                                break;
                                            case DialogueKeywords.Voice:
                                                lineInfo.voice = FindVoice(group.Value);
                                                break;
                                            case DialogueKeywords.SoundFX:
                                                lineInfo.soundFX = FindSoundFX(group.Value);
                                                break;
                                        }
                                    }


                                    string prependSpeaker = null;
                                    if(lineInfo.speakerIndex == -1)
                                    {
                                        // We couldn't find a matching character, the breakline method will preprend the character name
                                        // lineContent = line;
                                        lineInfo.speakerIndex = 0;
                                        prependSpeaker = lineInfo.character.FirstName;
                                        showWarnCharactersNotFound = true;
                                    }
                                    
                                    lineInfo.speakerName = prependSpeaker;
                                    List<Line> normalizedLines = BreakLine(lineInfo);
                                    if(normalizedLines.Count > 0)
                                    {
                                        lastLine = normalizedLines[normalizedLines.Count - 1];
                                    }
                                    currentDialogue.Lines.AddRange(normalizedLines);
                                } else 
                                {
                                    if(lastLine == null)
                                    {
                                        EditorUtility.DisplayDialog("Format incorrect", "The file didn't comply with the format", "OK");
                                        return;
                                    }

                                    // Trim whitespaces to ensure that there are none before or after the line break character
                                    string textContent = lastLine.Text.Trim(); 
                                    textContent += "\n";
                                    textContent += line.Trim();
                                    lineInfo.textContent = textContent;
                                    lineInfo.speakerIndex = lastLine.SpeakerNumber;
                                    lineInfo.character = lastLine.Speaker;
                                    lineInfo.speakerName = null;
                                    List<Line> normalizedLines = BreakLine(lineInfo);
                                    lastLine.Text = normalizedLines[0].Text;
                                    normalizedLines.RemoveAt(0);
                                    currentDialogue.Lines.AddRange(normalizedLines);
                                }
                                // Hook up each part of the dialogue line here
                                if(lineInfo.expression != null)
                                {
                                    if (currentLine >= maxLinesPerDialogue)
                                    {
                                        currentLine = 0;
                                    }
                                    currentDialogue.Lines[currentLine].Expression = lineInfo.expression;
                                    currentDialogue.Lines[currentLine].ExpressionNumber = lineInfo.expressionNumber;
                                }
                                if (lineBroke == true)
                                {
                                    currentLine++;
                                    lineBroke = false;
                                }
                                if (lineInfo.soundFX != null)
                                {
                                    currentDialogue.Lines[currentLine].SFX.Add(lineInfo.soundFX);
                                }
                                if(lineInfo.voice != null)
                                {
                                    currentDialogue.Lines[currentLine].VoiceSFX = lineInfo.voice;
                                }
                                currentLine++;
                                emptyLineCounter = 0;
                            }

                            if (EditorApplication.timeSinceStartup - startTime > waitForProgress)
                            {
                                progressBarShown = true;
                            }

                            if(progressBarShown)
                            {
                                progress = (bytesRead / totalBytes) * 0.9f; // I estimate the text processing id the 90% of the total work load
                                EditorUtility.DisplayProgressBar("Processing...", "Procesing text file.", progress);
                            }
                        }

                    }


                } else
                {
                    // If no file was selected cancel the process.
                    Debug.LogWarning("DialogueImporter: No input file selected. Process aborted");
                    return;
                }
                // Save the dialogue objects
                string folder = CreateIntermediateFolders(targetDirectory);
                int dialogueNumber = startingNumber;
                AssetDatabase.StartAssetEditing();
                for(int i=0; i<dialoguesToSave.Count;i++)
                {
                    Dialogue dia = dialoguesToSave[i];
                    dia.DialogueName = dialogueNumber+""; // dev note: I use this trick a lot when coding Java, happy to see it works with C# too.
                    string diaPath = folder + "/"+dialoguePrefix+"_" + dialogueNumber + "_.asset";
                    AssetDatabase.CreateAsset(dia, diaPath);
                    dialogueNumber++;
                    if (progressBarShown)
                    {
                        float saveProgress = (i + 1) / dialoguesToSave.Count;
                        progress += saveProgress * 0.1f;
                        EditorUtility.DisplayProgressBar("Processing...", "Saving dialogues to disk.", progress);
                    }
                }
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                if(progressBarShown)
                {
                    EditorUtility.ClearProgressBar();
                }
                if(showWarnCharactersNotFound)
                {
                    EditorUtility.DisplayDialog("Some character were not found", "Some characters couldn't be found on the database.", "OK");
                }
                
            }
            else
            {
                EditorUtility.DisplayDialog("CharacterDatabase was not specifid.", "Please create and assign a CharacterDatabase and try again", "OK");
            }
            
        }

        private string CreateIntermediateFolders(string folder)
        {
            // Normalize path
            string folderPath = folder.Replace('\\', '/');
            string[] parts = folderPath.Split('/');
            string tempPath = "Assets";
            foreach(string p in parts)
            {
                if(p.Length == 0)
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


        private List<Line> BreakLine(DialogueLineInfo lineInfo)
        {
            Character speaker = lineInfo.character;
            int speakerNumber = lineInfo.speakerIndex;
            string speakerName = lineInfo.speakerName;
            string lineContent = lineInfo.textContent;
            List<Line> result = new List<Line>(); 
            int lineLenghtOffset = 0;
            if(speakerName != null)
            {
                lineLenghtOffset = -1 * (speakerName + ": ").Length;
            }
            if(lineContent.Length + lineLenghtOffset <= maxCharactersPerLine)
            {
                Line line = new Line();
                line.Speaker = speaker;
                line.SpeakerNumber = speakerNumber;
                line.Text = "";
                if(speakerName != null)
                {
                    line.Text += speakerName + ": ";
                }
                line.Text += lineContent;
                result.Add(line);
            } else
            {
                // Break the line by words
                string[] words = lineContent.Split(' ');
                Line currentLine = new Line();
                currentLine.Speaker = speaker;
                currentLine.SpeakerNumber = speakerNumber;
                currentLine.Text = "";
                if(speakerName != null)
                {
                    currentLine.Text += speakerName + ": ";
                }
                result.Add(currentLine);
                foreach(string word in words)
                {
                    if(currentLine.Text.Length + lineLenghtOffset == 0)
                    {
                        currentLine.Text += word;
                        lineBroke = true;
                        continue;
                    }
                    if(word.Contains("\n"))
                    {
                        if(currentLine.Text.Length + lineLenghtOffset + word.Length + 1 <= maxCharactersPerLine)
                        {
                            currentLine.Text += " " + word;
                        } else
                        {
                            string[] parts = word.Split('\n');
                            // Try to fit both words keeping the line break character
                            if (currentLine.Text.Length + lineLenghtOffset + parts[0].Length + 1 <= maxCharactersPerLine)
                            {
                                currentLine.Text += " " + parts[0];
                                if (currentLine.Text.Length + lineLenghtOffset + parts[1].Length + 1 <= maxCharactersPerLine)
                                {
                                    // Best case scenario. Both words fit properly.
                                    currentLine.Text += "\n" + parts[1];
                                } else
                                {
                                    // Second word doesn't fit. It will need to go on a separate line.
                                    currentLine = new Line();
                                    currentLine.Speaker = speaker;
                                    currentLine.SpeakerNumber = speakerNumber;
                                    currentLine.Text = parts[1];
                                    result.Add(currentLine);
                                }
                            } else
                            {
                                // If not even the first word fits on the line. Both words will be on a new line separated by a whitespace instead of a line break
                                currentLine = new Line();
                                currentLine.Speaker = speaker;
                                currentLine.SpeakerNumber = speakerNumber;
                                currentLine.Text = parts[0] + " " + parts[1];
                                result.Add(currentLine);
                            }
                            
                        }
                        continue;
                    }
                    if (currentLine.Text.Length + lineLenghtOffset + word.Length + 1 > maxCharactersPerLine)
                    {
                        currentLine = new Line();
                        currentLine.Speaker = speaker;
                        currentLine.SpeakerNumber = speakerNumber;
                        currentLine.Text = word;
                        result.Add(currentLine);
                    } else
                    {
                        currentLine.Text += ' ' + word;
                    }

                }
            }
            return result;
        }

        private Character FindCharacterInDB(string name, out int index)
        {
            index = -1;
            Character found = database.Characters[0];
            for(int i=0;i<database.Characters.Count;i++)
            {
                Character stu = database.Characters[i];
                string lastFirst = stu.LastName + " " + stu.FirstName;
                string firstLast = stu.FirstName + " " + stu.LastName;
                switch(nameMatch)
                {
                    case NameMatchOption.FirstNameOnly:
                        if(stu.FirstName.Trim().ToLowerInvariant().Equals(name.Trim().ToLowerInvariant()))
                        {
                            found = database.Characters[i];
                            index = i;
                        }
                        break;
                    case NameMatchOption.LastNameOnly:
                        if (stu.LastName.Trim().ToLowerInvariant().Equals(name.Trim().ToLowerInvariant()))
                        {
                            found = database.Characters[i];
                            index = i;
                        }
                        break;
                    case NameMatchOption.FullName:
                        if (lastFirst.Trim().ToLowerInvariant().Equals(name.Trim().ToLowerInvariant())
                            || firstLast.Trim().ToLowerInvariant().Equals(name.Trim().ToLowerInvariant()))
                        {
                            // We got a match for full name, unless there is the same character with the name reversed we can safely assign it to the line.
                            found = database.Characters[i];
                            index = i;
                        }
                        break;
                }
                if(index != -1)
                {
                    // Once we found a match, don't continue searching
                    break;
                }
            }
            return found;
        }

        
        private Expression FindExpression(string exName, Character character, out int exNum)
        {
            
            for(int i = 0; i < database.Characters.Count; i++)
            {
                if(database.Characters[i].FirstName == character.FirstName)
                {
                    for(int en = 0; en < database.Characters[i].Expressions.Count; en++)
                    {
                        if (exName == database.Characters[i].Expressions[en].Name)
                        {
                            exNum = en+1;
                            return database.Characters[i].Expressions[en];
                        }
                    }
                }
            }
            exNum = 0;
            return null;
        }
        private AudioClip FindVoice(string voiceName)
        {
            return ResourcesExtension.Load<AudioClip>(voiceName);
        }
        private AudioClip FindSoundFX(string soundFXName)
        {
            return ResourcesExtension.Load<AudioClip>(soundFXName);
        }
    }
}
