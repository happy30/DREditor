using EventObjects;
using UnityEngine;
using UnityEditor;

using DREditor.Characters;
using DREditor.Utility;

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

        public void OnEnable()
        {
            dia = (Dialogue)target;
            propLines = serializedObject.FindProperty("Lines");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CreateForm();
            EditorUtility.SetDirty(dia);
            serializedObject.ApplyModifiedProperties();
        }

        void CreateForm()
        {
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
                    return;
                }
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
            GUI.backgroundColor = dia.Color;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(dia.DialogueName, EditorStyles.boldLabel);
                dia.Color = EditorGUILayout.ColorField(dia.Color, GUILayout.Width(50));
            }

            using (new EditorGUILayout.HorizontalScope("Box"))
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

            if (dia.Lines != null)
            {
                for (int i = 0; i < dia.Lines.Count; i++)
                {
                    var currLine = dia.Lines[i];
                    var color = Color.white;

                    if (dia.Lines[i].Speaker is Student)
                    {
                        var stu = dia.Lines[i].Speaker as Student;

                        float H, S, V;


                        color = stu.StudentCard.Color;
                        Color.RGBToHSV(color, out H, out S, out V);
                        S = 0.3f;
                        V = 0.95f;

                        color = Color.HSVToRGB(H, S, V);

                    }
                    else
                    {
                        GUI.backgroundColor = Color.white;
                    }

                    GUI.backgroundColor = color;
                    using (new EditorGUILayout.HorizontalScope("Box"))
                    {
                        using (new EditorGUILayout.VerticalScope(GUILayout.Width(120)))
                        {
                            GUI.backgroundColor = dia.Color;
                            var prependedArray = ContainerUtil.PrependedList(dia.GetCharacterNames(), "<No Character>");
                            currLine.SpeakerNumber = EditorGUILayout.IntPopup(currLine.SpeakerNumber, prependedArray,
                                ContainerUtil.Iota(prependedArray.Length, -1), GUILayout.Width(130));
                            currLine.Speaker = currLine.SpeakerNumber == -1 ? null : dia.Speakers.Characters[currLine.SpeakerNumber];

                            if (dia.Lines[i].Speaker)
                            {
                                var aliasNames = new string[dia.Lines[i].Speaker.Aliases.Count + 1];
                                aliasNames[0] = "(No Alias)";

                                for (int j = 1; j < dia.Lines[i].Speaker.Aliases.Count + 1; j++)
                                {
                                    aliasNames[j] = dia.Lines[i].Speaker.Aliases[j - 1].Name;
                                }
                                dia.Lines[i].AliasNumber = EditorGUILayout.IntPopup(dia.Lines[i].AliasNumber,
                                    aliasNames, dia.getAliasesIntValues(dia.Lines[i].Speaker),
                                        GUILayout.Width(130));
                            }

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.Label("Voice", GUILayout.Width(35));
                                dia.Lines[i].VoiceSFX =
                                    EditorGUILayout.ObjectField(dia.Lines[i].VoiceSFX, typeof(AudioClip), false,
                                    GUILayout.Width(90)) as AudioClip;
                            }

                            if (dia.Lines[i].SFX != null)
                            {
                                for (var j = 0; j < dia.Lines[i].SFX.Count; j++)
                                {
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUI.BeginDisabledGroup(dia.Lines[i].SFX[j] == null);

                                        if (GUILayout.Button(">", GUILayout.Width(20)))
                                        {
                                            PublicAudioUtil.PlayClip(dia.Lines[i].SFX[j]);
                                        }

                                        EditorGUI.EndDisabledGroup();

                                        dia.Lines[i].SFX[j] =
                                            (AudioClip)EditorGUILayout.ObjectField(dia.Lines[i].SFX[j], typeof(AudioClip), false,
                                                GUILayout.Width(76));

                                        if (GUILayout.Button("x", GUILayout.Width(20)))
                                        {
                                            dia.Lines[i].SFX.Remove(dia.Lines[i].SFX[j]);
                                        }
                                    }
                                }
                            }

                            if (GUILayout.Button("Add Sound"))
                            {
                                dia.Lines[i].SFX.Add(_sfx);
                            }

                            if (dia.Lines[i].Events != null)
                            {
                                for (var j = 0; j < dia.Lines[i].Events.Count; j++)
                                {
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        dia.Lines[i].Events[j] = (SceneEvent)EditorGUILayout.ObjectField(dia.Lines[i].Events[j], typeof(SceneEvent), false, GUILayout.Width(100));
                                        if (GUILayout.Button("x", GUILayout.Width(20)))
                                        {
                                            dia.Lines[i].Events.Remove(dia.Lines[i].Events[j]);
                                        }
                                    }
                                }
                            }

                            if (GUILayout.Button("Add Event"))
                            {
                                dia.Lines[i].Events.Add(CreateInstance<SceneEvent>());
                            }

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.Label("Automatic", GUILayout.Width(60));
                                dia.Lines[i].AutomaticLine = EditorGUILayout.Toggle(dia.Lines[i].AutomaticLine);
                            }
                            if (dia.Lines[i].AutomaticLine)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    dia.Lines[i].TimeToNextLine = EditorGUILayout.FloatField(dia.Lines[i].TimeToNextLine, GUILayout.Width(60));
                                }
                            }
                        }

                        if (dia.Lines[i].Speaker != null && !IsProtagonist(dia.Lines[i].Speaker))
                        {
                            using (new EditorGUILayout.VerticalScope("Box"))
                            {
                                var exprs = dia.Lines[i].Speaker.Expressions.Count;

                                if (exprs < dia.Lines[i].ExpressionNumber)
                                {
                                    dia.Lines[i].ExpressionNumber = 0;
                                }

                                var expressionNames = new string[dia.Lines[i].Speaker.Expressions.Count + 1];
                                expressionNames[0] = "<No change>";

                                for (int j = 1; j < dia.Lines[i].Speaker.Expressions.Count + 1; j++)
                                {
                                    expressionNames[j] = dia.Lines[i].Speaker.Expressions[j - 1].Name;
                                }

                                if (dia.Lines[i].Expression != null)
                                {
                                    GUIStyle expr = new GUIStyle();
                                    if (dia.Lines[i].Expression.Sprite != null && dia.Lines[i].ExpressionNumber > 0)
                                    {
                                        var tex = Utility.Editor.HandyFields.GetMaterialTexture(dia.Lines[i].Expression.Sprite);
                                        if (tex)
                                        {
                                            expr.normal.background = tex;
                                        }
                                    }

                                    EditorGUILayout.LabelField(GUIContent.none, expr, GUILayout.Width(100),
                                        GUILayout.Height(100));
                                }

                                dia.Lines[i].ExpressionNumber = EditorGUILayout.IntPopup(dia.Lines[i].ExpressionNumber,
                                    expressionNames, dia.getExpressionIntValues(dia.Lines[i].Speaker), GUILayout.Width(100));


                                if (dia.Lines[i].ExpressionNumber > 0)
                                {
                                    dia.Lines[i].Expression =
                                        dia.Lines[i].Speaker.Expressions[dia.Lines[i].ExpressionNumber - 1];
                                }
                                else
                                {
                                    dia.Lines[i].Expression = new Expression();
                                }
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(108),
                                GUILayout.Height(100));
                        }

                        GUI.backgroundColor = Color.white;
                        var propLine = propLines.GetArrayElementAtIndex(i);
                        var propLineText = propLine.FindPropertyRelative("Text");
                        propLineText.stringValue = EditorGUILayout.TextArea(propLineText.stringValue, GUILayout.Height(125), GUILayout.Width(Screen.width - 310));

                        GUI.backgroundColor = dia.Color;

                        using (new EditorGUILayout.VerticalScope())
                        {
                            if (dia.Lines.Count > 1)
                            {
                                if (GUILayout.Button("-", GUILayout.Width(20)) && dia.Lines.Count > 1)
                                {
                                    GUI.FocusControl(null);
                                    dia.Lines.Remove(dia.Lines[i]);
                                    serializedObject.Update();
                                }
                            }

                            if (i > 0)
                            {
                                if (GUILayout.Button("ʌ", GUILayout.Width(20)) && i > 0)
                                {
                                    {
                                        GUI.FocusControl(null);
                                        var line = dia.Lines[i - 1];

                                        dia.Lines[i - 1] = dia.Lines[i];
                                        dia.Lines[i] = line;
                                    }
                                }
                            }

                            if (i < dia.Lines.Count - 1)
                            {
                                if (GUILayout.Button("v", GUILayout.Width(20)))
                                {
                                    GUI.FocusControl(null);
                                    var line = dia.Lines[i + 1];

                                    dia.Lines[i + 1] = dia.Lines[i];
                                    dia.Lines[i] = line;
                                }
                            }
                            
                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button("*", GUILayout.Width(20)))
                                {
                                    Line Copy = new Line();

                                    Copy.translationKey = dia.Lines[i].translationKey;
                                    Copy.SpeakerNumber = dia.Lines[i].SpeakerNumber;
                                    Copy.Text = dia.Lines[i].Text;
                                    Copy.VoiceSFX = dia.Lines[i].VoiceSFX;
                                    Copy.SFX = dia.Lines[i].SFX;
                                    Copy.Events = dia.Lines[i].Events;
                                    Copy.TimeToNextLine = dia.Lines[i].TimeToNextLine;
                                    Copy.AutomaticLine = dia.Lines[i].AutomaticLine;
                                    Copy.Expression = dia.Lines[i].Expression;
                                    Copy.ExpressionNumber = dia.Lines[i].ExpressionNumber;
                                    Copy.AliasNumber = dia.Lines[i].AliasNumber;

                                    dia.Lines.Insert(i + 1, Copy);
                                }

                            if (GUILayout.Button("+", GUILayout.Width(20)))
                            {
                                dia.Lines.Insert(i + 1, new Line());
                                serializedObject.Update();
                            }
                        }
                    }
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
                        if (GUILayout.Button("Add Choice", GUILayout.Width(100)))
                        {
                            dia.Choices.Add(new Choice());
                            dia.Choices.Add(new Choice());
                            dia.Choices.Add(new Choice());
                        }
                    }

                    if (dia.Lines.Count > 0)
                    {
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
    }
}


