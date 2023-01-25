//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using DREditor.Characters;
using DREditor.Utility;
using DREditor.Utility.Editor;
using DREditor.Dialogues;
using DREditor.Editor;
using DREditor.TrialEditor;
using DG.Tweening;
using NSD;
using System;
using DREditor.Dialogues.Editor;
using DREditor.Dialogues.Events;
using DREditor.Camera;
using Cinemachine;

[CustomEditor(typeof(NSDBuilder))]
public class NSDBuilderEditor : DialogueEditorBase
{
    private NSDBuilder dia;
    private TrialCameraVFXDatabase cameraVFXDatabase;
    private TrialCameraAnimDatabase cameraAnimDatabase;
    private EvidenceDatabase evidenceDatabase;
    private SerializedProperty propLines;
    private SerializedProperty music;
    private bool editPanelMode = false;
    private int editPanelNum;

    private List<Type> _list;
    private string[] choices;
    private int _DiaEventIndex;

    bool debugMode;
    int testLine = -1;
    int testPhrase = -1;
    int testNoise = -1;
    public void OnEnable()
    {
        dia = target as NSDBuilder;
        propLines = serializedObject.FindProperty("PanelGroup");
        music = serializedObject.FindProperty("StartingMusic");
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
        if (!ValidateCharacterDatabase() || !ValidateCharacters() ||
            !ValidateCameraAnimDatabase() || !ValidateCameraAnims() ||
            !ValidateCameraVFXDatabase() || !ValidateCameraVFXs() || !ValidateEvidenceDB()) { return; }

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
        if (!editPanelMode)
        {
            for (int i = 0; i < dia.PanelGroup.Count; i++)
            {
                if (dia.PanelGroup[i].edit)
                {
                    editPanelMode = true;
                    editPanelNum = i;
                }
            }
        }
        
        if (editPanelMode)
        {
            EditPanelModeWindow();
            return;
        }
        if (testLine == -1)
        {
            EditNSDBase();
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(dia.DialogueName, EditorStyles.boldLabel);
            dia.Color = EditorGUILayout.ColorField(dia.Color, GUILayout.Width(50));
        }
        //EditHeader();
        EditTestLine();
        EditPanels();

        if (testLine == -1)
        {
            EditFooter();
        }
    }
    private bool ValidateEvidenceDB()
    {
        var database = Resources.Load<EvidenceDatabase>("Evidence/EvidenceDatabase");
        if (!database)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("There's no Evidence Database in the resources folder");
                EditorGUILayout.LabelField("Create a Evidence Database in Resources/DREditor/Evidence/Evidence Database.asset");
            }
            return false;
        }
        evidenceDatabase = database;
        return true;
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
                if (testLine != dia.PanelGroup.Count - 1 && GUILayout.Button("Next Line", GUILayout.Width(100)))
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
            for (int i = 0; i < dia.PanelGroup.Count; i++)
            {
                dia.PanelGroup[i].camAnimIdx = UnityEngine.Random.Range(0, cameraAnimDatabase.anims.Count - 1);
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
                if (dia.PanelGroup.Count > 0)
                {
                    tx = dia.PanelGroup[0].Text.Substring(0, dia.PanelGroup[0].Text.Length > 29 ? 30 : dia.PanelGroup[0].Text.Length);
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

    private void EditStudentBGColor(NSDBuilder.Panel currLine)
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
    
    private void EditPanels()
    {
        if (dia.PanelGroup == null)
        {
            return;
        }
        if (testLine != -1)
        {
            EditLine(testLine);
        }
        else
        {
            for (int i = 0; i < dia.PanelGroup.Count; i++)
            {
                
                EditLine(i);
                
            }
        }

        if (testLine == -1)
            dia.skip = HandyFields.IntField("Skip:", dia.skip, 20);
    }
    //bool reset = false;
    private void EditLine(int i)
    {
        var currLine = dia.PanelGroup[i];
        EditStudentBGColor(currLine);

        using (new EditorGUILayout.HorizontalScope("Box"))
        {
            EditLeftPanel(currLine, i);
            EditExpressionPanel(currLine);
            
            EditDialogueText(i);
            EditDialoguePosition(i);
            if (i >= propLines.arraySize || i >= dia.PanelGroup.Count)
                return;
        }
        EditLineLower(currLine, i);
        GUI.color = Color.white;
        HandyFields.UISplitter(dia.Color, 2, 0, 5);
    }

    private void EditLeftPanel(NSDBuilder.Panel currLine, int i)
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
            //EditAutomatic(currLine);
            /*
            using (new EditorGUILayout.HorizontalScope())
            {
                currLine.DontPan = HandyFields.Option(currLine.DontPan, "Don't Pan: ", 70);
            }
            */
            using (new EditorGUILayout.HorizontalScope())
            {
                /*
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
                */
                if (GUILayout.Button("Test Panel", GUILayout.Width(80)))
                {
                    if (!man)
                        man = FindObjectOfType<NSDManager>();
                    if (man != null)
                    {
                        TestPanel(currLine, i);
                    }
                    else
                        Debug.Log("Unsuccessful");
                }
                if (GUILayout.Button("^", GUILayout.Width(15)))
                {
                    testLine = i;
                }
                
            }
            if (GUILayout.Button("Edit Panel", GUILayout.Width(70)))
            {
                editPanelNum = i;
                editPanelMode = true;
            }
        }
    }
    private bool CheckApplication()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Must be in Play Mode", "You must be in play mode with a DRTrialCamera to test lines!", "Ok");
            return true;
        }
        return false;
    }
    private void TestPanel(NSDBuilder.Panel line, int i)
    {
        if (CheckApplication())
            return;
        man.PlayTestPanel(line, i);
    }
    DRTrialCamera cam;
    NSDManager man;
    private void TestCameraAnim(NSDBuilder.Panel line, bool gameTest)
    {
        if (CheckApplication())
            return;

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
    private void EditSpeaker(NSDBuilder.Panel currLine)
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

    private void EditEvents(NSDBuilder.Panel currLine)
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
    
    private void EditCamAnim(NSDBuilder.Panel currLine)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("Cam:", GUILayout.Width(40));
            currLine.camAnimIdx = EditorGUILayout.IntPopup(currLine.camAnimIdx, cameraAnimDatabase.GetNames().ToArray(), ContainerUtil.Iota(cameraAnimDatabase.anims.Count));
        }
    }
    private void EditAutomatic(NSDBuilder.Panel currLine)
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
    
    private void EditExpressionPanel(NSDBuilder.Panel currLine)
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
                string name = "Voice:";
                EditorGUILayout.LabelField(name, GUILayout.Width(name.Length * 7));

                var vfxProperty = propLines.GetArrayElementAtIndex(i).FindPropertyRelative("VoiceSFX");
                EditorGUIUtility.labelWidth = 1;
                EditorGUILayout.PropertyField(vfxProperty, new GUIContent(""));
            }



        }
        #endregion
    }
    private void EditDialoguePosition(int i)
    {
        GUI.backgroundColor = dia.Color;

        using (new EditorGUILayout.VerticalScope())
        {
            if (dia.PanelGroup.Count > 1)
            {
                if (GUILayout.Button("-", GUILayout.Width(20)) && dia.PanelGroup.Count > 1)
                {
                    GUI.FocusControl(null);
                    dia.PanelGroup.Remove(dia.PanelGroup[i]);
                }
            }

            if (i > 0)
            {
                if (GUILayout.Button("ʌ", GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    dia.PanelGroup.Swap(i, i - 1);
                }
            }

            if (i < dia.PanelGroup.Count - 1)
            {
                if (GUILayout.Button("v", GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    dia.PanelGroup.Swap(i, i + 1);
                }
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                dia.PanelGroup.Insert(i + 1, new NSDBuilder.Panel());
            }
        }
    }
    private void EditFooter()
    {
        
        if (GUILayout.Button("New Line", GUILayout.Width(100)))
        {
            dia.PanelGroup.Add(new NSDBuilder.Panel());
        }
    }
    void EditLineLower(NSDBuilder.Panel currentLine, int i)
    {
        GUILayout.Space(5f);

        using (new EditorGUILayout.VerticalScope())
        {
            if (dia.PanelGroup[i].co.enabled)
            {
                EditTCO(dia.PanelGroup[i], dia.PanelGroup[i].co);
            }
            else if (GUILayout.Button("Add Camera Override", GUILayout.Width(170)))
            {
                dia.PanelGroup[i].co.enabled = true;
                dia.PanelGroup[i].co.distance = 8.11f;
                if (currentLine.Speaker != null)
                {
                    dia.PanelGroup[i].co.seatFocus = currentLine.Speaker.TrialPosition;
                    dia.PanelGroup[i].co.height = currentLine.Speaker.TrialHeight;
                }
            }
        }

        using (new EditorGUILayout.VerticalScope())
        {
            if (dia.PanelGroup[i].SFX != null)
            {
                var sfxListProperty = propLines.GetArrayElementAtIndex(i).FindPropertyRelative("SFX");
                EditorGUIUtility.labelWidth = 1;
                //UnityEngine.Debug.Log(sfxListProperty.propertyType);
                EditorGUILayout.PropertyField(sfxListProperty, new GUIContent(""), GUILayout.MinWidth(270));
            }
        }
        
        /*
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
        */
    }

    #region Trial Camera Overrides
    float tcoMinWidth = 200;
    TCODatabase presetbase;
    int presetNum = 0;
    string presetName;
    private void EditTCO(NSDBuilder.Panel line, TCO o)
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
        //DebugCharacterSprites();
        //if (GUILayout.Button("Remove top half of Lines"))
            //SplitDialogue(true);
        //if (GUILayout.Button("Remove bottom half of Lines"))
            //SplitDialogue(false);
        if (GUILayout.Button("Randomize Camera Anims"))
            RandomizeCamAnim();
        DebugApplyTCOToChar();
    }

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
        foreach (NSDBuilder.Panel l in dia.PanelGroup)
        {
            if (l.Speaker == debugSpeaker)
            {
                l.co = coDebug;
            }
        }
        debugMode = false;
    }
    #endregion

    #region Convert Trial Dia to NSD
    TrialDialogue trialDia;
    void ConvertTrialOption()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Convert Trial Dia :", GUILayout.Width(120));
            trialDia = HandyFields.UnityField(trialDia, 170, 20);
            
        }
        if (trialDia != null && GUILayout.Button("Apply Conversion", GUILayout.Width(130))
                && EditorUtility.DisplayDialog("Convert", "Are you sure you want to Apply these lines to this NSD?" +
                "\n THIS WILL OVERWRITE ALL PANELS AND CANNOT BE UNDONE!", "Yes", "No"))
        {
            ConversionProcess(trialDia);
        }
    }
    void ConversionProcess(TrialDialogue tri)
    {
        for(int i = 0; i < tri.Lines.Count; i++)
        {
            TrialLine line = tri.Lines[i];
            if(i >= dia.PanelGroup.Count)
            {
                dia.PanelGroup.Add(new NSDBuilder.Panel());
            }
            NSDBuilder.Panel panel = dia.PanelGroup[i];
            panel.SpeakerNumber = line.SpeakerNumber;
            panel.PanelText = line.Text;
            panel.Text = line.Text;
            panel.ExpressionNumber = line.ExpressionNumber;
            panel.Expression = line.Expression;
        }
    }
    #endregion

    #region NSD Section
    private void EditNSDBase()
    {
        ConvertTrialOption();
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Starting Music :", GUILayout.Width(100));
            //dia.StartingMusic = HandyFields.UnityField(dia.StartingMusic, 140, 20);
            //var musicProperty = propLines.FindPropertyRelative("StartingMusic");
            EditorGUIUtility.labelWidth = 1;
            EditorGUILayout.PropertyField(music, new GUIContent(""));
        }
        dia.times = BuilderEditor.DisplayTimerSettings(dia.times);
        //dia.timerMinutes = HandyFields.FloatField("Timer Minutes :", dia.timerMinutes, 50, 7);
        //dia.timerSeconds = HandyFields.FloatField("Timer Seconds :", dia.timerSeconds, 50, 7);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Ending Panel Group Dialogue :", GUILayout.Width(180));
            dia.EndDialogue = HandyFields.UnityField(dia.EndDialogue, 170, 20);
        }
        using (new EditorGUILayout.VerticalScope())
        {
            dia.TrialNumber = HandyFields.IntField("Current Chapter: ", dia.TrialNumber, 20);
            try
            {
                Evidence x = evidenceDatabase.Evidences[dia.TrialNumber]; // a dud for thr log
            }
            catch
            {
                EditorGUILayout.LabelField("There is no evidence for that trial in the evidence database");

                return;
            }

            if (evidenceDatabase.Evidences[dia.TrialNumber] != null)
            {
                dia.TruthBulletsKind = DisplayBulletList(dia.TruthBulletsKind, "Kind", choiceNumKind);
                dia.TruthBulletsNormal = DisplayBulletList(dia.TruthBulletsNormal, "Normal", choiceNumNormal);
                dia.TruthBulletsMean = DisplayBulletList(dia.TruthBulletsMean, "Mean", choiceNumMean);
            }
        }
    }
    private List<TruthBullet> DisplayBulletList(List<TruthBullet> TruthBullets, string diff, List<int> choiceNum)
    {
        if (choiceNum.Count == 0)
        {
            choiceNum = dia.BulletNums;
        }
        EditorGUILayout.LabelField("Truth Bullets for: " + diff);
        if (TruthBullets != null)
        {
            for (int i = 0; i < TruthBullets.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    BulletIteration(TruthBullets[i], i, choiceNum, TruthBullets);
                    if (GUILayout.Button("Remove Bullet"))
                    {
                        TruthBullets.Remove(TruthBullets[i]);
                        choiceNum.RemoveAt(i);
                    }
                }

            }
        }

        if (GUILayout.Button("Add Bullet"))
        {
            TruthBullets.Add(evidenceDatabase.Evidences[dia.TrialNumber].TruthBullets[0]);
            choiceNum.Add(0);
        }
        dia.BulletNums = choiceNum;
        return TruthBullets;
    }
    private List<int> choiceNumKind = new List<int>();
    private List<int> choiceNumNormal = new List<int>();
    private List<int> choiceNumMean = new List<int>();
    private void BulletIteration(TruthBullet currBullet, int i, List<int> choiceNum, List<TruthBullet> TruthBullets)
    {
        Evidence trialEvidence = evidenceDatabase.Evidences[dia.TrialNumber];
        choiceNum[i] = EditorGUILayout.IntPopup(choiceNum[i], trialEvidence.toStringArray(),
                trialEvidence.tointArray());

        currBullet = trialEvidence.TruthBullets[choiceNum[i]]; // On an asset this had an error prob from Removing Nihile Bullet
        TruthBullets[i] = currBullet;
    }
    private void EditPanelModeWindow()
    {
        if (GUILayout.Button("Exit", GUILayout.MaxWidth(40)))
        {
            editPanelMode = false;
            dia.PanelGroup[editPanelNum].edit = editPanelMode;
        }
        
        dia.PanelGroup[editPanelNum].PanelText = dia.PanelGroup[editPanelNum].Text;
        dia.PanelGroup[editPanelNum].PanelText = HandyFields.StringArea("Panel Text", dia.PanelGroup[editPanelNum].PanelText);
        dia.PanelGroup[editPanelNum].Text = dia.PanelGroup[editPanelNum].PanelText;

        
        dia.PanelGroup[editPanelNum].waitTime = HandyFields.FloatField("Wait Time after animations finish: ", dia.PanelGroup[editPanelNum].waitTime,20);

        if (GUILayout.Button("Test Panel", GUILayout.Width(80)))
        {
            if (!man)
                man = FindObjectOfType<NSDManager>();
            if (man != null)
            {
                TestPanel(dia.PanelGroup[editPanelNum], editPanelNum);
            }
            else
                Debug.Log("Unsuccessful");
        }
        EditTestNoise();
        EditTestPhrase();
        GUILayout.Space(25);
        if (testNoise != -1)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("WhiteNoise " + (testNoise + 1), GUILayout.Width(70));
                if (GUILayout.Button("Exit", GUILayout.Width(60)))
                {
                    testNoise = -1;
                    return;
                }
            }
            EditWhiteNoise(dia.PanelGroup[editPanelNum].whiteNoises[testNoise]);
            return;
        }
        if (testPhrase != -1)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Phrase " + (testPhrase + 1), GUILayout.Width(70));
                if (GUILayout.Button("Exit", GUILayout.Width(60)))
                {
                    testPhrase = -1;
                    return;
                }
            }
            EditPhrase(dia.PanelGroup[editPanelNum].PhraseGroup[testPhrase]);
            return;
        }
        if (dia.PanelGroup[editPanelNum].PhraseGroup.Count != 0)
        {
            for(int i = 0; i < dia.PanelGroup[editPanelNum].PhraseGroup.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("");
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Phrase " + (i + 1), GUILayout.Width(70));
                    if (GUILayout.Button("^", GUILayout.Width(15)))
                    {
                        testPhrase = i;
                    }
                }
                
                EditPhrase(dia.PanelGroup[editPanelNum].PhraseGroup[i]);
                
            }
        }

        if(GUILayout.Button("Add Phrase"))
        {
            dia.PanelGroup[editPanelNum].PhraseGroup.Add(new NSDBuilder.Phrase());
        }


        if (dia.PanelGroup[editPanelNum].whiteNoises.Count != 0)
        {
            for (int i = 0; i < dia.PanelGroup[editPanelNum].whiteNoises.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("");
                }
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("White Noise " + (i + 1), GUILayout.Width(120));
                    if (GUILayout.Button("^", GUILayout.Width(15)))
                    {
                        testNoise = i;
                    }
                }
                EditWhiteNoise(dia.PanelGroup[editPanelNum].whiteNoises[i]);
            }
        }

        if (GUILayout.Button("Add White Noise"))
        {
            if (dia.PanelGroup[editPanelNum].whiteNoises.Count < 9)
            {
                dia.PanelGroup[editPanelNum].whiteNoises.Add(new NSDBuilder.WhiteNoise());
            }
        }
    }
    private void EditTestPhrase()
    {
        if (testPhrase != -1)
        {
            GUILayout.Space(25);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (testPhrase != 0 && GUILayout.Button("Previous Phrase", GUILayout.Width(120)))
                {
                    testPhrase -= 1;
                }
                else if (testPhrase == 0)
                    GUILayout.Space(120);
                GUILayout.Space(50);
                if (testPhrase != dia.PanelGroup[editPanelNum].PhraseGroup.Count - 1 && GUILayout.Button("Next Phrase", GUILayout.Width(100)))
                {
                    testPhrase += 1;
                }
            }
            
        }
    }
    private void EditTestNoise()
    {
        if (testNoise != -1)
        {
            GUILayout.Space(25);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (testNoise != 0 && GUILayout.Button("Previous Noise", GUILayout.Width(120)))
                {
                    testNoise -= 1;
                }
                else if (testNoise == 0)
                    GUILayout.Space(120);
                GUILayout.Space(50);
                if (testNoise != dia.PanelGroup[editPanelNum].whiteNoises.Count - 1 && GUILayout.Button("Next Noise", GUILayout.Width(100)))
                {
                    testNoise += 1;
                }
            }

        }
    }
    #region Phrase
    private void EditPhrase(NSDBuilder.Phrase currPhrase)
    {
        
        currPhrase.phraseTypeNum = HandyFields.Popup("Phrase Type :", currPhrase.phraseTypeNum, dia.PhraseTypeStrings.ToArray(), 100);
        currPhrase.phraseType = NSDBuilder.PhraseValues[currPhrase.phraseTypeNum];

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("Is Consent", GUILayout.Width(70));
            currPhrase.isConsent = EditorGUILayout.Toggle(currPhrase.isConsent);
        }
        if (currPhrase.phraseType == NSDBuilder.Phrase.PhraseType.OnlyHit || currPhrase.phraseType == NSDBuilder.Phrase.PhraseType.PartlyHit)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Is Answer", GUILayout.Width(70));
                currPhrase.isAnswer = EditorGUILayout.Toggle(currPhrase.isAnswer);
            }

            if (currPhrase.isAnswer)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Truth Bullet Answer", GUILayout.Width(190));
                    currPhrase.answerBulletNum = EditorGUILayout.Popup(currPhrase.answerBulletNum, dia.BulletStrings());
                    currPhrase.answerBullet = dia.TruthBulletsKind[currPhrase.answerBulletNum];
                }
            }
            

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Is Lie Answer", GUILayout.Width(90));
                currPhrase.isLieAnswer = EditorGUILayout.Toggle(currPhrase.isLieAnswer);
            }

            if (currPhrase.isLieAnswer)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Lie Bullet Answer", GUILayout.Width(190));
                    currPhrase.answerBulletNum = EditorGUILayout.Popup(currPhrase.answerBulletNum, dia.LieBulletStrings());
                    currPhrase.answerBullet = dia.TruthBulletsKind[currPhrase.answerBulletNum];
                }
            }

           

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Fuck up Dialogue", GUILayout.Width(190));
                currPhrase.fuckUpDialogue = HandyFields.UnityField(currPhrase.fuckUpDialogue,190,20);
            }
        }

        currPhrase.phraseSize = HandyFields.FloatField("Size of Phrase: ", currPhrase.phraseSize, 30);

        #region Phrase Text

        using (new EditorGUILayout.HorizontalScope())
        {
            
            currPhrase.wordCount = HandyFields.IntField("Words in Phrase: ", currPhrase.wordCount, 20);
            if(currPhrase.wordCount == 0)
            {
                GUILayout.Label("A Phrase cannot have 0 words! (be empty)", GUILayout.Width(270));
                if (dia.PanelGroup[editPanelNum].wordArray.Length != 0)
                {
                    dia.PanelGroup[editPanelNum].wordArray = new string[0];
                }
                currPhrase.phraseStart = 0;
                
                currPhrase.phraseText = "";
            }
            else if (string.IsNullOrWhiteSpace(dia.PanelGroup[editPanelNum].PanelText))
            {
                GUILayout.Label("Can't make a phrase when the panel doesn't have text!", GUILayout.Width(370));
                if (dia.PanelGroup[editPanelNum].wordArray.Length != 0)
                {
                    dia.PanelGroup[editPanelNum].wordArray = new string[0];
                }
                currPhrase.phraseStart = 0;
                
                currPhrase.phraseText = "";
            }
            else
            {
                dia.PanelGroup[editPanelNum].wordArray = dia.PanelGroup[editPanelNum].PanelText.Split(' ');

            }

        }
        
        if (dia.PanelGroup[editPanelNum].wordArray.Length != 0)
        {
            string[] copy = new string[dia.PanelGroup[editPanelNum].wordArray.Length];
            for (int i = 0; i < copy.Length; i++)
            {
                copy[i] = dia.PanelGroup[editPanelNum].wordArray[i];
            }
            for (int i = 0; i < copy.Length; i++)
            {
                int dupeNum = 1;
                for (int j = 0; j < copy.Length; j++)
                {
                    if (i == j)
                        continue;
                    if (copy[i] == copy[j])
                    {
                        copy[j] += dupeNum;
                        dupeNum++;
                    }
                }
            }
            currPhrase.phraseStart = HandyFields.Popup("Start of Phrase :", currPhrase.phraseStart, copy
                , 100, 6);


            if (dia.PanelGroup[editPanelNum].wordArray.Length-1 == currPhrase.phraseStart && currPhrase.wordCount > 1)
            {
                GUILayout.Label("Can't end with a word that's before the starting one!", GUILayout.Width(370));
                currPhrase.phraseStart = 0;
                
                currPhrase.phraseText = "";
            }
            else if(currPhrase.wordCount > dia.PanelGroup[editPanelNum].wordArray.Length)
            {
                GUILayout.Label("Bruh make up your mind", GUILayout.Width(370));
                currPhrase.phraseStart = 0;
                
                currPhrase.phraseText = "";
            }
            else
            {
                if(currPhrase.phraseStart == currPhrase.phraseStart + currPhrase.wordCount)
                {
                    currPhrase.phraseText = dia.PanelGroup[editPanelNum].wordArray[currPhrase.phraseStart];
                    GUILayout.Label("==" + currPhrase.phraseText + "==", GUILayout.Width(300));
                }
                else
                {
                    try
                    {
                        currPhrase.phraseText = string.Join(" ", dia.PanelGroup[editPanelNum].wordArray, currPhrase.phraseStart,
                        currPhrase.wordCount);
                        GUILayout.Label("==" + currPhrase.phraseText + "==", GUILayout.Width(700));
                    }
                    catch
                    {
                        currPhrase.phraseStart = 0;
                        currPhrase.phraseText = "";
                    }
                }
                
            }
        }
        using(new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("");
        }
        #endregion

        #region Partly Hitable

        if (currPhrase.phraseType == NSDBuilder.Phrase.PhraseType.PartlyHit)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("allExcept", GUILayout.Width(70));
                currPhrase.allExcept = EditorGUILayout.Toggle(currPhrase.allExcept);
            }

            using (new EditorGUILayout.HorizontalScope())
            {

                currPhrase.partlyHitableWordCount = HandyFields.IntField("Hitable Words in Phrase: ", currPhrase.partlyHitableWordCount, 20);
                if (currPhrase.partlyHitableWordCount == 0)
                {
                    GUILayout.Label("A Phrase cannot have 0 words! (be empty)", GUILayout.Width(270));
                    if (currPhrase.partlyHitableWordArray != null || currPhrase.partlyHitableWordArray.Length != 0)
                    {
                        currPhrase.partlyHitableWordArray = new string[0];
                    }
                    currPhrase.partlyHitableWordStart = 0;
                    currPhrase.partlyHitablePhraseText = "";
                }
                else if (string.IsNullOrWhiteSpace(dia.PanelGroup[editPanelNum].PanelText))
                {
                    GUILayout.Label("Can't make a phrase when the panel doesn't have text!", GUILayout.Width(370));
                    if (currPhrase.partlyHitableWordArray.Length != 0)
                    {
                        currPhrase.partlyHitableWordArray = new string[0];
                    }
                    currPhrase.partlyHitableWordStart = 0;
                    currPhrase.partlyHitablePhraseText = "";
                }
                else
                {
                    currPhrase.partlyHitableWordArray = currPhrase.phraseText.Split(' ');
                }
            }

            if (currPhrase.partlyHitableWordArray.Length != 0)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Start of Hitable Phrase", GUILayout.Width(150));
                    string[] copy = new string[currPhrase.partlyHitableWordArray.Length];
                    for (int i = 0; i < copy.Length; i++)
                    {
                        copy[i] = currPhrase.partlyHitableWordArray[i];
                    }
                    for (int i = 0; i < copy.Length; i++)
                    {
                        int dupeNum = 1;
                        for(int j = 0; j < copy.Length; j++)
                        {
                            if (i == j)
                                continue;
                            if(copy[i] == copy[j])
                            {
                                copy[j] += dupeNum;
                                dupeNum++;
                            }
                        }
                    }
                    currPhrase.partlyHitableWordStart = EditorGUILayout.Popup(currPhrase.partlyHitableWordStart,
                        copy);
                }


                if (currPhrase.partlyHitableWordArray.Length - 1 == currPhrase.partlyHitableWordStart && currPhrase.partlyHitableWordCount > 1)
                {
                    GUILayout.Label("Can't end with a word that's before the starting one!", GUILayout.Width(370));
                    currPhrase.partlyHitableWordStart = 0;
                    currPhrase.partlyHitablePhraseText = "";
                }
                else if (currPhrase.partlyHitableWordCount > currPhrase.partlyHitableWordArray.Length)
                {
                    GUILayout.Label("Bruh make up your mind", GUILayout.Width(370));
                    currPhrase.partlyHitableWordStart = 0;
                    currPhrase.partlyHitablePhraseText = "";
                    currPhrase.partlyHitableWordCount = 1;
                }
                else
                {
                    if (currPhrase.partlyHitableWordStart == currPhrase.partlyHitableWordStart + currPhrase.partlyHitableWordCount)
                    {
                        currPhrase.partlyHitablePhraseText = dia.PanelGroup[editPanelNum].wordArray[currPhrase.partlyHitableWordStart];
                        GUILayout.Label("==" + currPhrase.partlyHitablePhraseText + "==", GUILayout.Width(300));
                    }
                    else
                    {
                        try
                        {
                            currPhrase.partlyHitablePhraseText = string.Join(" ", currPhrase.partlyHitableWordArray, currPhrase.partlyHitableWordStart,
                            currPhrase.partlyHitableWordCount);
                            GUILayout.Label("==" + currPhrase.partlyHitablePhraseText + "==", GUILayout.Width(300));
                        }
                        catch
                        {
                            currPhrase.partlyHitableWordStart = 0;
                            currPhrase.partlyHitablePhraseText = "";
                        }
                    }

                }
                
            }
        }


        #endregion

        PhraseTrack(currPhrase);

        if(currPhrase.anim.animations.Count > 0)
        {
            currPhrase.anim.spawnPoint = HandyFields.Vector3Field("Spawn Point: ", currPhrase.anim.spawnPoint, 200, 7);
            currPhrase.anim.spawnAngle = HandyFields.Vector4Field("Spawn Angle: ", currPhrase.anim.spawnAngle, 200, 7);

            //currPhrase.phraseAnim.duration = HandyFields.FloatField("Duration: ", currPhrase.phraseAnim.duration);

            
            
            for (int i = 0; i < currPhrase.anim.animations.Count; i++) // Display all Phrase Animation
            {
                EditPhraseAnimation(currPhrase,currPhrase.anim.animations[i],i);
            }
        }

        if (GUILayout.Button("Add Animation"))
        {
            currPhrase.anim.animations.Add(new NSDBuilder.PhraseAnimation.AnimData());
        }
        GUILayout.Space(15);

        if (GUILayout.Button("Remove Phrase"))
        {
            if (EditorUtility.DisplayDialog("Remove Phrase", "Are you sure you want to Delete this Phrase?",
                    "Yes", "No"))
            {
                dia.PanelGroup[editPanelNum].PhraseGroup.Remove(currPhrase);
                if (testPhrase != -1 && testPhrase != 0)
                {
                    testPhrase -= 1;
                    return;
                }
            }
        }
        GUILayout.Space(25);
    }
    #endregion

    private void PhraseTrack(NSDBuilder.Phrase currPhrase)
    {
        currPhrase.path.position = HandyFields.Vector3Field("Track Position: ", currPhrase.path.position, 200, 7);
        currPhrase.path.rotation = HandyFields.Vector4Field("Track Angle: ", currPhrase.path.rotation, 200, 7);
        using(new GUILayout.HorizontalScope())
        {
            //if (currPhrase.path == (Nullable<typeof(NSDBuilder.Phrase.Path)>))

            if (GUILayout.Button("Import Selection", GUILayout.Width(130)))
            {
                //Debug.Log(currPhrase.path.points != null);
                //Debug.Log(Selection.gameObjects[0].GetComponent<Cinemachine.CinemachinePath>().m_Waypoints);
                CinemachinePath.Waypoint[] points = Selection.gameObjects[0].GetComponent<CinemachinePath>().m_Waypoints;
                if (points != null && points.Length > 0)
                    currPhrase.path.points = points;
                else
                    Debug.Log("You need to select a Path in the hierarchy!");
                //Debug.Log(currPhrase.path.points[0].position);
            }
            if (GUILayout.Button("Clear Waypoints", GUILayout.Width(130)))
            {
                currPhrase.path.points = null;
            }

            if(currPhrase.path.speedCurve == null)
            {
                ResetTrackCurve(currPhrase);
            }

            currPhrase.path.speedCurve = EditorGUILayout.CurveField(currPhrase.path.speedCurve);

            if (GUILayout.Button("Reset Curve", GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Reset Curve", "Are you sure you want to Reset this Curve?",
                    "Yes", "No"))
                {
                    ResetTrackCurve(currPhrase);
                }
            }

        }

        GUILayout.Space(25);
    }
    void ResetTrackCurve(NSDBuilder.Phrase currPhrase)
    {
        currPhrase.path.speedCurve = new AnimationCurve()
        {
            keys = new Keyframe[] {
                        new Keyframe(0,1), new Keyframe(0.5f, 0.09f), new Keyframe(1, 1) }
        };
    }
    private void PhraseTrack(NSDBuilder.WhiteNoise currPhrase)
    {
        currPhrase.path.position = HandyFields.Vector3Field("Track Position: ", currPhrase.path.position, 200, 7);
        currPhrase.path.rotation = HandyFields.Vector4Field("Track Angle: ", currPhrase.path.rotation, 200, 7);
        using (new GUILayout.HorizontalScope())
        {
            //if (currPhrase.path == (Nullable<typeof(NSDBuilder.Phrase.Path)>))

            if (GUILayout.Button("Import Selection", GUILayout.Width(130)))
            {
                //Debug.Log(currPhrase.path.points != null);
                //Debug.Log(Selection.gameObjects[0].GetComponent<Cinemachine.CinemachinePath>().m_Waypoints);
                CinemachinePath.Waypoint[] points = Selection.gameObjects[0].GetComponent<CinemachinePath>().m_Waypoints;
                if (points != null && points.Length > 0)
                    currPhrase.path.points = points;
                else
                    Debug.Log("You need to select a Path in the hierarchy!");
                //Debug.Log(currPhrase.path.points[0].position);
            }
            if (GUILayout.Button("Clear Waypoints", GUILayout.Width(130)))
            {
                currPhrase.path.points = null;
            }

            if (currPhrase.path.speedCurve == null)
            {
                ResetTrackCurve(currPhrase);
            }

            currPhrase.path.speedCurve = EditorGUILayout.CurveField(currPhrase.path.speedCurve);

            if (GUILayout.Button("Reset Curve", GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Reset Curve", "Are you sure you want to Reset this Curve?",
                    "Yes", "No"))
                {
                    ResetTrackCurve(currPhrase);
                }
            }

        }

        GUILayout.Space(25);
    }
    void ResetTrackCurve(NSDBuilder.WhiteNoise currPhrase)
    {
        currPhrase.path.speedCurve = new AnimationCurve()
        {
            keys = new Keyframe[] {
                        new Keyframe(0,1), new Keyframe(0.5f, 0.09f), new Keyframe(1, 1) }
        };
    }
    #region Phrase Anim
    private void EditPhraseAnimation(NSDBuilder.Phrase currPhrase,NSDBuilder.PhraseAnimation.AnimData anim, int i)
    {
        using (new EditorGUILayout.VerticalScope("Box"))
        {
            
            EditorGUILayout.LabelField("Animation " + (i + 1) + ": ");
            anim.animTypeNum = EditorGUILayout.Popup(anim.animTypeNum, NSDBuilder.PhraseAnimation.AnimTypeStrings.ToArray(), GUILayout.Width(100));
            anim.currentAnimType = currPhrase.anim.GetAnimType[anim.animTypeNum];

            DisplayAnim(anim, currPhrase);


            if (GUILayout.Button("Remove Animation"))
            {
                if (EditorUtility.DisplayDialog("Remove Animation", "Are you sure you want to Delete this Animation?",
                    "Yes", "No"))
                {
                    currPhrase.anim.animations.Remove(anim);
                }
            }
        }
        
    }
    private void EditWNPhraseAnimation(NSDBuilder.WhiteNoise currPhrase, NSDBuilder.PhraseAnimation.AnimData anim, int i)
    {
        using (new EditorGUILayout.VerticalScope("Box"))
        {

            EditorGUILayout.LabelField("Animation " + (i + 1) + ": ");
            anim.animTypeNum = EditorGUILayout.Popup(anim.animTypeNum, NSDBuilder.PhraseAnimation.AnimTypeStrings.ToArray(), GUILayout.Width(100));
            anim.currentAnimType = currPhrase.panim.GetAnimType[anim.animTypeNum];

            DisplayAnim(anim, null);


            if (GUILayout.Button("Remove Animation"))
            {
                if (EditorUtility.DisplayDialog("Remove Animation", "Are you sure you want to Delete this Animation?",
                    "Yes", "No"))
                {
                    currPhrase.panim.animations.Remove(anim);
                }
            }
        }

    }
    private void DisplayAnim(NSDBuilder.PhraseAnimation.AnimData anim, NSDBuilder.Phrase currPhrase)
    {
        switch (anim.currentAnimType)
        {
            case NSDBuilder.PhraseAnimation.AnimType.Transform:
                DisplayTransform(anim,currPhrase);
                break;

            case NSDBuilder.PhraseAnimation.AnimType.Rotation:
                DisplayRotation(anim, currPhrase);
                break;

            case NSDBuilder.PhraseAnimation.AnimType.Scale:
                DisplayScale(anim, currPhrase);
                break;

            case NSDBuilder.PhraseAnimation.AnimType.Fade:
                DisplayFade(anim, currPhrase);
                break;

            case NSDBuilder.PhraseAnimation.AnimType.CharacterByCharacter:
                DisplayCharByChar(anim, currPhrase);
                break;
        }

    }
    private void DisplayTransform(NSDBuilder.PhraseAnimation.AnimData anim, NSDBuilder.Phrase currPhrase)
    {
        anim.tStruct.startPosition = HandyFields.Vector3Field("Starting Position: ", anim.tStruct.startPosition);
        anim.tStruct.endPosition = HandyFields.Vector3Field("Ending Position: ", anim.tStruct.endPosition);
        
        using(new EditorGUILayout.HorizontalScope())
        {
            anim.tStruct.startTime = HandyFields.FloatField("Start Time: ", anim.tStruct.startTime,30);
            if(anim.tStruct.startTime < 0)
            {
                anim.tStruct.startTime = 0;
            }
            float fallBackDuration = anim.tStruct.duration;
            anim.tStruct.duration = HandyFields.FloatField("Duration: ", anim.tStruct.duration,30);
            if(anim.tStruct.duration <= 0)
            {
                if (fallBackDuration == 0)
                    anim.tStruct.duration = 1;
                else
                    anim.tStruct.duration = fallBackDuration;
            }
            if(anim.animationCurves.Count != 0)
            {
                if (GUILayout.Button("Update Curves"))
                {
                    anim.animationCurves[0] = UpdateKey(anim.animationCurves[0], anim.tStruct.startPosition.x, anim.tStruct.endPosition.x
                        , anim.tStruct.startTime, anim.tStruct.duration);
                    anim.animationCurves[1] = UpdateKey(anim.animationCurves[1], anim.tStruct.startPosition.y, anim.tStruct.endPosition.y
                        , anim.tStruct.startTime, anim.tStruct.duration);
                    anim.animationCurves[2] = UpdateKey(anim.animationCurves[2], anim.tStruct.startPosition.z, anim.tStruct.endPosition.z
                        , anim.tStruct.startTime, anim.tStruct.duration);
                }

                if (GUILayout.Button("Reset Curves"))
                {
                    anim.animationCurves.Clear();
                    anim.animationCurves.Add(new AnimationCurve());
                    anim.animationCurves.Add(new AnimationCurve());
                    anim.animationCurves.Add(new AnimationCurve());
                }
            }
        }

        if (anim.animationCurves.Count == 0)
        {
            if (GUILayout.Button("Generate Curves"))
            {
                anim.animationCurves.Add(new AnimationCurve());
                anim.animationCurves.Add(new AnimationCurve());
                anim.animationCurves.Add(new AnimationCurve());
            }
        }
        else
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                anim.animationCurves[0] = DisplayCurve(anim.animationCurves[0], anim.tStruct.startPosition.x, anim.tStruct.endPosition.x
                    , anim.tStruct.startTime, anim.tStruct.duration);
                anim.animationCurves[1] = DisplayCurve(anim.animationCurves[1], anim.tStruct.startPosition.y, anim.tStruct.endPosition.y
                    , anim.tStruct.startTime, anim.tStruct.duration);
                anim.animationCurves[2] = DisplayCurve(anim.animationCurves[2], anim.tStruct.startPosition.z, anim.tStruct.endPosition.z
                    , anim.tStruct.startTime, anim.tStruct.duration);
            }
            
        }
    }
    private void DisplayRotation(NSDBuilder.PhraseAnimation.AnimData anim, NSDBuilder.Phrase currPhrase)
    {
        anim.rStruct.startRotation = HandyFields.Vector4Field("Starting Position: ", anim.rStruct.startRotation);
        anim.rStruct.endRotation = HandyFields.Vector4Field("Ending Position: ", anim.rStruct.endRotation);

        using (new EditorGUILayout.HorizontalScope())
        {
            anim.rStruct.startTime = HandyFields.FloatField("Start Time: ", anim.rStruct.startTime, 30);
            if (anim.rStruct.startTime < 0)
            {
                anim.rStruct.startTime = 0;
            }
            float fallBackDuration = anim.rStruct.duration;
            anim.rStruct.duration = HandyFields.FloatField("Duration: ", anim.rStruct.duration, 30);
            if (anim.rStruct.duration <= 0)
            {
                if (fallBackDuration == 0)
                    anim.rStruct.duration = 1;
                else
                    anim.rStruct.duration = fallBackDuration;
            }

            if (anim.animationCurves.Count != 0)
            {
                if (GUILayout.Button("Update Curves"))
                {
                    anim.animationCurves[0] = UpdateKey(anim.animationCurves[0], anim.rStruct.startRotation.x, anim.rStruct.endRotation.x
                        , anim.rStruct.startTime, anim.rStruct.duration);
                    anim.animationCurves[1] = UpdateKey(anim.animationCurves[1], anim.rStruct.startRotation.y, anim.rStruct.endRotation.y
                        , anim.rStruct.startTime, anim.rStruct.duration);
                    anim.animationCurves[2] = UpdateKey(anim.animationCurves[2], anim.rStruct.startRotation.z, anim.rStruct.endRotation.z
                        , anim.rStruct.startTime, anim.rStruct.duration);
                    anim.animationCurves[3] = UpdateKey(anim.animationCurves[3], anim.rStruct.startRotation.w, anim.rStruct.endRotation.w
                        , anim.rStruct.startTime, anim.rStruct.duration);
                }

                if (GUILayout.Button("Reset Curves"))
                {
                    anim.animationCurves.Clear();
                    anim.animationCurves.Add(new AnimationCurve());
                    anim.animationCurves.Add(new AnimationCurve());
                    anim.animationCurves.Add(new AnimationCurve());
                    anim.animationCurves.Add(new AnimationCurve());
                }
            }
        }

        if (anim.animationCurves.Count == 0)
        {
            if (GUILayout.Button("Generate Curves"))
            {
                anim.animationCurves.Add(new AnimationCurve());
                anim.animationCurves.Add(new AnimationCurve());
                anim.animationCurves.Add(new AnimationCurve());
                anim.animationCurves.Add(new AnimationCurve());
            }
        }
        else
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                anim.animationCurves[0] = DisplayCurve(anim.animationCurves[0], anim.rStruct.startRotation.x, anim.rStruct.endRotation.x
                        , anim.rStruct.startTime, anim.rStruct.duration);
                anim.animationCurves[1] = DisplayCurve(anim.animationCurves[1], anim.rStruct.startRotation.y, anim.rStruct.endRotation.y
                    , anim.rStruct.startTime, anim.rStruct.duration);
                anim.animationCurves[2] = DisplayCurve(anim.animationCurves[2], anim.rStruct.startRotation.z, anim.rStruct.endRotation.z
                    , anim.rStruct.startTime, anim.rStruct.duration);
                anim.animationCurves[3] = DisplayCurve(anim.animationCurves[3], anim.rStruct.startRotation.w, anim.rStruct.endRotation.w
                    , anim.rStruct.startTime, anim.rStruct.duration);
            }

        }
    }
    private void DisplayScale(NSDBuilder.PhraseAnimation.AnimData anim, NSDBuilder.Phrase currPhrase)
    {
        anim.sStruct.startScale = HandyFields.Vector3Field("Starting Scale: ", anim.sStruct.startScale);
        anim.sStruct.endScale = HandyFields.Vector3Field("Ending Scale: ", anim.sStruct.endScale);

        using (new EditorGUILayout.HorizontalScope())
        {
            anim.sStruct.startTime = HandyFields.FloatField("Start Time: ", anim.sStruct.startTime, 30);
            if (anim.sStruct.startTime < 0)
            {
                anim.sStruct.startTime = 0;
            }
            float fallBackDuration = anim.sStruct.duration;
            anim.sStruct.duration = HandyFields.FloatField("Duration: ", anim.sStruct.duration, 30);
            if (anim.sStruct.duration <= 0)
            {
                if (fallBackDuration == 0)
                    anim.sStruct.duration = 1;
                else
                    anim.sStruct.duration = fallBackDuration;
            }

            if (anim.animationCurves.Count != 0)
            {
                if (GUILayout.Button("Update Curves"))
                {
                    anim.animationCurves[0] = UpdateKey(anim.animationCurves[0], anim.sStruct.startScale.x, anim.sStruct.endScale.x
                        , anim.sStruct.startTime, anim.sStruct.duration);
                    anim.animationCurves[1] = UpdateKey(anim.animationCurves[1], anim.sStruct.startScale.y, anim.sStruct.endScale.y
                        , anim.sStruct.startTime, anim.sStruct.duration);
                    anim.animationCurves[2] = UpdateKey(anim.animationCurves[2], anim.sStruct.startScale.z, anim.sStruct.endScale.z
                        , anim.sStruct.startTime, anim.sStruct.duration);
                }

                if (GUILayout.Button("Reset Curves"))
                {
                    anim.animationCurves.Clear();
                    anim.animationCurves.Add(new AnimationCurve());
                    anim.animationCurves.Add(new AnimationCurve());
                    anim.animationCurves.Add(new AnimationCurve());
                }
            }
        }

        if(anim.sStruct.startScale == new Vector3(0, 0, 0))
        {
            anim.sStruct.startScale = new Vector3(1, 1, 1);
            anim.sStruct.endScale = new Vector3(1, 1, 1);
        }

        if (anim.animationCurves.Count == 0)
        {
            if (GUILayout.Button("Generate Curves"))
            {
                anim.animationCurves.Add(new AnimationCurve());
                anim.animationCurves.Add(new AnimationCurve());
                anim.animationCurves.Add(new AnimationCurve());
            }
        }
        else
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                anim.animationCurves[0] = DisplayCurve(anim.animationCurves[0], anim.sStruct.startScale.x, anim.sStruct.endScale.x
                        , anim.sStruct.startTime, anim.sStruct.duration);
                anim.animationCurves[1] = DisplayCurve(anim.animationCurves[1], anim.sStruct.startScale.y, anim.sStruct.endScale.y
                    , anim.sStruct.startTime, anim.sStruct.duration);
                anim.animationCurves[2] = DisplayCurve(anim.animationCurves[2], anim.sStruct.startScale.z, anim.sStruct.endScale.z
                    , anim.sStruct.startTime, anim.sStruct.duration);
            }

        }
    }
    private void DisplayFade(NSDBuilder.PhraseAnimation.AnimData anim, NSDBuilder.Phrase currPhrase)
    {
        anim.fStruct.startFade = HandyFields.FloatField("Starting Fade: ", anim.fStruct.startFade);
        anim.fStruct.endFade = HandyFields.FloatField("Ending Fade: ", anim.fStruct.endFade);

        using (new EditorGUILayout.HorizontalScope())
        {
            anim.fStruct.startTime = HandyFields.FloatField("Start Time: ", anim.fStruct.startTime, 30);
            if (anim.fStruct.startTime < 0)
            {
                anim.fStruct.startTime = 0;
            }
            float fallBackDuration = anim.fStruct.duration;
            anim.fStruct.duration = HandyFields.FloatField("Duration: ", anim.fStruct.duration, 30);
            if (anim.fStruct.duration <= 0)
            {
                if (fallBackDuration == 0)
                    anim.fStruct.duration = 1;
                else
                    anim.fStruct.duration = fallBackDuration;
            }

            if (anim.animationCurves.Count != 0)
            {
                if (GUILayout.Button("Update Curves"))
                {
                    anim.animationCurves[0] = UpdateKey(anim.animationCurves[0], anim.fStruct.startFade, anim.fStruct.endFade
                        , anim.fStruct.startTime, anim.fStruct.duration);
                }

                if (GUILayout.Button("Reset Curves"))
                {
                    anim.animationCurves.Clear();
                    anim.animationCurves.Add(new AnimationCurve());
                }
            }
        }

        if (anim.animationCurves.Count == 0)
        {
            if (GUILayout.Button("Generate Curves"))
            {
                anim.animationCurves.Add(new AnimationCurve());
            }
        }
        else
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                anim.animationCurves[0] = DisplayCurve(anim.animationCurves[0], anim.fStruct.startFade, anim.fStruct.endFade
                        , anim.fStruct.startTime, anim.fStruct.duration);
            }

        }
    }
    private void DisplayCharByChar(NSDBuilder.PhraseAnimation.AnimData anim, NSDBuilder.Phrase currPhrase)
    {
        anim.cBCAnim = HandyFields.UnityField<CBCAnim>(anim.cBCAnim, 140, 20);
    }
    private AnimationCurve UpdateKey(AnimationCurve curve, float start, float end,float startTime, float duration)
    {
        Keyframe oldStartTimeKey = curve.keys[0];
        Keyframe oldDurationTimeKey = curve.keys[curve.keys.Length - 1];

        
        

        curve.keys = ValidateCurves(oldStartTimeKey, oldDurationTimeKey, curve, start, end, startTime, duration);


        return curve;
    }
    private AnimationCurve DisplayCurve(AnimationCurve curve,float start,float end,float startTime, float duration)
    {
        if(curve.keys.Length == 0)
        {
            curve.AddKey(startTime,start);
            curve.AddKey(startTime + duration,end);
        }
        
        curve = EditorGUILayout.CurveField(curve);
        return curve;
    }
    private Keyframe[] ValidateCurves(Keyframe oldStart, Keyframe oldDuration, AnimationCurve curve, float start, float end, float startTime, float duration)
    {
        Keyframe newStart = new Keyframe(startTime, start);
        Keyframe newDuration = new Keyframe(startTime + duration, end);
        if(curve.keys[curve.keys.Length - 1].time == newDuration.time && curve.keys[0].time == newStart.time)
        {
            if(curve.keys[curve.keys.Length-1].value == newDuration.value && curve.keys[0].value == newStart.value)
            {
                return curve.keys;
            }
            else
            {
                curve.MoveKey(0, new Keyframe(0, start));
                curve.MoveKey(curve.keys.Length-1, new Keyframe(startTime+duration, end));
                return curve.keys;
            }
        }

        Keyframe[] newKeys = new Keyframe[curve.keys.Length];
        float startOffset = startTime - oldStart.time;
        float durationOffset = (startTime + duration) - oldDuration.time;
        if (curve.keys.Length > 2)
        {
            for (int i = 1; i <= newKeys.Length - 2; i++)
            {
                
                if (curve.keys[i].time <= startTime)
                {
                    //Debug.Log(startOffset);
                    newKeys[i] = new Keyframe(curve.keys[i].time + startOffset, curve.keys[i].value);
                    //Debug.Log("Start Offset Key frame at " + i + " moved to " + newKeys[i].time);
                }
                else if(curve.keys[i].time >= startTime + duration)
                {
                    //Debug.Log(durationOffset);
                    newKeys[i] = new Keyframe(curve.keys[i].time + durationOffset, curve.keys[i].value);
                    //Debug.Log("Duration Offset Key frame at " + i + " moved to " + curve.keys[i].time);
                }
                else
                {
                    float behind = curve.keys[i].time - curve.keys[i - 1].time;
                    newKeys[i] = new Keyframe(curve.keys[i].time + behind, curve.keys[i].value);
                }
            }
        }
        newKeys[0] = new Keyframe(startTime, start);
        newKeys[curve.keys.Length - 1] = new Keyframe(startTime + duration, end);
        return newKeys;
    }

    #endregion
    /*
    private void AnimationImportation(NSDBuilder.Phrase p)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            clip = HandyFields.UnityField(clip, 170, 30);

            if (p.clipAnim.Count != 0 && GUILayout.Button("Export Animation"))
            {
                AnimationClip c = new AnimationClip();
                foreach (NSDBuilder.WNAnim pAnim in p.clipAnim)
                {
                    c.SetCurve(pAnim.path, typeof(RectTransform), pAnim.propertyName, pAnim.curve);
                }
                AssetDatabase.CreateAsset(c, "Assets/Scriptable Objects/Minigames/NSD/WhiteNoiseTest.anim");
            }
            if (GUILayout.Button("Import Animation"))
            {
                if (clip != null)
                {
                    p.clipAnim.Clear();
                    EditorCurveBinding[] info = AnimationUtility.GetCurveBindings(clip);
                    for (int i = 0; i < info.Length; i++)
                    {
                        p.clipAnim.Add(new NSDBuilder.WNAnim());
                        p.clipAnim[i].path = info[i].path;
                        p.clipAnim[i].propertyName = info[i].propertyName;
                        p.clipAnim[i].curve = AnimationUtility.GetEditorCurve(clip, info[i]);
                    }
                }
            }

            if (p.clipAnim.Count != 0)
            {
                if (GUILayout.Button("Clear Animation"))
                {
                    p.clipAnim.Clear();
                }
            }
        }
    }
    */
    #region WhiteNoise
    AnimationClip clip;
    private void EditWhiteNoise(NSDBuilder.WhiteNoise wn)
    {
        wn.text = HandyFields.StringArea("Text: ", wn.text);
        wn.size = HandyFields.FloatField("Size: ", wn.size, 30);
        using (new EditorGUILayout.HorizontalScope())
        {
            clip = HandyFields.UnityField(clip, 170, 30);

            if (wn.anim.Count != 0 && GUILayout.Button("Export Animation"))
            {
                AnimationClip c = new AnimationClip();
                foreach (NSDBuilder.WNAnim wnAnim in wn.anim)
                {
                    c.SetCurve(wnAnim.path, typeof(RectTransform), wnAnim.propertyName, wnAnim.curve);
                }
                AssetDatabase.CreateAsset(c, "Assets/Scriptable Objects/Minigames/NSD/WhiteNoiseTest.anim");
            }
            if (GUILayout.Button("Import Animation"))
            {
                if (clip != null)
                {
                    wn.anim.Clear();
                    EditorCurveBinding[] info = AnimationUtility.GetCurveBindings(clip);
                    for (int i = 0; i < info.Length; i++)
                    {
                        wn.anim.Add(new NSDBuilder.WNAnim());
                        wn.anim[i].path = info[i].path;
                        wn.anim[i].propertyName = info[i].propertyName;
                        wn.anim[i].curve = AnimationUtility.GetEditorCurve(clip, info[i]);
                    }
                }
            }

            if (wn.anim.Count != 0)
            {
                if (GUILayout.Button("Clear Animation"))
                {
                    wn.anim.Clear();
                }
            }
        }
        GUILayout.Space(15);
        PhraseTrack(wn);
        GUILayout.Space(15);
        if (wn.panim.animations.Count > 0)
        {
            wn.panim.spawnPoint = HandyFields.Vector3Field("Spawn Point: ", wn.panim.spawnPoint, 200, 7);
            wn.panim.spawnAngle = HandyFields.Vector4Field("Spawn Angle: ", wn.panim.spawnAngle, 200, 7);

            //currPhrase.phraseAnim.duration = HandyFields.FloatField("Duration: ", currPhrase.phraseAnim.duration);



            for (int i = 0; i < wn.panim.animations.Count; i++) // Display all Phrase Animation
            {
                EditWNPhraseAnimation(wn, wn.panim.animations[i], i);
            }
        }
        if (GUILayout.Button("Add Animation"))
        {
            wn.panim.animations.Add(new NSDBuilder.PhraseAnimation.AnimData());
        }
        GUILayout.Space(15);
        if (GUILayout.Button("Remove White Noise"))
        {
            if (EditorUtility.DisplayDialog("Remove Noise", "Are you sure you want to Delete this White Noise?",
                    "Yes", "No"))
            {
                dia.PanelGroup[editPanelNum].whiteNoises.Remove(wn);
                if (testNoise != -1 && testNoise != 0)
                {
                    testNoise -= 1;
                    return;
                }
            }
            
        }
    }


    #endregion

    #endregion
}
public class BuilderEditor
{
    public static TimerDiff[] DisplayTimerSettings(TimerDiff[] b)
    {
        for(int i = 0; i < b.Length; i++)
        {
            EditorGUILayout.LabelField("Time for " + b[i].difficulty.ToString());
            using (new GUILayout.HorizontalScope())
            {
                b[i].min = HandyFields.FloatField("Minutes: ", b[i].min, 100);
                b[i].sec = HandyFields.FloatField("Seconds: ", b[i].sec, 100);
            }
        }
        GUILayout.Space(25);
        return b;
    }
}
