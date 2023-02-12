using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

using DREditor.Characters;
using DREditor.Utility;
using DREditor.Utility.Editor;
using DREditor.EventObjects;
using DREditor.Dialogues.Events;

namespace DREditor.Dialogues.Editor
{
	public class DialogueEditorBase : UnityEditor.Editor
	{
		protected int value;
		protected bool _directDialogue;
		protected AudioClip _sfx = null;

		public bool IsProtagonist(Character character) => character is Protagonist;
	}

	[CustomEditor(typeof(Dialogue))]
	public class DialogueEditor : DialogueEditorBase
	{
		Dialogue dia;

		SerializedProperty propLines;

		private int lineEditNum = -1;
		private List<bool> _lineEdit = new List<bool>();
		private List<Rect> _lineEditRect = new List<Rect>();
		private List<Type> _list;
		private string[] choices;

		private int _DiaEventIndex;
		private bool _showDiaEvents;
		private bool isMouseOnSprite;
		private int lineToEnlarge;

		public void OnEnable()
		{
			dia = (Dialogue)target;
			propLines = serializedObject.FindProperty("Lines");

			UpdateDiaEventList();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			CreateForm();

			if (isMouseOnSprite)
			{
				ShowEnlargedSprite(lineToEnlarge, 1000f, 400f);
				isMouseOnSprite = false;
			}

			EditorUtility.SetDirty(dia);
			serializedObject.ApplyModifiedProperties();
		}

		void CreateForm()
		{
			if (dia.Speakers == null)
			{
				int _assetamount = GetCharacterDatabase();

				if (_assetamount != 1) return;
			}

			if (dia.Speakers.Characters == null)
			{
				EditorGUILayout.LabelField("Add at least one character in the CharacterDatabase.");
				return;
			}

			if (dia.Speakers.Characters.Count == 0)
			{
				EditorGUILayout.LabelField("Add at least one character in the CharacterDatabase.");
				return;
			}

			if (dia.Speakers.Characters.Count > 0)
			{
				foreach (var stu in dia.Speakers.Characters)
				{
					if (stu == null)
					{
						EditorGUILayout.LabelField("Nullref in CharacterDatabase. Is an element empty?");
						return;
					}
				}
			}

			EditorStyles.textArea.wordWrap = true;
			EditorStyles.textField.wordWrap = true;

			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField(dia.DialogueName, EditorStyles.boldLabel);
				dia.Color = EditorGUILayout.ColorField(dia.Color, GUILayout.Width(50));
			}

			using (new EditorGUILayout.VerticalScope("Box"))
			{
				GUI.backgroundColor = dia.Color;
				dia.DialogueMode = (BoxMode)EditorGUILayout.EnumPopup("Dialogue Mode:", dia.DialogueMode);
				using (new EditorGUILayout.HorizontalScope())
				{
					if (dia.DialogueName == "") dia.DialogueName = target.name;
					dia.DialogueName = EditorGUILayout.DelayedTextField("Dialogue Code: ", dia.DialogueName);

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
				}
				
				if (dia.DialogueMode == BoxMode.Normal)
				{
					dia.ShowDialoguePanel = EditorGUILayout.Toggle("Show Dialogue Panel: ", dia.ShowDialoguePanel);
				}

				GUI.backgroundColor = Color.white;
			}

			_showDiaEvents = ShowDialogueEventList(dia.StartEvents, _showDiaEvents, "Start Events");

			if (dia.Lines != null)
			{
				foreach (Line l in dia.Lines)
				{
					_lineEdit.Add(false);
					_lineEditRect.Add(new Rect());
				}

				for (int i = 0; i < dia.Lines.Count; i++)
				{
					Rect rect = new Rect();
					var currentLine = dia.Lines[i];
					var color = Color.white;

					if (currentLine.Speaker is Student)
					{
						var stu = currentLine.Speaker as Student;

						float H, S, V;

						color = stu.StudentCard.Color;
						Color.RGBToHSV(color, out H, out S, out V);
						S = EditorGUIUtility.isProSkin? 0.2f : 0.3f;
						V = EditorGUIUtility.isProSkin? 1f : 0.95f;

						color = Color.HSVToRGB(H, S, V);
					}
					else
					{
						color = Color.white;
					}

					if (currentLine.SpeakerNumber != 0)
					{
						if (EditorGUIUtility.isProSkin)
						{
							GUI.backgroundColor = Color.white;
							GUI.color = color;
						}
						else
						{
							GUI.backgroundColor = color;
							GUI.color = Color.white;
						}
					}
					else
					{
						if (EditorGUIUtility.isProSkin)
						{
							GUI.color = dia.Color;
						}
						else
						{
							GUI.backgroundColor = color;
							GUI.color = Color.white;
						}
					}

					using (new GUILayout.HorizontalScope("Box"))
					{
						using (new EditorGUILayout.VerticalScope())
						{
							//MainBox
							using (new EditorGUILayout.HorizontalScope())
							{
                                #region Preview
                                if (lineEditNum != i)
								{
									//Column 1
									if (currentLine.Expression != null)
									{
										using (new EditorGUILayout.VerticalScope())
										{
											GUIStyle expr = new GUIStyle();

											if (currentLine.Expression.Sprite != null && currentLine.ExpressionNumber > 0)
											{
												var tex = Utility.Editor.HandyFields.GetMaterialTexture(currentLine.Expression.Sprite);
												if (tex)
												{
													expr.normal.background = tex;
												}
											}

											if (EditorGUIUtility.isProSkin) GUI.color = Color.white;
											else GUI.backgroundColor = Color.white;

											rect = new Rect(EditorGUILayout.GetControlRect(GUILayout.Width(130), GUILayout.Height(130)));
											EditorGUI.LabelField(rect, GUIContent.none, expr);

											if (EditorGUIUtility.isProSkin) GUI.color = color;
											else GUI.backgroundColor = color;
										}
									}

									//Column 2
									using (new EditorGUILayout.VerticalScope())
									{
										var propLine = propLines.GetArrayElementAtIndex(i);
										var propLineText = propLine.FindPropertyRelative("Text");
										StringBuilder sb = new StringBuilder();

										if (currentLine.Speaker)
										{
											if (currentLine.AliasNumber != 0)
											{
												sb.Append(currentLine.AliasNumber != 0 ? $"{currentLine.Speaker.Aliases[currentLine.AliasNumber - 1].Name} ({currentLine.Speaker.FullName(false)})" : "");
											}
											else
											{
												sb.Append(currentLine.AliasNumber != 0 ? "" : $"{currentLine.Speaker.FullName(false)}");
											}

										}
										EditorGUILayout.LabelField(new GUIContent(string.IsNullOrWhiteSpace(sb.ToString()) ? "" : sb.ToString(), "Character Name"), GUILayout.Width(Screen.width - 220));
										TextPreview(propLineText.stringValue, GUILayout.Width(Screen.width - 210));
									}
								}
                                #endregion

                                #region Editor
                                else
                                {
									//Column 1
									using (new EditorGUILayout.VerticalScope())
									{
										var prependedArray = ContainerUtil.PrependedList(dia.GetCharacterNames(), "<No Character>");
										currentLine.SpeakerNumber = EditorGUILayout.IntPopup(currentLine.SpeakerNumber, prependedArray,
											ContainerUtil.Iota(prependedArray.Length, -1), GUILayout.Width(130));
										currentLine.Speaker = currentLine.SpeakerNumber == -1 ? null : dia.Speakers.Characters[currentLine.SpeakerNumber];
										if (currentLine.SpeakerNumber == -1)
										{
											currentLine.Speaker = null;
											currentLine.VoiceSFX = null;
											currentLine.Expression = null;
											currentLine.ExpressionNumber = 0;
											currentLine.AliasNumber = 0;
										}
										else
										{
											currentLine.Speaker = dia.Speakers.Characters[currentLine.SpeakerNumber];
										}

										if (currentLine.Speaker)
										{
											var aliasNames = new string[currentLine.Speaker.Aliases.Count + 1];
											aliasNames[0] = "(No Alias)";

											for (int j = 1; j < currentLine.Speaker.Aliases.Count + 1; j++)
											{
												aliasNames[j] = currentLine.Speaker.Aliases[j - 1].Name;
											}
											currentLine.AliasNumber = EditorGUILayout.IntPopup(currentLine.AliasNumber,
												aliasNames, dia.getAliasesIntValues(currentLine.Speaker),
													GUILayout.Width(130));
										}

										using (new EditorGUILayout.HorizontalScope())
										{
											GUILayout.Label("Voice", GUILayout.Width(35));

											currentLine.VoiceSFX = EditorGUILayout.ObjectField(currentLine.VoiceSFX, typeof(AudioClip), false, GUILayout.Width(65)) as AudioClip;

											if (GUILayout.Button(">", GUILayout.Width(20)))
											{
												PublicAudioUtil.PlayClip(currentLine.VoiceSFX);
											}
										}

										if (currentLine.SFX != null)
										{
											for (var j = 0; j < currentLine.SFX.Count; j++)
											{
												using (new EditorGUILayout.HorizontalScope())
												{
													EditorGUI.BeginDisabledGroup(currentLine.SFX[j] == null);

													if (GUILayout.Button(">", GUILayout.Width(20)))
													{
														PublicAudioUtil.PlayClip(currentLine.SFX[j]);
													}

													EditorGUI.EndDisabledGroup();

													currentLine.SFX[j] =
														(AudioClip)EditorGUILayout.ObjectField(currentLine.SFX[j], typeof(AudioClip), false,
															GUILayout.Width(84));

													if (GUILayout.Button("x", GUILayout.Width(20)))
													{
														currentLine.SFX.Remove(currentLine.SFX[j]);
													}
												}
											}
										}

										if (GUILayout.Button("Add Sound", GUILayout.Width(130)))
										{
											currentLine.SFX.Add(_sfx);
										}

										if (currentLine.Events != null)
										{
											for (var j = 0; j < currentLine.Events.Count; j++)
											{
												using (new EditorGUILayout.HorizontalScope())
												{
													currentLine.Events[j] = (SceneEvent)EditorGUILayout.ObjectField(currentLine.Events[j], typeof(SceneEvent), false, GUILayout.Width(100));
													if (GUILayout.Button("x", GUILayout.Width(20)))
													{
														currentLine.Events.Remove(currentLine.Events[j]);
													}
												}
											}
										}

										if (GUILayout.Button("Add Scene Event", GUILayout.Width(130)))
										{
											currentLine.Events.Add(CreateInstance<SceneEvent>());
										}

										using (new EditorGUILayout.HorizontalScope())
										{
											GUILayout.Label("Auto:", GUILayout.Width(55));
											currentLine.AutomaticLine = EditorGUILayout.Toggle(currentLine.AutomaticLine, GUILayout.Width(15));
											if (currentLine.AutomaticLine)
											{
												currentLine.TimeToNextLine = EditorGUILayout.FloatField(currentLine.TimeToNextLine, GUILayout.Width(24));
												EditorGUILayout.LabelField("sec", GUILayout.Width(22));
											}
										}
									}

									//Column 2
									if (currentLine.Speaker != null && !IsProtagonist(currentLine.Speaker))
									{
										using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(100)))
										{
											var exprs = currentLine.Speaker.Expressions.Count;

											if (exprs < currentLine.ExpressionNumber)
											{
												currentLine.ExpressionNumber = 0;
											}

											var expressionNames = new string[currentLine.Speaker.Expressions.Count + 1];
											expressionNames[0] = "<No change>";

											for (int j = 1; j < currentLine.Speaker.Expressions.Count + 1; j++)
											{
												expressionNames[j] = currentLine.Speaker.Expressions[j - 1].Name;
											}

											if (currentLine.Expression != null)
											{
												GUIStyle expr = new GUIStyle();

												if (currentLine.Expression.Sprite != null && currentLine.ExpressionNumber > 0)
												{
													var tex = HandyFields.GetMaterialTexture(currentLine.Expression.Sprite);
													if (tex)
													{
														expr.normal.background = tex;
													}
												}

												if (EditorGUIUtility.isProSkin) GUI.color = Color.white;
												else GUI.backgroundColor = Color.white;

												rect = new Rect(EditorGUILayout.GetControlRect(GUILayout.Width(100), GUILayout.Height(100)));
												EditorGUI.LabelField(rect, GUIContent.none, expr);

												if (expr.normal.background != null && rect.Contains(Event.current.mousePosition))
												{
													isMouseOnSprite = true;
													lineToEnlarge = i;
												}
												else
												{
													if (isMouseOnSprite != true) isMouseOnSprite = false;
												}

												if (EditorGUIUtility.isProSkin) GUI.color = color;
												else GUI.backgroundColor = color;
											}

											currentLine.ExpressionNumber = EditorGUILayout.IntPopup(currentLine.ExpressionNumber,
												expressionNames, dia.getExpressionIntValues(currentLine.Speaker), GUILayout.Width(100));

											if (currentLine.ExpressionNumber > 0)
											{
												currentLine.Expression =
													currentLine.Speaker.Expressions[currentLine.ExpressionNumber - 1];
											}
											else
											{
												currentLine.Expression = new Expression();
											}
										}
									}

									//Column 3
									using (new EditorGUILayout.VerticalScope())
									{
										var propLine = propLines.GetArrayElementAtIndex(i);
										var propLineText = propLine.FindPropertyRelative("Text");
										if (currentLine.Speaker != null && !IsProtagonist(currentLine.Speaker))
										{
											propLineText.stringValue = EditorGUILayout.TextArea(propLineText.stringValue, GUILayout.Height(62), GUILayout.Width(Screen.width - 320));
											TextPreview(propLineText.stringValue, GUILayout.Height(62), GUILayout.Width(Screen.width - 320));
										}
										else
										{
											propLineText.stringValue = EditorGUILayout.TextArea(propLineText.stringValue, GUILayout.Height(62), GUILayout.Width(Screen.width - 210));
											TextPreview(propLineText.stringValue, GUILayout.Height(62), GUILayout.Width(Screen.width - 210));
										}
									}

									//Buttons Column
									using (new EditorGUILayout.VerticalScope())
									{
										if (dia.Lines.Count > 1)
										{
											if (GUILayout.Button(new GUIContent("-", "Delete this Line."), GUILayout.Width(20)) && dia.Lines.Count > 1)
											{
												GUI.FocusControl(null);
												dia.Lines.Remove(currentLine);
												serializedObject.ApplyModifiedProperties();
												serializedObject.Update();
												return;
											}
										}
										else
										{
											GUILayout.Space(20f);
										}

										if (i > 0)
										{
											if (GUILayout.Button(new GUIContent("ʌ", "Move this line up."), GUILayout.Width(20)) && i > 0)
											{
												{
													GUI.FocusControl(null);
													var line = dia.Lines[i - 1];

													dia.Lines[i - 1] = dia.Lines[i];
													dia.Lines[i] = line;
												}
											}
										}
										else
										{
											GUILayout.Space(20f);
										}

										if (i < dia.Lines.Count - 1)
										{
											if (GUILayout.Button(new GUIContent("v", "Move this line down."), GUILayout.Width(20)))
											{
												GUI.FocusControl(null);
												var line = dia.Lines[i + 1];

												dia.Lines[i + 1] = dia.Lines[i];
												dia.Lines[i] = line;
											}
										}
										else
										{
											GUILayout.Space(20f);
										}

										GUILayout.Space(25f);
										GUILayout.FlexibleSpace();

										if (GUILayout.Button(new GUIContent("*", "Duplicate this Line below."), GUILayout.Width(20)))
										{
											Line Copy = new Line();

											Copy.translationKey = currentLine.translationKey;
											Copy.SpeakerNumber = currentLine.SpeakerNumber;
											Copy.Text = currentLine.Text;
											Copy.VoiceSFX = currentLine.VoiceSFX;
											Copy.SFX = currentLine.SFX;
											Copy.Events = currentLine.Events;
											Copy.DiaEvents = new List<IDialogueEvent>();

											foreach (IDialogueEvent ev in currentLine.DiaEvents)
											{
												Copy.DiaEvents.Add(ev);
											}

											Copy.TimeToNextLine = currentLine.TimeToNextLine;
											Copy.AutomaticLine = currentLine.AutomaticLine;
											Copy.Expression = currentLine.Expression;
											Copy.ExpressionNumber = currentLine.ExpressionNumber;
											Copy.AliasNumber = currentLine.AliasNumber;

											dia.Lines.Insert(i + 1, Copy);
											serializedObject.Update();
										}

										if (GUILayout.Button(new GUIContent("+", "Add a new Line below."), GUILayout.Width(20)))
										{
											dia.Lines.Insert(i + 1, NewLine());
											serializedObject.Update();
										}

										GUILayout.FlexibleSpace();
									}
								}
                                #endregion
							}

							//DialogueEvents Extension
							if (lineEditNum == i)
							{
								GUILayout.Space(5f);
								currentLine.ShowDiaEvents = ShowDialogueEventList(currentLine.DiaEvents, currentLine.ShowDiaEvents);
							}
						}
					}
					_lineEditRect[i] = GUILayoutUtility.GetLastRect();

					if (Event.current.type == EventType.MouseDown && _lineEditRect[i].Contains(Event.current.mousePosition))
					{
						lineEditNum = i;
						Repaint();
					}

					GUI.color = Color.white;
					HandyFields.UISplitter(dia.Color, 1.5f, 0, 5);
				}
			}

			if (dia.Choices != null)
			{
				for (int i = 0; i < dia.Choices.Count; i++)
				{
					using (new EditorGUILayout.HorizontalScope("Box"))
					{
						GUILayout.Label("Choice " + (i + 1).ToString(), GUILayout.Width(80));

						dia.Choices[i].ChoiceText = EditorGUILayout.TextField(dia.Choices[i].ChoiceText, GUILayout.Width(Screen.width - 343));

						if (dia.Choices[i].NextDialogue == null)
						{
							GUI.backgroundColor = Color.cyan;
						}
						dia.Choices[i].NextDialogue = (Dialogue)EditorGUILayout.ObjectField(dia.Choices[i].NextDialogue,
													typeof(Dialogue),
													true,
													GUILayout.Width(100));

						GUI.backgroundColor = dia.Color;
						if (dia.Choices[i].NextDialogue != null)
						{
							string[] nextDialogueTexts = new string[dia.Choices[i].NextDialogue.Lines.Count];
							int[] optValues = new int[dia.Choices[i].NextDialogue.Lines.Count];

							for (int j = 0; j < dia.Choices[i].NextDialogue.Lines.Count; j++)
							{
								nextDialogueTexts[j] = dia.Choices[i].NextDialogue.Lines[j].Text;
								optValues[j] = value;
								value++;
							}

							value = 0;
							GUI.backgroundColor = Color.white;
							GUI.backgroundColor = dia.Choices[i].NextDialogue.Color;
							dia.Choices[i].NextIndexInDialogue = EditorGUILayout.IntPopup(dia.Choices[i].NextIndexInDialogue, nextDialogueTexts, optValues, GUILayout.Width(100));
						}
					}
				}

				GUI.backgroundColor = dia.Color;

				if (dia.Choices.Count > 0)
				{
					if (GUILayout.Button("Remove Choices", GUILayout.Width(130)))
					{
						dia.Choices.Clear();
					}
				}
			}

			if (dia.Variable.Enabled)
			{
				using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(400)))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						GUI.backgroundColor = dia.Color;
						EditorGUILayout.LabelField("If ", GUILayout.Width(80));

						dia.Variable.BoolVariable =
							(BoolWithEvent)EditorGUILayout.ObjectField(dia.Variable.BoolVariable, typeof(BoolWithEvent), false, GUILayout.Width(200));
					}
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("True: ", GUILayout.Width(80));

						if (dia.Variable.NextDialogueTrue == null)
						{
							GUI.backgroundColor = Color.cyan;
						}
						dia.Variable.NextDialogueTrue = (Dialogue)EditorGUILayout.ObjectField(dia.Variable.NextDialogueTrue, typeof(Dialogue), false, GUILayout.Width(200));

						GUI.backgroundColor = dia.Color;

						if (dia.Variable.NextDialogueTrue != null)
						{
							string[] nextDialogueTextsTrue = new string[dia.Variable.NextDialogueTrue.Lines.Count];
							int[] optValues2 = new int[dia.Variable.NextDialogueTrue.Lines.Count];

							for (int j = 0; j < dia.Variable.NextDialogueTrue.Lines.Count; j++)
							{
								nextDialogueTextsTrue[j] = dia.Variable.NextDialogueTrue.Lines[j].Text;
								optValues2[j] = value;
								value++;
							}
							dia.Variable.NextIndexInDialogueTrue = EditorGUILayout.IntPopup(
								dia.Variable.NextIndexInDialogueTrue, nextDialogueTextsTrue, optValues2, GUILayout.Width(100));
						}
					}
					value = 0;

					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("False: ", GUILayout.Width(80));

						if (dia.Variable.NextDialogueFalse == null)
						{
							GUI.backgroundColor = Color.cyan;
						}

						dia.Variable.NextDialogueFalse = (Dialogue)EditorGUILayout.ObjectField(dia.Variable.NextDialogueFalse, typeof(Dialogue), false, GUILayout.Width(200));

						GUI.backgroundColor = dia.Color;

						if (dia.Variable.NextDialogueFalse != null)
						{
							string[] nextDialogueTextsFalse = new string[dia.Variable.NextDialogueFalse.Lines.Count];
							int[] optValues3 = new int[dia.Variable.NextDialogueFalse.Lines.Count];

							for (int j = 0; j < dia.Variable.NextDialogueFalse.Lines.Count; j++)
							{
								nextDialogueTextsFalse[j] = dia.Variable.NextDialogueFalse.Lines[j].Text;
								optValues3[j] = value;
								value++;
							}

							dia.Variable.NextIndexInDialogueFalse = EditorGUILayout.IntPopup(
								dia.Variable.NextIndexInDialogueFalse, nextDialogueTextsFalse, optValues3, GUILayout.Width(100));
						}
					}
					value = 0;

					GUI.backgroundColor = dia.Color;

					if (GUILayout.Button("Remove Condition", GUILayout.Width(130)))
					{
						dia.Variable.BoolVariable = null;
						dia.Variable.Enabled = false;
					}
				}
			}

			if (dia.DirectTo.Enabled)
			{
				using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(400)))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						GUILayout.Label("Next Dialogue:", GUILayout.Width(90));

						if (dia.DirectTo.NewDialogue == null)
						{
							GUI.backgroundColor = Color.cyan;
						}

						dia.DirectTo.NewDialogue =
							(Dialogue)EditorGUILayout.ObjectField(dia.DirectTo.NewDialogue, typeof(Dialogue), false,
								GUILayout.Width(180));
						GUI.backgroundColor = dia.Color;

						if (dia.DirectTo.NewDialogue != null)
						{
							string[] nextDialogueTexts = new string[dia.DirectTo.NewDialogue.Lines.Count];
							int[] optValues = new int[dia.DirectTo.NewDialogue.Lines.Count];

							for (int j = 0; j < dia.DirectTo.NewDialogue.Lines.Count; j++)
							{
								nextDialogueTexts[j] = dia.DirectTo.NewDialogue.Lines[j].Text;
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
						dia.DirectTo.NewDialogue = null;
						dia.DirectTo.Enabled = false;

					}
				}
			}

			if (dia.SceneTransition.Enabled)
			{
				using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(400)))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						GUILayout.Label("Name of next Scene:", GUILayout.Width(130));

						dia.SceneTransition.Scene = GUILayout.TextField(dia.SceneTransition.Scene);

					}
					if (GUILayout.Button("Remove", GUILayout.Width(80)))
					{
						dia.SceneTransition.Scene = string.Empty;
						dia.SceneTransition.Enabled = false;

					}
				}
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				if (dia.Choices.Count == 0 && !dia.Variable.Enabled && !dia.DirectTo.Enabled && !dia.SceneTransition.Enabled)
				{
					if (dia.Speakers == null)
					{
						int _assetamount = GetCharacterDatabase();

						if (_assetamount != 1) return;
					}
					else
					{
						if (GUILayout.Button("New Line", GUILayout.Width(100)))
						{
							dia.Lines.Add(NewLine());
						}
					}

					if (dia.Lines.Count > 0)
					{
						if (GUILayout.Button("Add Choice", GUILayout.Width(100)))
						{
							dia.Choices.Add(new Choice());
							dia.Choices.Add(new Choice());
							dia.Choices.Add(new Choice());
						}

						if (GUILayout.Button("Add Condition", GUILayout.Width(100)))
						{
							dia.Variable.Enabled = true;
						}

						if (GUILayout.Button("Direct to...", GUILayout.Width(100)))
						{
							dia.DirectTo.Enabled = true;
						}

						if (GUILayout.Button("Enter Scene", GUILayout.Width(100)))
						{
							dia.SceneTransition.Enabled = true;
						}
					}
				}
			}

			GUILayout.Space(30);
		}

        #region UIMethods
		private bool ShowDialogueEventList(List<IDialogueEvent> eventList, bool open, string title = "Dialogue Events")
        {
			using (new EditorGUILayout.VerticalScope("Box"))
			{
				int _errors = ErrorCheck(eventList);
				GUIStyle _diastyle = new GUIStyle();
				_diastyle.padding = new RectOffset(20, 20, 0, 0);

				//Check for errors in Dialogue Events
				if (_errors > 0)
				{
					open = EditorGUILayout.BeginFoldoutHeaderGroup(open,
						new GUIContent($"{title} ({eventList.Count}) Errors: {_errors}",
						"Show list of Dialogue Events to be invoked at this line."));
				}
				else
				{
					open = EditorGUILayout.BeginFoldoutHeaderGroup(open,
						new GUIContent($"{title} ({eventList.Count})",
						"Show list of Dialogue Events to be invoked at this line."));
				}

				if (open)
				{
					for (int d = 0; d < eventList.Count; d++)
					{
                        if (eventList[d] != null)
                        {
                            EditorGUILayout.BeginHorizontal("Box");
							using (new EditorGUILayout.VerticalScope())
							{
								if (eventList[d] != null)
								{
									EditorGUILayout.LabelField(eventList[d]?.GetType().Name, EditorStyles.largeLabel);
									HandyFields.UISplitter(Color.gray);
									eventList[d]?.EditorUI();
									GUILayout.Space(5);
									eventList[d]?.ShowHelpBox();
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
									eventList.Remove(eventList[d]);
									Repaint();
								}

								if (eventList[d] != null) //Kinda sucks that I'm forced to do nested ifs here.
								{
									if (GUILayout.Button(new GUIContent("?", "Show Help"), GUILayout.Width(20)))
									{
										eventList[d].ToggleHelpBox();
									}
								}
							}
							EditorGUILayout.EndHorizontal();
                        }
                    }

					EditorGUILayout.BeginHorizontal("Box");
					_DiaEventIndex = EditorGUILayout.Popup(_DiaEventIndex, choices);
					if (GUILayout.Button("Add Dialogue Event"))
					{
						eventList.Add((IDialogueEvent)Activator.CreateInstance(_list[_DiaEventIndex]));
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndFoldoutHeaderGroup();
			}
			return open;
		}
        #endregion

        #region Private Functions
        private int GetCharacterDatabase()
		{
			string[] _databaseguids = AssetDatabase.FindAssets("t:CharacterDatabase");
			if (_databaseguids.Length == 1)
			{
				string _databasepath = AssetDatabase.GUIDToAssetPath(_databaseguids[0]);

				EditorGUILayout.LabelField($"Loading CharacterDatabase at {_databasepath}.");
				dia.Speakers = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(_databasepath);
				return 1;
			}
			else if (_databaseguids.Length > 1)
			{
				using (new EditorGUILayout.VerticalScope())
				{
					EditorGUILayout.LabelField("There are more than one CharacterDatabase Assets found in your project.");
					EditorGUILayout.Space(10);
					EditorGUILayout.LabelField("Here are the path of all the files:");
					for (int i = 0; i < _databaseguids.Length; i++)
					{
						string _path = AssetDatabase.GUIDToAssetPath(_databaseguids[i]);
						EditorGUILayout.LabelField($"	• {_path}");
					}
					EditorGUILayout.Space(10);
					EditorGUILayout.LabelField("Only one CharacterDatabase is allowed. Please delete all the duplicates until one remains.");
				}
				return 2;
			}
			else
			{
				using (new EditorGUILayout.VerticalScope())
				{
					EditorGUILayout.LabelField("There are no CharacterDatabase Asset found in your project.");
					EditorGUILayout.LabelField("Please create one by right clicking in the Project Window and navigating to: \n[Create > DREditor > Characters > Character Database]", GUILayout.Height(50));
					EditorGUILayout.LabelField("Do not create more than one. Only one CharacterDatabase is allowed.");
				}
				return 0;
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

		private void ShowEnlargedSprite(int line, float size = 200f, float offset = 90f)
		{
			GUI.backgroundColor = Color.white;
			GUI.color = Color.white;
			GUIStyle exprstyle = new GUIStyle();
			Material exprMat = dia.Lines[line].Expression.Sprite;
			var tex = Utility.Editor.HandyFields.GetMaterialTexture(exprMat);
			if (tex) exprstyle.normal.background = tex;

			if (exprstyle != null)
			{
				EditorGUI.LabelField(new Rect(Event.current.mousePosition.x - offset, Event.current.mousePosition.y, size, size), GUIContent.none, exprstyle);
			}

			isMouseOnSprite = false;
		}

		private void TextPreview(string text, params GUILayoutOption[] options)
		{
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.richText = true;
			style.wordWrap = true;

			if (text.Contains("<nar>") || text.Contains("<tut>")) text = $"{text}</color>";

			text = Regex.Replace(text, "<emp>", "<color=yellow>");
			text = Regex.Replace(text, "</emp>", "</color>");
			text = Regex.Replace(text, "<nar>", "<color=cyan>");
			text = Regex.Replace(text, "</nar>", "</color>");
			text = Regex.Replace(text, "<tut>", "<color=lime>");
			text = Regex.Replace(text, "</tut>", "</color>");

			if (text == "") text = "<color=yellow>INFO: This line has no text. This will be skipped, and only the Dialogue Events in this line will be invoked.\nYou can ignore this info if this is intended.</color>";
			EditorGUILayout.LabelField(new GUIContent(text, "Line Preview with formatting."), style, options);
		}

		private Line NewLine()
		{
			Line _neoLine = new Line();
			_neoLine.Speaker = null;
			_neoLine.SpeakerNumber = -1;
			_neoLine.DiaEvents = new List<IDialogueEvent>();
			if (dia.Lines.Count <= 0)
			{
				_neoLine.DiaEvents.Add(new ChangeCharacterFocus());
				_neoLine.DiaEvents.Add(new ChangeWindowPattern());
			}
			return _neoLine;
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
		#endregion
	}
}