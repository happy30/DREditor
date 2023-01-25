#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using DREditor.Dialogues;
using DREditor.Characters;
using DREditor.Utility.Editor;
using DREditor.Progression;
using DREditor.EventObjects;

namespace DREditor.Toolbox
{
    using Debug = UnityEngine.Debug;
    public class MassDialogueEdit : ScriptableWizard
    {
        public ProgressionDatabase progression;
        public Variable Variable = new Variable();

        [MenuItem("Tools/DREditor/Mass Edit Dialogues")]
        public static void CreateWizard()
        {
            DisplayWizard<MassDialogueEdit>("Edit Dialogue", "Edit");
        }

        private void OnWizardCreate()
        {

            if (progression)
            {
				foreach (Dialogue o in Selection.objects)
                {
					if (o.GetType() != typeof(Dialogue))
						continue;
                    if (Variable.Enabled)
                    {
						o.Variable = Variable;
						EditorUtility.SetDirty(o);
                    }
                }
            }
            else
            {
				EditorUtility.DisplayDialog("Progression was not specified.", "Please set up the progression database to " +
					"use this functionality!", "OK");
			}
			Close();
        }

        private void OnGUI()
        {
			if (!progression && GUILayout.Button("Refresh Looking for Progression Database"))
            {
				GetProgressionDatabase();
				return;
            }
			if (progression && !Variable.Enabled && GUILayout.Button("Apply Variable"))
				Variable.Enabled = true;
            #region Variable Section
            if (Variable.Enabled)
			{
				using (new EditorGUILayout.VerticalScope("Box", GUILayout.Width(400)))
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						
						EditorGUILayout.LabelField("If ", GUILayout.Width(80));

						Variable.BoolVariable =
							(BoolWithEvent)EditorGUILayout.ObjectField(Variable.BoolVariable, typeof(BoolWithEvent), false, GUILayout.Width(200));
					}

					#region Variable With Progression Database
					using (new EditorGUILayout.VerticalScope())
					{
						GUILayout.Label("Add Condition: ");
						Variable.Chapter = HandyFields.IntField("Chapter: ", Variable.Chapter);
						int tmpChapter = Variable.Chapter;
						if (progression != null && progression.Chapters.Count != 0)
						{

							if (Variable.Chapter < 0 || Variable.Chapter > progression.Chapters.Count)
							{
								Variable.Chapter = 0;
							}

							Chapter chapter = progression.Chapters[Variable.Chapter];

							if (chapter.Objectives.Count > 0 && Variable.Objective <= chapter.Objectives.Count)
							{
								Objective o;
								using (new EditorGUILayout.HorizontalScope())
								{
									Variable.Objective = EditorGUILayout.Popup(Variable.Objective, chapter.GetObjectives());
									o = chapter.Objectives[Variable.Objective];
									if (GUILayout.Button("Add All Flags of Objective") && Variable.Flags.Count == 0)
									{
										for (int i = 0; i < o.Flags.Count; i++)
										{
											Variable.Flags.Add(i);
										}
									}
								}

								if (Variable.Flags.Count != 0 && o.Flags.Count != 0)
								{
									if (Variable.Flags.Count <= o.Flags.Count)
									{
										string[] flagNames = o.GetFlagNames();
										for (int i = 0; i < Variable.Flags.Count; i++)
										{
											if (Variable.Flags[i] > o.Flags.Count)
											{
												Variable.Flags[i] = o.Flags.Count - 1;
											}
											using (new EditorGUILayout.HorizontalScope())
											{
												Variable.Flags[i] = EditorGUILayout.Popup(Variable.Flags[i], flagNames);
												if (GUILayout.Button("Remove Flag"))
												{
													Variable.Flags.Remove(Variable.Flags[i]);
												}
											}
										}
									}
									else
									{
										Debug.LogWarning("Reached Variable Flags greater than objective flags");
										Variable.Flags.Clear();
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
										Variable.Flags.Add(0);
									}

								}
							}
							else
							{
								GUILayout.Label("The set objective is either too high or too low so \n I'm setting it" +
									" to the last available objective for the chapter, \n" +
									"If you're still seeing this message it means the chapter has no \n" +
									"objectives and needs to have one.");
								Variable.Objective = chapter.Objectives.Count;
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

						if (Variable.NextDialogueTrue == null)
						{
							GUI.backgroundColor = Color.cyan;
						}
						Variable.NextDialogueTrue = (Dialogue)EditorGUILayout.ObjectField(Variable.NextDialogueTrue, typeof(Dialogue), false, GUILayout.Width(200));
					}

					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("False: ", GUILayout.Width(80));

						if (Variable.NextDialogueFalse == null)
						{
							GUI.backgroundColor = Color.cyan;
						}

						Variable.NextDialogueFalse = (Dialogue)EditorGUILayout.ObjectField(Variable.NextDialogueFalse, typeof(Dialogue), false, GUILayout.Width(200));
					}
					//GUI.backgroundColor = dia.Color;

					if (GUILayout.Button("Remove Condition", GUILayout.Width(130)))
					{
						Variable.BoolVariable = null;
						Variable.Enabled = false;
					}
				}
			}
			#endregion



			if (GUILayout.Button("Apply to Dialogues"))
				OnWizardCreate();
		}
        private void OnWizardUpdate()
        {
			if (!progression)
				GetProgressionDatabase();
			if (!progression)
            {
				EditorUtility.DisplayDialog("Progression was not specified.", "Please set up the progression database to " +
                    "use this functionality!", "OK");
			}
        }
        private int GetProgressionDatabase()
		{
			string[] _databaseguids = AssetDatabase.FindAssets("t:ProgressionDatabase");
			if (_databaseguids.Length == 1)
			{
				string _databasepath = AssetDatabase.GUIDToAssetPath(_databaseguids[0]);

				//EditorGUILayout.LabelField($"Loading ProgressionDatabase at {_databasepath}.");
				progression = AssetDatabase.LoadAssetAtPath<ProgressionDatabase>(_databasepath);
				return 1;
			}
			else if (_databaseguids.Length > 1)
			{
				using (new EditorGUILayout.VerticalScope())
				{
					//EditorGUILayout.LabelField("There are more than one ProgressionDatabase Assets found in your project.");
					//EditorGUILayout.Space(10);
					//EditorGUILayout.LabelField("Here are the path of all the files:");
					for (int i = 0; i < _databaseguids.Length; i++)
					{
						string _path = AssetDatabase.GUIDToAssetPath(_databaseguids[i]);
						EditorGUILayout.LabelField($"	• {_path}");
					}
					//EditorGUILayout.Space(10);
					//EditorGUILayout.LabelField("Only one ProgressionDatabase is allowed. Please delete all the duplicates until one remains.");
				}
				return 2;
			}
			else
			{
				using (new EditorGUILayout.VerticalScope())
				{
					//EditorGUILayout.LabelField("There is no ProgressionDatabase Asset found in your project.");
					//EditorGUILayout.LabelField("Please create one by right clicking in the Project Window and navigating to: \n[Create > DREditor > Progression > Progression Database]", GUILayout.Height(50));
					//EditorGUILayout.LabelField("Do not create more than one. Only one CharacterDatabase is allowed.");
				}
				return 0;
			}
		}
	}
}
#endif