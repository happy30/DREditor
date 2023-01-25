//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DREditor.Utility.Editor;
[CustomEditor(typeof(TrialBuilder))]
public class TrialBuilderEditor : Editor
{
    private TrialBuilder tb;
    MinigameTypeDB MTDB;
    private void Awake()
    {
        MTDB = (MinigameTypeDB)Resources.Load("Trial/MinigameTypeDB");
        if (MTDB == null)
        {
            Debug.LogError("MinigameTypeDatabase not found : Are you missing it in Resources/Trial/MinigameTypeDB ?");
        }
    }
    private void OnEnable() => tb = target as TrialBuilder;
    public override void OnInspectorGUI()
    {
        HandyFields.Label("Trial Builder");
        TrialForm();
        EditorUtility.SetDirty(tb);
    }
    public void TrialForm()
    {
        if (MTDB == null)
        {
            Debug.LogError("MinigameTypeDatabase not found : Are you missing it in Resources/Trial/MinigameTypeDB ?");
            return;
        }

        if(tb.TrialSequences.Count == 0)
        {
            if (GUILayout.Button("Add a new part of the trial!"))
            {
                tb.TrialSequences.Add(new TrialBuilder.TrialSequence());
            }
        }

        if (tb.TrialSequences.Count != 0)
        {
            for (int i = 0; i < tb.TrialSequences.Count; i++)
            {
                var sequence = tb.TrialSequences[i];
                using (new EditorGUILayout.HorizontalScope("Box"))
                {
                    ShowSequenceInfo(sequence, i);
                    GUILayout.FlexibleSpace();
                }
            }
        }

        if (tb.TrialSequences.Count != 0)
        {
            if (GUILayout.Button("Add a new part of the trial!"))
            {
                tb.TrialSequences.Add(new TrialBuilder.TrialSequence());
            }
        }
        
        tb.EndSceneName = HandyFields.StringField("End Scene Name: ", tb.EndSceneName, null, 130);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("End Gate: ");
            tb.EndGate = HandyFields.UnityField(tb.EndGate, 120, 30);
        }
        
    }

    public void ShowSequenceInfo(TrialBuilder.TrialSequence sequence, int i)
    {
        using (new EditorGUILayout.VerticalScope("Box"))
        {
            GUILayout.Label(i.ToString(), GUILayout.Width(100));
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Minigame Type: ", GUILayout.Width(100));
                sequence.SequenceTypeNumber = EditorGUILayout.IntPopup(sequence.SequenceTypeNumber,
                MTDB.ToStringArray(), MTDB.ToIntArray(), GUILayout.Width(180));
                sequence.SequenceType = MTDB.MinigameList[sequence.SequenceTypeNumber].TypeName;
                sequence.MinigameType = MTDB.MinigameList[sequence.SequenceTypeNumber];
                sequence.SequenceDescription = HandyFields.StringArea("Description: ", sequence.SequenceDescription);
                using (new EditorGUILayout.VerticalScope())
                {
                    EditSequencePosition(i);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Minigame Asset: ", GUILayout.Width(100));
                sequence.MinigameAsset = HandyFields.UnityField<ScriptableObject>(sequence.MinigameAsset, 180, 20);
                
            }

            /*
            if (GUILayout.Button("Remove this part of the trial"))
            {
                tb.TrialSequences.Remove(sequence);
            }
            */
        }
    }
    private void EditSequencePosition(int i)
    {

        #region Column 4 Side Buttons
        //Column 4
        using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(25)))
        {
            if (tb.TrialSequences.Count > 1)
            {
                if (GUILayout.Button(new GUIContent("-", "Delete this Line."), GUILayout.Width(20)) && tb.TrialSequences.Count > 1)
                {
                    GUI.FocusControl(null);
                    tb.TrialSequences.Remove(tb.TrialSequences[i]);
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
                if (GUILayout.Button(new GUIContent("^", "Move this line up."), GUILayout.Width(20)) && i > 0)
                {
                    {
                        GUI.FocusControl(null);
                        var line = tb.TrialSequences[i - 1];

                        tb.TrialSequences[i - 1] = tb.TrialSequences[i];
                        tb.TrialSequences[i] = line;
                    }
                }
            }
            else
            {
                GUILayout.Space(20f);
            }

            if (i < tb.TrialSequences.Count - 1)
            {
                if (GUILayout.Button(new GUIContent("v", "Move this line down."), GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    var line = tb.TrialSequences[i + 1];

                    tb.TrialSequences[i + 1] = tb.TrialSequences[i];
                    tb.TrialSequences[i] = line;
                }
            }
            else
            {
                GUILayout.Space(20f);
            }

            GUILayout.Space(25f);
            GUILayout.FlexibleSpace();

            

            if (GUILayout.Button(new GUIContent("+", "Add a new Line below."), GUILayout.Width(20)))
            {
                tb.TrialSequences.Insert(i + 1, new TrialBuilder.TrialSequence());
                serializedObject.Update();
            }

            GUILayout.FlexibleSpace();
        }
        #endregion

    }
}
