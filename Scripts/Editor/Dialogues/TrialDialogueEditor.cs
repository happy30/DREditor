#if UNITY_EDITOR
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using DREditor.Characters;
using DREditor.Utility;
using DREditor.Utility.Editor;
using DREditor.Dialogues;
using DREditor.Dialogues.Editor;
using DREditor.EventObjects;
using DREditor.Progression;
using DREditor.Dialogues.Events;
using System;
using DREditor.Camera;

namespace DREditor.Editor
{
    using Debug = UnityEngine.Debug;
    [CustomEditor(typeof(TrialDialogue))]
    public class TrialDialogueEditor : DialogueEditorBase
    {
        private TrialDialogue dia;
        private TrialCameraVFXDatabase cameraVFXDatabase;
        private TrialCameraAnimDatabase cameraAnimDatabase;
		ProgressionDatabase progression;
		private SerializedProperty propLines;
		private List<Type> _list;
		private string[] choices;
		private int _DiaEventIndex;

		bool debugMode;

		public void OnEnable()
        {
            dia = target as TrialDialogue;
            propLines = serializedObject.FindProperty("Lines");
			UpdateDiaEventList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CreateForm();
            EditorUtility.SetDirty(dia);
            serializedObject.ApplyModifiedProperties();
        }
        private void CreateForm()
        {

			if (progression == null)
			{
				int _assetamount = GetProgressionDatabase();

				if (_assetamount != 1) return;
			}
			if (!ValidateCharacterDatabase() || !ValidateCharacters() ||
                !ValidateCameraAnimDatabase() || !ValidateCameraAnims() ||
                !ValidateCameraVFXDatabase() || !ValidateCameraVFXs()) { return; }

			#region Debug Mode
			using (new EditorGUILayout.HorizontalScope())
			{
				debugMode = HandyFields.Option(debugMode, "Debug Mode: ", 80);
			}

			if (debugMode)
			{
				DebugMode();
				return;
			}
			#endregion
			
            EditorStyles.textArea.wordWrap = true;
            EditorStyles.textField.wordWrap = true;
            GUI.backgroundColor = dia.Color;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(dia.DialogueName, EditorStyles.boldLabel);
                dia.Color = EditorGUILayout.ColorField(dia.Color, GUILayout.Width(50));
            }
            EditHeader();
			EditTestLine();
            EditPanels();
			if (testLine == -1)
            {
				EditFooter();
			}
        }
		private void EditTestLine()
        {
			if (testLine != -1)
			{
				GUILayout.Space(25);
				using (new EditorGUILayout.HorizontalScope())
				{
					if (testLine != 0 && GUILayout.Button("Previous Line", GUILayout.Width(100)))
					{
						testLine -= 1;
					}
					GUILayout.Space(50);
					if (testLine != dia.Lines.Count - 1 && GUILayout.Button("Next Line", GUILayout.Width(100)))
					{
						testLine += 1;
					}
				}
			}
        }
		private void UpdateDiaEventList()
		{
			if (DialogueEventList.ListOfClasses == null)
			{
				DialogueEventList.UpdateList();
			}

			_list = DialogueEventList.ListOfClasses;
			choices = new string[_list.Count];
			for (int x = 0; x < choices.Length; x++)
			{
				choices[x] = _list[x].Name;
			}
		}
		private void RandomizeCamAnim()
        {
			if (EditorUtility.DisplayDialog("Randomize Cam Anims?",
						"WARNING!: This will overwrite ALL Camera Animations on every line in the asset! " +
						"Are you sure you want to continue?", "Yes", "No"))
			{
				for (int i = 0; i < dia.Lines.Count; i++)
				{
					dia.Lines[i].camAnimIdx = UnityEngine.Random.Range(0, cameraAnimDatabase.anims.Count - 1);
				}
				debugMode = false;
			}
			
        }
        private bool ValidateCharacterDatabase()
        {
            if (dia.Speakers == null)
            {
                var database = Resources.Load<CharacterDatabase>("Characters/CharacterDatabase");
                if (database)
                {
                    dia.Speakers = database;
                }
                else
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField("CharacterDatabase is not set.");
                        EditorGUILayout.LabelField("Create a CharacterDatabase in Resources/Characters/CharacterDatabase.asset");
                    }
                    return false;
                }
            }
            return true;
        }

        private bool ValidateCharacters()
        {
            if (dia.Speakers.Characters == null)
            {
                EditorGUILayout.LabelField("Add at least one character in the CharacterDatabase.");
                return false;
            }

            if (dia.Speakers.Characters.Empty())
            {
                EditorGUILayout.LabelField("Add at least one character in the CharacterDatabase.");
                return false;
            }

            foreach (var stu in dia.Speakers.Characters)
            {
                if (stu == null)
                {
                    EditorGUILayout.LabelField("Nullref in CharacterDatabase. Is an element empty?");
                    return false;
                }
            }
            return true;
        }

        private bool ValidateCameraVFXDatabase()
        {
            var database = Resources.Load<TrialCameraVFXDatabase>("DREditor/CameraVFX/CameraVFXDatabase");
            if (!database)
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("CameraVFXDatabase is not set.");
                    EditorGUILayout.LabelField("Create a CameraVFXDatabase in Resources/DREditor/CameraVFX/CameraVFXDatabase.asset");
                }
                return false;
            }
            cameraVFXDatabase = database;
            return true;
        }
        private bool ValidateCameraVFXs() => true;

        private bool ValidateCameraAnimDatabase()
        {
            var database = Resources.Load<TrialCameraAnimDatabase>("DREditor/CameraAnim/CameraAnimDatabase");
            if (!database)
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("CameraAnimDatabase is not set.");
                    EditorGUILayout.LabelField("Create a CameraVFXDatabase in Resources/DREditor/CameraAnim/CameraAnimDatabase.asset");
                }
                return false;
            }
            cameraAnimDatabase = database;
            return true;
        }

        private bool ValidateCameraAnims() => true;

        private void EditHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Dialogue Nr: ", GUILayout.Width(100));
                GUI.backgroundColor = Color.white;
                dia.DialogueName = GUILayout.TextField(dia.DialogueName, GUILayout.Width(40));

                if (GUILayout.Button("Update", GUILayout.Width(80)))
                {
                    var path = AssetDatabase.GetAssetPath(dia);
                    var flds = path.Split('/');
                    var nm = flds[flds.Length - 2];

                    string tx = "";
                    if (dia.Lines.Count > 0)
                    {
                        tx = dia.Lines[0].Text.Substring(0, dia.Lines[0].Text.Length > 29 ? 30 : dia.Lines[0].Text.Length);
                        tx = tx.Replace('?', ' ');
                    }

                    string fileName = nm + "_" + dia.DialogueName + "_" + tx;

                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(dia), fileName);
                    Debug.Log(fileName);
                    Debug.Log(AssetDatabase.GetAssetPath(dia));
                }
                GUILayout.Space(Screen.width - 370);
                GUI.backgroundColor = dia.Color;

            }
        }

        private void EditStudentBGColor(TrialLine currLine)
        {
            var color = Color.white;
            if (currLine.Speaker is Student)
            {
                var stu = currLine.Speaker as Student;
                color = stu.StudentCard.Color;
                Color.RGBToHSV(color, out float H, out float _, out float _);
                float S = 0.3f;
                float V = 0.95f;
                color = Color.HSVToRGB(H, S, V);
            }
            GUI.backgroundColor = color;
        }
		int testLine = -1;
        private void EditPanels()
        {
            if (dia.Lines == null)
            {
                return;
            }
			if (testLine != -1)
            {
				EditLine(testLine);
            }
            else
            {
				for (int i = 0; i < dia.Lines.Count; i++)
				{
					EditLine(i);
				}
			}
            
			if (testLine == -1)
				dia.skip = HandyFields.IntField("Skip:", dia.skip, 20);
		}
		private void EditLine(int i)
        {
			var currLine = dia.Lines[i];
			EditStudentBGColor(currLine);

			using (new EditorGUILayout.HorizontalScope("Box"))
			{
				EditLeftPanel(currLine, i);
				EditExpressionPanel(currLine);
				EditDialogueText(i);
				EditDialoguePosition(i);
				if (i >= propLines.arraySize || i >= dia.Lines.Count)
					return;
			}
			EditLineLower(currLine, i);
			GUI.color = Color.white;
			HandyFields.UISplitter(dia.Color, 2, 0, 5);
		}

        private void EditLeftPanel(TrialLine currLine, int i)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(120)))
            {
				if (testLine != -1 && GUILayout.Button("Exit", GUILayout.Width(60)))
                {
					testLine = -1;
                }
                GUI.backgroundColor = dia.Color;
                EditSpeaker(currLine);
                //EditSFX(currLine);
                EditEvents(currLine);
                //EditCamVFX(currLine);
                EditCamAnim(currLine);
                EditAutomatic(currLine);

				using (new EditorGUILayout.HorizontalScope())
                {
					currLine.DontPan = HandyFields.Option(currLine.DontPan, "Don't Pan: ", 70);
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("Test", GUILayout.Width(50)))
					{
						if (!cam)
							cam = FindObjectOfType<DRTrialCamera>();
						if (cam != null)
						{
							TestCameraAnim(currLine, true);
						}
						else
							Debug.Log("Unsuccessful");
					}
					if (GUILayout.Button("Anim", GUILayout.Width(50)))
					{
						if (!cam)
							cam = FindObjectOfType<DRTrialCamera>();
						if (cam != null)
						{
							TestCameraAnim(currLine, false);
						}
						else
							Debug.Log("Unsuccessful");
					}
					if (GUILayout.Button("^", GUILayout.Width(15)))
					{
						testLine = i;
					}
				}
				
			}
        }
		DRTrialCamera cam;
		private void TestCameraAnim(TrialLine line, bool gameTest)
        {
            if (!Application.isPlaying)
            {
				EditorUtility.DisplayDialog("Must be in Play Mode", "You must be in play mode with a DRTrialCamera to test lines!", "Ok");
				return;
            }
            if (gameTest)
            {
				cam.InvokeTestLine(line);
			}
            else
            {
				if (!line.co.enabled && line.Speaker.TrialPosition < 16)
					cam.SeatFocus = line.Speaker.TrialPosition;
				if (line.co.enabled)
				{
					cam.ApplyOverrides(line.co, line.DontPan);
				}
				List<string> names = cameraAnimDatabase.GetNames();
				cam.TriggerAnim(names[line.camAnimIdx]);
			}
			
			
			
        }
        private void EditSpeaker(TrialLine currLine)
        {
            var prependedArray = ContainerUtil.PrependedList(dia.GetCharacterNames(), "<No Character>");
            currLine.SpeakerNumber = EditorGUILayout.IntPopup(currLine.SpeakerNumber, prependedArray, ContainerUtil.Iota(prependedArray.Length, -1), GUILayout.Width(130));
            currLine.Speaker = currLine.SpeakerNumber == -1 ? null : dia.Speakers.Characters[currLine.SpeakerNumber];

            if (currLine.Speaker)
            {
                var aliasNames = new string[currLine.Speaker.Aliases.Count + 1];
                aliasNames[0] = "(No Alias)";

                for (int j = 1; j < currLine.Speaker.Aliases.Count + 1; j++)
                {
                    aliasNames[j] = currLine.Speaker.Aliases[j - 1].Name;
                }
                currLine.AliasNumber = EditorGUILayout.IntPopup(currLine.AliasNumber,
                    aliasNames, dia.getAliasesIntValues(currLine.Speaker),
                        GUILayout.Width(130));
            }
        }

        private void EditEvents(TrialLine currLine)
        {
            if (currLine.Events != null)
            {
                for (var j = 0; j < currLine.Events.Count; j++)
                {
                    var ev = currLine.Events[j];
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        currLine.Events[j] = HandyFields.UnityField(ev, 120);

                        if (GUILayout.Button("x", GUILayout.Width(20)))
                        {
                            currLine.Events.Remove(ev);
                        }
                    }
                }
            }

            if (GUILayout.Button("Add Event"))
            {
                currLine.Events.Add(CreateInstance<SceneEvent>());
            }
        }
        private void EditCamVFX(TrialLine currLine)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("VFX:", GUILayout.Width(40));
                currLine.vfxIdx = EditorGUILayout.IntPopup(currLine.vfxIdx, cameraVFXDatabase.GetNames().ToArray(), ContainerUtil.Iota(cameraVFXDatabase.vfxs.Count));
            }
        }
        private void EditCamAnim(TrialLine currLine)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Cam:", GUILayout.Width(40));
                currLine.camAnimIdx = EditorGUILayout.IntPopup(currLine.camAnimIdx, cameraAnimDatabase.GetNames().ToArray(), ContainerUtil.Iota(cameraAnimDatabase.anims.Count));
            }
        }
        private void EditAutomatic(TrialLine currLine)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Automatic:", GUILayout.Width(70));
                currLine.AutomaticLine = EditorGUILayout.Toggle(currLine.AutomaticLine);
            }

            if (currLine.AutomaticLine)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    currLine.TimeToNextLine = EditorGUILayout.FloatField(currLine.TimeToNextLine, GUILayout.Width(60));
                }
            }
        }
        private void EditSFX(TrialLine currLine)
        {
            if (currLine.SFX != null)
            {
                for (var j = 0; j < currLine.SFX.Count; j++)
                {
                    var sfx = currLine.SFX[j];
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(sfx == null))
                        {
                            if (GUILayout.Button(">", GUILayout.Width(20)))
                            {
                                //PublicAudioUtil.PlayClip(sfx);
                            }
                        }

                        //currLine.SFX[j] = HandyFields.UnityField(sfx, 76);

                        if (GUILayout.Button("x", GUILayout.Width(20)))
                        {
                            currLine.SFX.Remove(sfx);
                        }
                    }
                }
            }

            if (GUILayout.Button("Add Sound"))
            {
                currLine.SFX.Add(null);
            }
        }
        private void EditExpressionPanel(TrialLine currLine)
        {
            if (currLine.Speaker != null && !IsProtagonist(currLine.Speaker))
            {
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    var exprs = currLine.Speaker.Expressions.Count;

                    if (exprs < currLine.ExpressionNumber)
                    {
                        currLine.ExpressionNumber = 0;
                    }

                    if (currLine.Expression != null)
                    {
                        GUIStyle expr = new GUIStyle();
                        if (currLine.ExpressionNumber > 0)
                        {
                            var tex = HandyFields.GetMaterialTexture(currLine.Expression.Sprite);
                            if (tex)
                            {
                                expr.normal.background = tex;
                            }
                        }
                        EditorGUILayout.LabelField(GUIContent.none, expr, GUILayout.Width(100),
                            GUILayout.Height(100));
                    }

                    var expressionNames = new string[currLine.Speaker.Expressions.Count + 1];
                    expressionNames[0] = "<No change>";

                    for (int j = 1; j < currLine.Speaker.Expressions.Count + 1; j++)
                    {
                        expressionNames[j] = currLine.Speaker.Expressions[j - 1].Name;
                    }

                    currLine.ExpressionNumber = EditorGUILayout.IntPopup(currLine.ExpressionNumber,
                        expressionNames, dia.getExpressionIntValues(currLine.Speaker), GUILayout.Width(100));

                    if (currLine.ExpressionNumber > 0)
                    {
                        currLine.Expression =
                            currLine.Speaker.Expressions[currLine.ExpressionNumber - 1];
                    }
                    else
                    {
                        currLine.Expression = new Expression();
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(108),
                    GUILayout.Height(100));
            }
        }
        private void EditDialogueText(int i)
        {
			#region Column 3 Text and SFX
			//Column 3 Text
			if (i >= propLines.arraySize)
				return;
			using (new EditorGUILayout.VerticalScope())
			{
				var propLine = propLines.GetArrayElementAtIndex(i);
				var propLineText = propLine.FindPropertyRelative("Text");
				propLineText.stringValue = EditorGUILayout.TextArea(propLineText.stringValue, GUILayout.Height(62));

				using (new EditorGUILayout.HorizontalScope())
				{
					string name = "Voice FX:";
					EditorGUILayout.LabelField(name, GUILayout.Width(name.Length * 7));

					var vfxProperty = propLines.GetArrayElementAtIndex(i).FindPropertyRelative("VoiceSFX");
					EditorGUIUtility.labelWidth = 1;
					EditorGUILayout.PropertyField(vfxProperty, new GUIContent(""));
				}

				using (new EditorGUILayout.HorizontalScope())
				{
					dia.Lines[i].MusicChange = HandyFields.Option(dia.Lines[i].MusicChange, "Change Music", 90);
				}

				if (dia.Lines[i].MusicChange)
				{
					var musicProperty = propLines.GetArrayElementAtIndex(i).FindPropertyRelative("Music");
					EditorGUIUtility.labelWidth = 1;
					EditorGUILayout.PropertyField(musicProperty, new GUIContent(""), GUILayout.Width(270));
				}

			}
			
			#endregion
		}
		private void EditDialoguePosition(int i)
        {
            GUI.backgroundColor = dia.Color;

            using (new EditorGUILayout.VerticalScope())
            {
                if (dia.Lines.Count > 1)
                {
                    if (GUILayout.Button("-", GUILayout.Width(20)) && dia.Lines.Count > 1)
                    {
                        GUI.FocusControl(null);
                        dia.Lines.Remove(dia.Lines[i]);
                    }
                }

                if (i > 0)
                {
                    if (GUILayout.Button("ʌ", GUILayout.Width(20)))
                    {
                        GUI.FocusControl(null);
                        dia.Lines.Swap(i, i - 1);
                    }
                }

                if (i < dia.Lines.Count - 1)
                {
                    if (GUILayout.Button("v", GUILayout.Width(20)))
                    {
                        GUI.FocusControl(null);
                        dia.Lines.Swap(i, i + 1);
                    }
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    dia.Lines.Insert(i + 1, new TrialLine());
                }
            }
        }
        private void EditFooter()
        {
			//Variable from dialogue usually here
			if (dia.DirectTo.Enabled)
			{
				using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(400)))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						GUILayout.Label("Next Dialogue:", GUILayout.Width(90));

						if (dia.DirectTo.NewTrialDialogue == null)
						{
							GUI.backgroundColor = Color.cyan;
						}

						dia.DirectTo.NewTrialDialogue =
							(TrialDialogue)EditorGUILayout.ObjectField(dia.DirectTo.NewTrialDialogue, typeof(TrialDialogue), false,
								GUILayout.Width(180));
						GUI.backgroundColor = dia.Color;

						if (dia.DirectTo.NewTrialDialogue != null)
						{
							string[] nextDialogueTexts = new string[dia.DirectTo.NewTrialDialogue.Lines.Count];
							int[] optValues = new int[dia.DirectTo.NewTrialDialogue.Lines.Count];

							for (int j = 0; j < dia.DirectTo.NewTrialDialogue.Lines.Count; j++)
							{
								nextDialogueTexts[j] = dia.DirectTo.NewTrialDialogue.Lines[j].Text;
								optValues[j] = value;
								value++;
							}


							dia.DirectTo.NewDialogueIndex = EditorGUILayout.IntPopup(
								dia.DirectTo.NewDialogueIndex, nextDialogueTexts, optValues, GUILayout.Width(100));

						}

					}
					value = 0;

					if (GUILayout.Button("Remove", GUILayout.Width(80)))
					{
						dia.DirectTo.NewTrialDialogue = null;
						dia.DirectTo.Enabled = false;

					}
				}
			}

			if (dia.SceneTransition.Enabled)
			{
				using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(400)))
				{
					using (new EditorGUILayout.VerticalScope())
					{
						GUILayout.Label("Name of next Scene:", GUILayout.Width(130));
						using (new EditorGUILayout.HorizontalScope())
						{
							dia.SceneTransition.Scene = GUILayout.TextField(dia.SceneTransition.Scene);
						}
						using (new EditorGUILayout.HorizontalScope())
						{
							dia.SceneTransition.ToDark = HandyFields.Option(dia.SceneTransition.ToDark, "To Menu: ");
						}
						using (new EditorGUILayout.HorizontalScope())
						{
							dia.SceneTransition.OnLoadNoDark = HandyFields.Option(dia.SceneTransition.OnLoadNoDark, "OnLoadNoDark: ", 100);
						}
						dia.SceneTransition.AtEnd = HandyFields.UnityField(dia.SceneTransition.AtEnd, 130, 30);
					}
					if (GUILayout.Button("Remove", GUILayout.Width(80)))
					{
						dia.SceneTransition.Scene = string.Empty;
						dia.SceneTransition.Enabled = false;

					}
				}
			}

			if (dia.FlagTrigger.Enabled)
			{
				using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(400)))
				{
					using (new EditorGUILayout.VerticalScope())
					{
						GUILayout.Label("Trigger Flag: ");
						dia.FlagTrigger.Chapter = HandyFields.IntField("Chapter: ", dia.FlagTrigger.Chapter);
						int tmpChapter = dia.FlagTrigger.Chapter;
						if (progression != null && progression.Chapters.Count != 0)
						{

							if (dia.FlagTrigger.Chapter < 0 || dia.FlagTrigger.Chapter > progression.Chapters.Count)
							{
								dia.FlagTrigger.Chapter = 0;
							}

							Chapter chapter = progression.Chapters[dia.FlagTrigger.Chapter];

							if (chapter.Objectives.Count > 0 && dia.FlagTrigger.Objective < chapter.Objectives.Count)
							{

								dia.FlagTrigger.Objective = EditorGUILayout.Popup(dia.FlagTrigger.Objective, chapter.GetObjectives());
								//Debug.Log(dia.FlagTrigger.Objective);
								//Debug.Log(chapter.Objectives.Count);
								Objective o = chapter.Objectives[dia.FlagTrigger.Objective];

								if (dia.FlagTrigger.Flags.Count != 0 && o.Flags.Count != 0)
								{
									if (dia.FlagTrigger.Flags.Count <= o.Flags.Count)
									{
										string[] flagNames = o.GetFlagNames();
										for (int i = 0; i < dia.FlagTrigger.Flags.Count; i++)
										{
											if (dia.FlagTrigger.Flags[i] > o.Flags.Count)
											{
												dia.FlagTrigger.Flags[i] = o.Flags.Count - 1;
											}
											using (new EditorGUILayout.HorizontalScope())
											{
												dia.FlagTrigger.Flags[i] = EditorGUILayout.Popup(dia.FlagTrigger.Flags[i], flagNames);
												if (GUILayout.Button("Remove Flag"))
												{
													dia.FlagTrigger.Flags.Remove(dia.FlagTrigger.Flags[i]);
												}
											}
										}
									}
									else
									{
										dia.FlagTrigger.Flags.Remove(dia.FlagTrigger.Flags.Count - 1);
									}

								}
								if (o.Flags.Count == 0)
								{
									GUILayout.Label("The Current Objective does not have any" +
										" flags, add some to continue setup.");
								}
								else
								{
									if (GUILayout.Button("Add Flag"))
									{
										dia.FlagTrigger.Flags.Add(0);
									}
								}
							}
							else
							{
								GUILayout.Label("The set objective is either too high or too low so \n I'm setting it" +
									" to the last available objective for the chapter, \n" +
									"If you're still seeing this message it means the chapter has no \n" +
									"objectives and needs to have one, or try pressing the remove button and try again.");
								dia.FlagTrigger.Objective = chapter.Objectives.Count - 1;
							}
						}
						else
						{
							GUILayout.Label("Set up a ProgressionDatabase or add some chapters to it " +
								"to use ProgressionDatabase functionality.");
						}
					}
					if (GUILayout.Button("Remove", GUILayout.Width(80)))
					{
						dia.FlagTrigger.Enabled = false;
						dia.FlagTrigger.Objective = 0;
						dia.FlagTrigger.Flags.Clear();
					}
				}
			}

			if (dia.EndVidEnabled)
			{
				using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(400)))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						dia.EndVideo = HandyFields.UnityField(dia.EndVideo);
					}
					if (GUILayout.Button("Remove", GUILayout.Width(80)))
					{
						dia.EndVidEnabled = false;
					}
				}
			}

			#region End Event Buttons
			using (new EditorGUILayout.HorizontalScope())
			{
				if (!dia.Variable.Enabled)
				{
					// was: dia.Choices.Count == 0 && !dia.Variable.Enabled && !dia.DirectTo.Enabled && !dia.SceneTransition.Enabled
					if (dia.Speakers == null)
					{
						if (!(dia.Speakers != null))
						{
							dia.Speakers = Resources.Load<CharacterDatabase>("Characters/CharacterDatabase");
						}
						else
						{
							using (new EditorGUILayout.VerticalScope())
							{
								EditorGUILayout.LabelField("CharacterDatabase is not set.");
								EditorGUILayout.LabelField("Create a CharacterDatabase in Resources/Characters/CharacterDatabase.asset");

							}
						}
					}
					else
					{
						if (GUILayout.Button("New Line", GUILayout.Width(100)))
						{
							dia.Lines.Add(new TrialLine());
						}
					}



					if (dia.Lines.Count > 0)
					{
						if (!dia.DirectTo.Enabled && GUILayout.Button("Direct to...", GUILayout.Width(100)))
						{
							dia.DirectTo.Enabled = true;
						}
						if (GUILayout.Button("Add Condition", GUILayout.Width(100)))
						{
							dia.Variable.Enabled = true;
						}
					}

				}
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				if (dia.Lines.Count > 0)
				{
					if (!dia.SceneTransition.Enabled && GUILayout.Button("Enter Scene", GUILayout.Width(100)))
					{
						dia.SceneTransition.Enabled = true;
					}

					if (!dia.FlagTrigger.Enabled)
					{
						if (GUILayout.Button("Trigger Flag", GUILayout.Width(100)))
						{
							dia.FlagTrigger.Enabled = true;
						}
					}

					if (!dia.EndVidEnabled)
					{
						if (GUILayout.Button("End Video", GUILayout.Width(100)))
						{
							dia.EndVidEnabled = true;
						}
					}
				}
			}
			#endregion

        }
		void EditLineLower(TrialLine currentLine, int i)
        {
			GUILayout.Space(5f);

			using (new EditorGUILayout.VerticalScope())
			{
				if (dia.Lines[i].co.enabled)
                {
					EditTCO(dia.Lines[i], dia.Lines[i].co);
				}
				else if (GUILayout.Button("Add Camera Override", GUILayout.Width(170)))
                {
					dia.Lines[i].co.enabled = true;
					dia.Lines[i].co.distance = 8.11f;
					if (currentLine.Speaker != null)
					{
						dia.Lines[i].co.seatFocus = currentLine.Speaker.TrialPosition;
						dia.Lines[i].co.height = currentLine.Speaker.TrialHeight;
					}
				}
			}

			using (new EditorGUILayout.VerticalScope())
			{
				if (dia.Lines[i].SFX != null)
				{
					var sfxListProperty = propLines.GetArrayElementAtIndex(i).FindPropertyRelative("SFX");
					EditorGUIUtility.labelWidth = 1;
					//UnityEngine.Debug.Log(sfxListProperty.propertyType);
					EditorGUILayout.PropertyField(sfxListProperty, new GUIContent(""), GUILayout.MinWidth(270));
				}
			}

			#region Dialogue Events
			//DialogueEvents Extension
			using (new EditorGUILayout.VerticalScope())
			{
				if (currentLine.DiaEvents != null)
				{
					using (new EditorGUILayout.VerticalScope("Box"))
					{
						int _errors = ErrorCheck(currentLine.DiaEvents);
						GUIStyle _diastyle = new GUIStyle();
						_diastyle.padding = new RectOffset(20, 20, 0, 0);

						//Check for errors in Dialogue Events
						if (_errors > 0)
						{
							currentLine.ShowDiaEvents = EditorGUILayout.BeginFoldoutHeaderGroup(currentLine.ShowDiaEvents,
								new GUIContent($"Dialogue Events ({currentLine.DiaEvents.Count}) Errors: {_errors}",
								"Show list of Dialogue Events to be invoked at this line."));
						}
						else
						{
							currentLine.ShowDiaEvents = EditorGUILayout.BeginFoldoutHeaderGroup(currentLine.ShowDiaEvents,
								new GUIContent($"Dialogue Events ({currentLine.DiaEvents.Count})",
								"Show list of Dialogue Events to be invoked at this line."));
						}

						if (currentLine.ShowDiaEvents)
						{
							var eventListProperty = propLines.GetArrayElementAtIndex(i).FindPropertyRelative("DiaEvents");

							for (int d = 0; d < currentLine.DiaEvents.Count; d++)
							{
								EditorGUILayout.BeginHorizontal("Box");
								//Debug.Log(eventListProperty.GetArrayElementAtIndex(d).type);
								using (new EditorGUILayout.VerticalScope())
								{
									if (currentLine.DiaEvents[d] != null)
									{
										EditorGUILayout.LabelField(currentLine.DiaEvents[d]?.GetType().Name, EditorStyles.largeLabel);
										HandyFields.UISplitter(Color.gray);

										currentLine.DiaEvents[d]?.EditorUI(eventListProperty.GetArrayElementAtIndex(d));
										GUILayout.Space(5);
										currentLine.DiaEvents[d]?.ShowHelpBox();
									}
									else
									{
										EditorGUILayout.HelpBox("Unsupported/Null Dialogue Event.\n" +
										  "This may have been broken by an update. Please delete this using the - button on the right side of this error " +
										  "and reapply your event. If still isn't fixed, post an issue on GitHub.", MessageType.Error, true);
									}
								}
								using (new EditorGUILayout.VerticalScope(GUILayout.Width(20)))
								{
									if (GUILayout.Button(new GUIContent("-", "Delete Dialogue Event.\nWarning: this is non-recoverable. Using undo won't do a thing."), GUILayout.Width(20)))
									{
										currentLine.DiaEvents.Remove(currentLine.DiaEvents[d]);
										Repaint();
										continue;
									}

									if (currentLine.DiaEvents[d] != null) //Kinda sucks that I'm forced to do nested ifs here.
									{
										if (GUILayout.Button(new GUIContent("?", "Show Help"), GUILayout.Width(20)))
										{
											currentLine.DiaEvents[d].ToggleHelpBox();
										}
									}
								}

								EditorGUILayout.EndHorizontal();
							}

							EditorGUILayout.BeginHorizontal("Box");
							_DiaEventIndex = EditorGUILayout.Popup(_DiaEventIndex, choices);
							if (GUILayout.Button("Add Dialogue Event"))
							{
								currentLine.DiaEvents.Add((IDialogueEvent)Activator.CreateInstance(_list[_DiaEventIndex]));
							}
							EditorGUILayout.EndHorizontal();
						}
						EditorGUILayout.EndFoldoutHeaderGroup();
					}
				}
			}

			#endregion

		}

        #region Trial Camera Overrides
        float tcoMinWidth = 200;
		TCODatabase presetbase;
		int presetNum = 0;
		string presetName;
		private void EditTCO(TrialLine line, TCO o)
        {
			if (GUILayout.Button("X", GUILayout.Width(20)))
			{
				o.Clear();
			}

			using (new EditorGUILayout.HorizontalScope())
            {
				using (new GUILayout.VerticalScope("Box", GUILayout.MinWidth(120)))
				{
					using (new EditorGUILayout.HorizontalScope(GUILayout.MinWidth(tcoMinWidth)))
					{
						if (line == null || !line.DontPan)
						{
							o.seatFocus = HandyFields.FloatField("SeatFocus: ", o.seatFocus, 30, 7);
							if (line != null && GUILayout.Button("Use Char", GUILayout.Width(100)))
							{
								if (line.Speaker != null)
								{
									o.seatFocus = line.Speaker.TrialPosition;
								}
							}
						}
						else if (o.seatFocus != 0)
							o.seatFocus = 0;
					}
					using (new EditorGUILayout.HorizontalScope(GUILayout.MinWidth(tcoMinWidth)))
					{
						o.distance = HandyFields.FloatField("Distance: ", o.distance, 30);
						if (GUILayout.Button("Use Default", GUILayout.Width(100)))
						{
							o.distance = 8.11f;
						}
					}
					using (new EditorGUILayout.HorizontalScope(GUILayout.MinWidth(tcoMinWidth)))
					{
						o.height = HandyFields.FloatField("Height: ", o.height, 30);
						if (line != null && GUILayout.Button("Use Char", GUILayout.Width(100)))
						{
							if (line.Speaker != null)
							{
								o.height = line.Speaker.TrialHeight;
							}
						}
					}
				}

				using (new GUILayout.VerticalScope("Box"))
				{
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.Label("Preset Base:", GUILayout.MaxWidth(100));
						presetbase = HandyFields.UnityField(presetbase, 170, 20);
						
					}
					if (presetbase != null)
					{
						if (presetbase.Presets.Count != 0)
                        {
							presetNum = EditorGUILayout.IntPopup(presetNum, presetbase.GetNames(), presetbase.GetInts());
							if (GUILayout.Button("Apply"))
							{
								TCO preset = presetbase.Presets[presetNum].Preset;
								o.seatFocus = preset.seatFocus;
								o.distance = preset.distance;
								o.height = preset.height;
							}
						}
						using (new GUILayout.HorizontalScope())
						{
							GUILayout.Label("New Preset Name:", GUILayout.MaxWidth(125));
							presetName = EditorGUILayout.TextField(presetName);
						}
						if (GUILayout.Button("Create New Preset", GUILayout.MaxWidth(150)))
						{
							presetbase.Presets.Add(new TCOPreset(presetName, new TCO(o.seatFocus, o.height, o.distance)));
						}
					}
				}
			}
		}
        #endregion

        private int GetProgressionDatabase()
		{
			string[] _databaseguids = AssetDatabase.FindAssets("t:ProgressionDatabase");
			if (_databaseguids.Length == 1)
			{
				string _databasepath = AssetDatabase.GUIDToAssetPath(_databaseguids[0]);

				EditorGUILayout.LabelField($"Loading ProgressionDatabase at {_databasepath}.");
				progression = AssetDatabase.LoadAssetAtPath<ProgressionDatabase>(_databasepath);
				return 1;
			}
			else if (_databaseguids.Length > 1)
			{
				using (new EditorGUILayout.VerticalScope())
				{
					EditorGUILayout.LabelField("There are more than one ProgressionDatabase Assets found in your project.");
					EditorGUILayout.Space(10);
					EditorGUILayout.LabelField("Here are the path of all the files:");
					for (int i = 0; i < _databaseguids.Length; i++)
					{
						string _path = AssetDatabase.GUIDToAssetPath(_databaseguids[i]);
						EditorGUILayout.LabelField($"	• {_path}");
					}
					EditorGUILayout.Space(10);
					EditorGUILayout.LabelField("Only one ProgressionDatabase is allowed. Please delete all the duplicates until one remains.");
				}
				return 2;
			}
			else
			{
				using (new EditorGUILayout.VerticalScope())
				{
					EditorGUILayout.LabelField("There is no ProgressionDatabase Asset found in your project.");
					EditorGUILayout.LabelField("Please create one by right clicking in the Project Window and navigating to: \n[Create > DREditor > Progression > Progression Database]", GUILayout.Height(50));
					EditorGUILayout.LabelField("Do not create more than one. Only one CharacterDatabase is allowed.");
				}
				return 0;
			}
		}

		private int ErrorCheck(List<IDialogueEvent> events)
		{
			int _foundErrors = 0;
			for (int d = 0; d < events.Count; d++)
			{
				if (events[d] == null) _foundErrors++;
			}
			return _foundErrors;
		}
		/// <summary>
		/// For any Tools that might help during development
		/// </summary>
		void DebugMode() //*
		{
			DebugCharacterSprites();
			if (GUILayout.Button("Remove top half of Lines"))
				SplitDialogue(true);
			if (GUILayout.Button("Remove bottom half of Lines"))
				SplitDialogue(false);
			if (GUILayout.Button("Randomize Camera Anims"))
				RandomizeCamAnim();
			DebugApplyTCOToChar();
		}

		#region Test Every Sprite on a Character
		/// <summary>
		/// Rewrites the dialogue to contain a line for every sprite for the character on the
		/// first line.
		/// </summary>
		void DebugCharacterSprites() //*
		{
			if (GUILayout.Button("Test First Line Characters Sprites", GUILayout.Width(250)))
			{
				if (dia.Lines.Count > 0 && dia.Lines[0].Speaker != null && dia.Lines[0].Speaker.Expressions.Count > 0)
				{
					if (EditorUtility.DisplayDialog("Make dialogue have line of every sprite?",
						"WARNING!: This will rewrite all lines of the dialogue asset! " +
						"Are you sure you want to continue?", "Yes", "No"))
					{
						Character character = dia.Lines[0].Speaker;
						int speakerNum = dia.Lines[0].SpeakerNumber;
						for (int i = dia.Lines.Count - 1; i > 0; i--) // Make it just one line
						{
							if (dia.Lines.Count == 1)
								break;

							dia.Lines.Remove(dia.Lines[i]);
						}

						for (int i = 0; i < character.Expressions.Count; i++)
						{
							if (i == 0)
							{
								dia.Lines[i].ExpressionNumber = i + 1;
								dia.Lines[i].Expression = character.Expressions[i];
								dia.Lines[i].Text = dia.Lines[i].Expression.Name;
								continue;
							}
							dia.Lines.Add(new TrialLine());
							dia.Lines[i].SpeakerNumber = speakerNum;
							dia.Lines[i].ExpressionNumber = i + 1;
							dia.Lines[i].Expression = character.Expressions[i];
							dia.Lines[i].Text = dia.Lines[i].Expression.Name;
						}
						debugMode = false;
					}
				}
				else
				{
					Debug.LogWarning("Please Add a line, add a Speaker to the first line, " +
						"or add expressions the the character.");
				}

			}
		}
		#endregion

		#region Split Dialogue

		void SplitDialogue(bool top)
		{
			if (top)
				dia.Lines.RemoveRange(0, dia.Lines.Count / 2);
			else
				dia.Lines.RemoveRange(dia.Lines.Count / 2, dia.Lines.Count - dia.Lines.Count / 2);
			debugMode = false;
		}

		#endregion
		#region Apply TCO to all lines with this character
		bool applytcoDebug = false;
		TCO coDebug;
		Character debugSpeaker = null;
		int SpeakerNumber = 0;
		void DebugApplyTCOToChar()
        {
			if (!applytcoDebug && GUILayout.Button("Apply TCO to All of a Character", GUILayout.Width(200)))
				applytcoDebug = true;
			if (applytcoDebug)
            {
				if (GUILayout.Button("Exit", GUILayout.MaxWidth(50)))
					applytcoDebug = false;
				
				
				var prependedArray = ContainerUtil.PrependedList(dia.GetCharacterNames(), "<No Character>");
				SpeakerNumber = EditorGUILayout.IntPopup(SpeakerNumber, prependedArray, ContainerUtil.Iota(prependedArray.Length, -1), GUILayout.Width(130));
				debugSpeaker = SpeakerNumber == -1 ? null : dia.Speakers.Characters[SpeakerNumber];
				if (coDebug == null)
					coDebug = new TCO();
				coDebug.enabled = true;
				EditTCO(null, coDebug);
				if (GUILayout.Button("Apply", GUILayout.MaxWidth(50)))
					DebugSetTCOToChar();
			}
		}
		void DebugSetTCOToChar()
        {
			foreach(TrialLine l in dia.Lines)
            {
				if (l.Speaker == debugSpeaker)
                {
					l.co = coDebug;
                }
            }
			debugMode = false;
        }
		#endregion
	}
}
#endif