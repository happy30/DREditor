#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

using DREditor.Characters;
using DREditor.Utility;
using DREditor.Utility.Editor;
using DREditor.EventObjects;
using DREditor.Dialogues.Events;
using DREditor.Progression;

namespace DREditor.Dialogues.Editor
{
	using Debug = UnityEngine.Debug;
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

		ProgressionDatabase progression;

		bool debugMode;

		private List<Type> _list;
		private string[] choices;

		private int _DiaEventIndex;

		private bool isMouseOnSprite;
		private int lineToEnlarge;
		int testLine = -1;

		public void OnEnable()
		{
			dia = (Dialogue)target;
			propLines = serializedObject.FindProperty("Lines");

			UpdateDiaEventList();
			
			/* Goal: Verify the current expression. This tool can be found in VerifyDialogues.cs 
			 * if (dia.Lines != null)
            {
				for (int i = 0; i < dia.Lines.Count; i++)
				{
					var currentLine = dia.Lines[i];
					if (currentLine.Speaker != null && !IsProtagonist(currentLine.Speaker) && currentLine.ExpressionNumber != 0)
					{
						// If sprite moved position on characters expression list
						if (currentLine.Speaker.Expressions[currentLine.ExpressionNumber - 1].Name != currentLine.Expression.Name)
						{
							if (currentLine.Speaker.Expressions.Contains(currentLine.Expression))
							{
								currentLine.ExpressionNumber = currentLine.Speaker.Expressions.IndexOf(currentLine.Expression) + 1;
								Debug.Log("By Expression Changed " + dia.name + "'s Line " + i + "expression number for " + currentLine.Expression.Name);
								continue;
							}
							foreach(Expression e in currentLine.Speaker.Expressions)
                            {
								if(e.Name == currentLine.Expression.Name)
                                {
									currentLine.ExpressionNumber = currentLine.Speaker.Expressions.IndexOf(e) + 1;
									Debug.Log("By Name Changed " + dia.name + "'s Line " + i + "expression number for " + e.Name);
									break;
								}
							}
						}
					}
				}
			}
			 */


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

			if (progression == null)
			{
				int _assetamount = GetProgressionDatabase();

				//if (_assetamount != 1) return;
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

            #region Options
            using (new EditorGUILayout.HorizontalScope())
			{
				dia.IsInstant = HandyFields.Option(dia.IsInstant, "Is Instant: ", 60);
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				dia.UnlockPause = HandyFields.Option(dia.UnlockPause, "Unlock Pause: ", 90);
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				dia.UnlockPauseOption = HandyFields.Option(dia.UnlockPauseOption, "Unlock Option: ", 95);
                if (dia.UnlockPauseOption)
					dia.PauseOptionNum = HandyFields.IntField("Option Number: ", dia.PauseOptionNum, 20, 7);
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				if (dia.UnlockPauseOption)
					dia.PauseOptionTo = HandyFields.Option(dia.PauseOptionTo, "To: ", 25);
			}
			
			
			#endregion

			EditorStyles.textArea.wordWrap = true;
			EditorStyles.textField.wordWrap = true;

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
		private void EditHeader()
        {
			using (new EditorGUILayout.VerticalScope("Box"))
			{
				GUI.backgroundColor = dia.Color;
				//dia.DialogueMode = (BoxMode)EditorGUILayout.EnumPopup("Dialogue Mode:", dia.DialogueMode);
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

				/*if (dia.DialogueMode == BoxMode.Normal)
				{
					dia.ShowDialoguePanel = EditorGUILayout.Toggle("Show Dialogue Panel: ", dia.ShowDialoguePanel);
				}*/

				GUI.backgroundColor = Color.white;
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
		}
		private void EditLine(int i)
        {
			EditStudentBGColor(dia.Lines[i]);
			using (new EditorGUILayout.HorizontalScope("Box"))
			{
				using (new EditorGUILayout.VerticalScope())
				{
					//MainBox
					using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth)))
					{
						EditLeftPanel(dia.Lines[i], i);
						EditExpressionPanel(dia.Lines[i]);
						EditDialogueText(i);
						EditDialoguePosition(i);
					}
					EditLineLower(dia.Lines[i], i);
				}
			}

			GUI.color = Color.white;
			HandyFields.UISplitter(dia.Color, 2, 0, 5);
		}
		private void EditStudentBGColor(Line currentLine)
		{
			var color = Color.white;

			if (currentLine.Speaker is Student)
			{
				var stu = currentLine.Speaker as Student;

				float H, S, V;

				color = stu.StudentCard.Color;
				Color.RGBToHSV(color, out H, out S, out V);
				S = EditorGUIUtility.isProSkin ? 0.2f : 0.3f;
				V = EditorGUIUtility.isProSkin ? 1f : 0.95f;

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
		}
		private void EditLeftPanel(Line currentLine, int i)
        {

			#region Column 1
			//Column 1

			using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(130)))
			{
				if(testLine != -1)
                {
					using (new EditorGUILayout.HorizontalScope())
                    {
						if (testLine != -1 && GUILayout.Button("Exit", GUILayout.Width(60)))
						{
							testLine = -1;
						}
						GUILayout.Label(i.ToString());
					}
                }
				else
					GUILayout.Label(i.ToString());

				var prependedArray = ContainerUtil.PrependedList(dia.GetCharacterNames(), "<No Character>");
				currentLine.SpeakerNumber = EditorGUILayout.IntPopup(currentLine.SpeakerNumber, prependedArray,
					ContainerUtil.Iota(prependedArray.Length, -1), GUILayout.MaxWidth(130));
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
							GUILayout.MaxWidth(130));
				}
				/* VFX Section
				var vfxNames = dia.GetVFXNames();
				var vfxarr = new string[dia.GetVFXNames().Length + 1];
				vfxarr[0] = "(No VFX)";
				for (int j = 1; j < vfxNames.Length + 1; j++)
				{
					vfxarr[j] = vfxNames[j - 1];
				}
				var vfxInts = new int[vfxarr.Length];
				for (int j = 0; j < vfxInts.Length; j++)
				{
					vfxInts[j] = j;
				}
				dia.Lines[i].VFXNumber = EditorGUILayout.IntPopup(dia.Lines[i].VFXNumber,
					vfxarr, vfxInts,
						GUILayout.MaxWidth(130));
				*/
				/*var prependedArray2 = ContainerUtil.PrependedList(dia.GetVFXNames(), "<No VFX>");
				currLine.VFXNumber = EditorGUILayout.IntPopup(currLine.VFXNumber, prependedArray2,
					ContainerUtil.Iota(prependedArray2.Length, -1), GUILayout.Width(130));
				currLine.VFX = currLine.VFXNumber == -1 ? null : dia.VFXDB.VFXClips[currLine.VFXNumber];*/


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

				#region Options
				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.Label("Options", GUILayout.MaxWidth(50));
					dia.Lines[i].ShowOptions = EditorGUILayout.Toggle(dia.Lines[i].ShowOptions, GUILayout.MaxWidth(30));
					if (GUILayout.Button("^", GUILayout.Width(15)))
					{
						testLine = i;
					}
				}

				using (new EditorGUILayout.HorizontalScope())
				{
					if (dia.Lines[i].ShowOptions)
					{
						/*var prependedArray = ContainerUtil.PrependedList(dia.GetCharacterNames(), "<No Character>");
						currentLine.SpeakerNumber = EditorGUILayout.IntPopup(currentLine.SpeakerNumber, prependedArray,
							ContainerUtil.Iota(prependedArray.Length, -1), GUILayout.Width(130));*/
						//GUILayout.Label("Don't Pan", GUILayout.Width(60));
						dia.Lines[i].DontPan = HandyFields.Option(dia.Lines[i].DontPan, "Don't Pan");

						GUILayout.Label("PLeave", GUILayout.Width(50));
						dia.Lines[i].Leave = EditorGUILayout.Toggle(dia.Lines[i].Leave, GUILayout.Width(15));
						GUILayout.Label("PTC", GUILayout.Width(30));
						dia.Lines[i].PanToChar = EditorGUILayout.Toggle(dia.Lines[i].PanToChar, GUILayout.Width(15));
						if (dia.Lines[i].PanToChar)
						{
							dia.Lines[i].CharToPanNum = EditorGUILayout.IntPopup(dia.Lines[i].CharToPanNum, prependedArray,
								ContainerUtil.Iota(prependedArray.Length, -1), GUILayout.Width(130));
							dia.Lines[i].CharToPan = dia.Lines[i].CharToPanNum == -1 ? null : dia.Speakers.Characters[dia.Lines[i].CharToPanNum];
						}


					}
				}

				if (dia.Lines[i].ShowOptions)
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						dia.Lines[i].FlashWhite = HandyFields.Option(dia.Lines[i].FlashWhite, "Flash White", 75);
						//dia.Lines[i].Faint = HandyFields.Option(dia.Lines[i].Faint, "Faint", 35);
						dia.Lines[i].BlurVision = HandyFields.Option(dia.Lines[i].BlurVision, "Blur Vision", 65);
					}
				}

				#endregion

				
			}
			#endregion

		}
		private void EditExpressionPanel(Line currLine)
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
					dia.Lines[i].StopEnv = HandyFields.Option(dia.Lines[i].StopEnv, "Stop", 30);
					dia.Lines[i].StopSFX = EditorGUILayout.Toggle(dia.Lines[i].StopSFX, GUILayout.Width(15));
					using (new EditorGUILayout.HorizontalScope())
					{
						string name = "Env:";
						EditorGUILayout.LabelField(name, GUILayout.Width(name.Length * 7));

						var vfxProperty = propLines.GetArrayElementAtIndex(i).FindPropertyRelative("EnvSFX");
						EditorGUIUtility.labelWidth = 1;
						EditorGUILayout.PropertyField(vfxProperty, new GUIContent(""));
					}
				}

			}
			#endregion

		}
		private void EditDialoguePosition(int i)
        {

			#region Column 4 Side Buttons
			//Column 4
			using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(25)))
			{
				if (dia.Lines.Count > 1)
				{
					if (GUILayout.Button(new GUIContent("-", "Delete this Line."), GUILayout.Width(20)) && dia.Lines.Count > 1)
					{
						GUI.FocusControl(null);
						dia.Lines.Remove(dia.Lines[i]);
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

					Copy.translationKey = dia.Lines[i].translationKey;
					Copy.SpeakerNumber = dia.Lines[i].SpeakerNumber;
					Copy.Text = dia.Lines[i].Text;
					Copy.VoiceSFX = dia.Lines[i].VoiceSFX;
					Copy.SFX = dia.Lines[i].SFX;
					Copy.Events = dia.Lines[i].Events;
					Copy.DiaEvents = new List<IDialogueEvent>();

					foreach (IDialogueEvent ev in dia.Lines[i].DiaEvents)
					{
						Copy.DiaEvents.Add(ev);
					}

					Copy.TimeToNextLine = dia.Lines[i].TimeToNextLine;
					Copy.AutomaticLine = dia.Lines[i].AutomaticLine;
					Copy.Expression = dia.Lines[i].Expression;
					Copy.ExpressionNumber = dia.Lines[i].ExpressionNumber;
					Copy.AliasNumber = dia.Lines[i].AliasNumber;

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
			#endregion

		}
		private void EditLineLower(Line currentLine, int i)
        {

			GUILayout.Space(5f);

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
		private void EditFooter()
        {
			/*using (new EditorGUILayout.VerticalScope("Box"))
			{
				dia.ShowEndEvents = EditorGUILayout.BeginFoldoutHeaderGroup(dia.ShowEndEvents,
						new GUIContent($"End Events ",
						"Show list of Dialogue Events to be invoked at this line."));


				EditorGUILayout.EndFoldoutHeaderGroup();
			}*/
			using (new EditorGUILayout.HorizontalScope())
			{
				dia.ClearLock = HandyFields.Option(dia.ClearLock, "Clear Locks", 80);
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
						dia.Choices[i].NextDialogue =
							(Dialogue)EditorGUILayout.ObjectField(dia.Choices[i].NextDialogue, typeof(Dialogue), true,
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

					#region Variable With Progression Database
					using (new EditorGUILayout.VerticalScope())
					{
						GUILayout.Label("Add Condition: ");
						dia.Variable.Chapter = HandyFields.IntField("Chapter: ", dia.Variable.Chapter);
						int tmpChapter = dia.Variable.Chapter;
						if (progression != null && progression.Chapters.Count != 0)
						{

							if (dia.Variable.Chapter < 0 || dia.Variable.Chapter > progression.Chapters.Count)
							{
								dia.Variable.Chapter = 0;
							}

							Chapter chapter = progression.Chapters[dia.Variable.Chapter];

							if (chapter.Objectives.Count > 0 && dia.Variable.Objective <= chapter.Objectives.Count)
							{
								Objective o;
								using (new EditorGUILayout.HorizontalScope())
								{
									dia.Variable.Objective = EditorGUILayout.Popup(dia.Variable.Objective, chapter.GetObjectives());
									o = chapter.Objectives[dia.Variable.Objective];
									if (GUILayout.Button("Add All Flags of Objective") && dia.Variable.Flags.Count == 0)
									{
										for (int i = 0; i < o.Flags.Count; i++)
										{
											dia.Variable.Flags.Add(i);
										}
									}
								}

								if (dia.Variable.Flags.Count != 0 && o.Flags.Count != 0)
								{
									if (dia.Variable.Flags.Count <= o.Flags.Count)
									{
										string[] flagNames = o.GetFlagNames();
										for (int i = 0; i < dia.Variable.Flags.Count; i++)
										{
											if (dia.Variable.Flags[i] > o.Flags.Count)
											{
												dia.Variable.Flags[i] = o.Flags.Count - 1;
											}
											using (new EditorGUILayout.HorizontalScope())
											{
												dia.Variable.Flags[i] = EditorGUILayout.Popup(dia.Variable.Flags[i], flagNames);
												if (GUILayout.Button("Remove Flag"))
												{
													dia.Variable.Flags.Remove(dia.Variable.Flags[i]);
												}
											}
										}
									}
									else
									{
										Debug.LogWarning("Reached Variable Flags greater than objective flags");
										dia.Variable.Flags.Clear();
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
										dia.Variable.Flags.Add(0);
									}

								}
							}
							else
							{
								GUILayout.Label("The set objective is either too high or too low so \n I'm setting it" +
									" to the last available objective for the chapter, \n" +
									"If you're still seeing this message it means the chapter has no \n" +
									"objectives and needs to have one.");
								dia.Variable.Objective = chapter.Objectives.Count;
							}
						}
						else
						{
							GUILayout.Label("Set up a ProgressionDatabase or add some chapters to it " +
								"to use ProgressionDatabase functionality.");
						}
					}
					#endregion

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
					using (new EditorGUILayout.VerticalScope())
					{
						GUILayout.Label("Name of next Scene:", GUILayout.Width(130));
						using (new EditorGUILayout.HorizontalScope())
						{
							dia.SceneTransition.Scene = GUILayout.TextField(dia.SceneTransition.Scene);
						}
						using (new EditorGUILayout.HorizontalScope())
						{
							dia.SceneTransition.ToDark = HandyFields.Option(dia.SceneTransition.ToDark, "To Dark: ");
						}
						using (new EditorGUILayout.HorizontalScope())
						{
							dia.SceneTransition.ToMenu = HandyFields.Option(dia.SceneTransition.ToMenu, "To Menu: ");
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

			if (dia.StartInvestigation.Enabled)
			{
				using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(400)))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						dia.StartInvestigation.NextObjective = HandyFields.Option(dia.StartInvestigation.NextObjective, "Next Objective: ", 100);
						dia.StartInvestigation.NewDialogue = HandyFields.UnityField(dia.StartInvestigation.NewDialogue, 120, 30);
					}
					if (GUILayout.Button("Remove", GUILayout.Width(80)))
					{
						dia.StartInvestigation.NextObjective = false;
						dia.StartInvestigation.Enabled = false;
						dia.StartInvestigation.NewDialogue = null;
					}
				}
			}

			#region End Event Buttons
			using (new EditorGUILayout.HorizontalScope())
			{
				if (dia.Choices.Count == 0 && !dia.Variable.Enabled)
				{
					// was: dia.Choices.Count == 0 && !dia.Variable.Enabled && !dia.DirectTo.Enabled && !dia.SceneTransition.Enabled
					if (dia.Speakers == null)
					{
						if (Resources.Load<CharacterDatabase>("Characters/CharacterDatabase"))
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
							dia.Lines.Add(new Line());
						}
					}



					if (dia.Lines.Count > 0)
					{
						if (GUILayout.Button("Add Condition", GUILayout.Width(100)))
						{
							dia.Variable.Enabled = true;
						}

						if (!dia.DirectTo.Enabled && GUILayout.Button("Direct to...", GUILayout.Width(100)))
						{
							dia.DirectTo.Enabled = true;
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
					}

				}
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				if (dia.Choices.Count == 0)
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
						if (!dia.StartInvestigation.Enabled)
						{
							if (GUILayout.Button("Investigation", GUILayout.Width(100)))
							{
								dia.StartInvestigation.Enabled = true;
							}
						}
					}

				}
			}
			#endregion

			GUILayout.Space(30);
		}

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
				EditorGUI.LabelField(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y,
					tex.width/5, tex.height/5), GUIContent.none, exprstyle);
			}

			isMouseOnSprite = false;
		}

		private void TextPreview(string text, float height, float width)
		{
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.richText = true;
			style.wordWrap = true;

			if (text.Contains("$") || text.Contains("<nar>") || text.Contains("<tut>")) text = $"{text}</color>";
			
			int c = text.Count(x => x == '@');
			var regex = new Regex(Regex.Escape("@"));
			for (int i = 0; i < c; i++)
			{
				if (i % 2 == 0)
					text = regex.Replace(text, "<color=yellow>", 1);
				else
					text = regex.Replace(text, "</color>", 1);
			}
			text = Regex.Replace(text, "<emp>", "<color=yellow>");
			text = Regex.Replace(text, "</emp>", "</color>");
			text = Regex.Replace(text, "<nar>", "<color=cyan>");
			text = Regex.Replace(text, "\\$", "<color=cyan>");
			text = Regex.Replace(text, "</nar>", "</color>");
			text = Regex.Replace(text, "<tut>", "<color=lime>");
			text = Regex.Replace(text, "</tut>", "</color>");

			if (text == "") 
				text = "<size=\"10\"><color=magenta>INFO: This line has no text. This will be skipped, and only the Dialogue Events in this line will be invoked." +
					"You can ignore this info if this is intended.</color></size>";

			EditorGUILayout.LabelField(new GUIContent(text, "Text Preview with formatting"), style, GUILayout.Height(height));
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
							dia.Lines.Add(new Line());
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
	}

}
#endif