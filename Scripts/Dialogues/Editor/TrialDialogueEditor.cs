using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using DREditor.Characters;
using DREditor.Utility;
using DREditor.Utility.Editor;

namespace DREditor.Dialogues.Editor
{
    [CustomEditor(typeof(TrialDialogue))]
    public class TrialDialogueEditor : DialogueEditorBase
    {
        private TrialDialogue dia;
        private TrialCameraVFXDatabase cameraVFXDatabase;
        private TrialCameraAnimDatabase cameraAnimDatabase;

        private SerializedProperty propLines;

        public void OnEnable()
        {
            dia = target as TrialDialogue;
            propLines = serializedObject.FindProperty("Lines");
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
                !ValidateCameraVFXDatabase() || !ValidateCameraVFXs()) { return; }

            EditorStyles.textArea.wordWrap = true;
            EditorStyles.textField.wordWrap = true;
            GUI.backgroundColor = dia.Color;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(dia.DialogueName, EditorStyles.boldLabel);
                dia.Color = EditorGUILayout.ColorField(dia.Color, GUILayout.Width(50));
            }
            EditHeader();
            EditPanels();
            EditFooter();
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

        private void EditPanels()
        {
            if (dia.Lines == null)
            {
                return;
            }
            for (int i = 0; i < dia.Lines.Count; i++)
            {
                var currLine = dia.Lines[i];
                EditStudentBGColor(currLine);

                using (new EditorGUILayout.HorizontalScope("Box"))
                {
                    EditLeftPanel(currLine);
                    EditExpressionPanel(currLine);
                    EditDialogueText(i);
                    EditDialoguePosition(i);
                }
            }
        }

        private void EditLeftPanel(TrialLine currLine)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(120)))
            {
                GUI.backgroundColor = dia.Color;
                EditSpeaker(currLine);
                EditSFX(currLine);
                EditEvents(currLine);
                EditCamVFX(currLine);
                EditCamAnim(currLine);
                EditAutomatic(currLine);
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
                GUILayout.Label("Automatic", GUILayout.Width(60));
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
                                PublicAudioUtil.PlayClip(sfx);
                            }
                        }

                        currLine.SFX[j] = HandyFields.UnityField(sfx, 76);

                        if (GUILayout.Button("x", GUILayout.Width(20)))
                        {
                            currLine.SFX.Remove(sfx);
                        }
                    }
                }
            }

            if (GUILayout.Button("Add Sound"))
            {
                currLine.SFX.Add(_sfx);
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
        private void EditDialogueText(int currLineIdx)
        {
            GUI.backgroundColor = Color.white;
            var propLine = propLines.GetArrayElementAtIndex(currLineIdx);
            var propLineText = propLine.FindPropertyRelative("Text");
            propLineText.stringValue = EditorGUILayout.TextArea(propLineText.stringValue, GUILayout.Height(125), GUILayout.Width(Screen.width - 310));
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
            if (GUILayout.Button("New Line", GUILayout.Width(100)))
            {
                dia.Lines.Add(new TrialLine());
            }
        }
    }
}