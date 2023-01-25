//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TruthBulletSelect;
using UnityEditor;
using DREditor.Utility.Editor;
using DREditor.Dialogues;

[CustomEditor(typeof(TBSelectBuilder))]
public class TBSelectBuilderEditor : Editor
{
    TBSelectBuilder tbsb;
    EvidenceDatabase evidenceDatabase;
    public void OnEnable()
    {
        tbsb = target as TBSelectBuilder;
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (!ValidateEvidenceDB())
        {
            return;
        }
        
        
        
        CreateForm();
        EditorUtility.SetDirty(tbsb);
        serializedObject.ApplyModifiedProperties();
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
    TrialDialogue allDialogue;
    void CreateForm()
    {
        tbsb.damageOnWrong = HandyFields.IntField("Damage on Wrong: ", tbsb.damageOnWrong, 25, 7);
        tbsb.times = BuilderEditor.DisplayTimerSettings(tbsb.times);
        //tbsb.timerMinutes = HandyFields.FloatField("Timer Minutes: ", tbsb.timerMinutes, 25, 7);
        //tbsb.timerSeconds = HandyFields.FloatField("Timer Seconds: ", tbsb.timerSeconds, 25, 7);
        tbsb.question = HandyFields.StringArea("Question: ", tbsb.question);
        /*
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("End Dialogue: ", GUILayout.Width(120));
            tbsb.endDialogue = HandyFields.UnityField(tbsb.endDialogue, 170, 25);
        }
        */
        tbsb.chapter = HandyFields.IntField("Chapter: ", tbsb.chapter, 25, 7);

        if (tbsb.chapter < evidenceDatabase.Evidences.Count - 1 || tbsb.chapter > evidenceDatabase.Evidences.Count - 1)
        {
            GUILayout.Label("Chapter has no Evidence", GUILayout.Width(220));
            return;
        }
        if (tbsb.selections.Count == 0)
        {
            for (int i = 0; i < evidenceDatabase.Evidences[tbsb.chapter].TruthBullets.Count; i++)
            {
                tbsb.selections.Add(new TBSelectBuilder.Selection());
            }
        }
        if (evidenceDatabase.Evidences[tbsb.chapter].TruthBullets.Count != tbsb.selections.Count)
        {
            int diff = tbsb.selections.Count - evidenceDatabase.Evidences[tbsb.chapter].TruthBullets.Count;

            if (diff > 0) // Removed TB from Evidence
            {
                tbsb.selections.RemoveAt(tbsb.selections.Count - diff);
            }
            else // Added TB to Evidence
            {
                diff *= -1;
                for (int i = 0; i < diff; i++)
                {
                    tbsb.selections.Add(new TBSelectBuilder.Selection());
                }
            }
            //Debug.Log("Cleared Stuff " + tbsb.selections.Count + " " + evidenceDatabase.Evidences[tbsb.chapter].TruthBullets.Count);
            //tbsb.selections.Clear();
            
        }
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Copy All Wrong Dialogue", GUILayout.Width(170));
            allDialogue = HandyFields.UnityField(allDialogue, 170, 25);
            if (GUILayout.Button("Paste To All"))
            {
                for (int i = 0; i < tbsb.selections.Count; i++)
                {
                    tbsb.selections[i].wrongDialogue = allDialogue;
                }
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("", GUILayout.Width(120));
        }

        Evidence ev = evidenceDatabase.Evidences[tbsb.chapter];
        for (int i = 0; i < tbsb.selections.Count; i++)
        {
            tbsb.selections[i].bulletOption = ev.TruthBullets[i];
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(tbsb.selections[i].bulletOption.Title, GUILayout.Width(120));
                tbsb.selections[i].isAnswer = EditorGUILayout.Toggle(tbsb.selections[i].isAnswer);
            }
            if (!tbsb.selections[i].isAnswer)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("On Wrong Dialogue: ", GUILayout.Width(120));
                    tbsb.selections[i].wrongDialogue = HandyFields.UnityField(tbsb.selections[i].wrongDialogue, 170, 25);
                }
            }
            else
            {
                tbsb.selections[i].wrongDialogue = null;
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("", GUILayout.Width(120));
            }
        }

    }
}
