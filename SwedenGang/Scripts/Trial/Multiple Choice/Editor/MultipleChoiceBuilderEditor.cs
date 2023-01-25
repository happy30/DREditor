using DREditor.Utility.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MultipleChoice;

[CustomEditor(typeof(MultipleChoiceBuilder))]
public class MultipleChoiceBuilderEditor : Editor
{
    MultipleChoiceBuilder mcb;
    public void OnEnable()
    {
        mcb = target as MultipleChoiceBuilder;
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CreateForm();
        EditorUtility.SetDirty(mcb);
        serializedObject.ApplyModifiedProperties();
    }
    void CreateForm()
    {
        mcb.question = HandyFields.StringArea("Question: ", mcb.question);
        mcb.damageOnWrong = HandyFields.IntField("Damage on Wrong: ", mcb.damageOnWrong, 25, 7);
        mcb.times = BuilderEditor.DisplayTimerSettings(mcb.times);
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Start Dialogue: ", GUILayout.Width(120));
            mcb.startDialogue = HandyFields.UnityField(mcb.startDialogue, 170, 25);
        }
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("", GUILayout.Width(120));
        }
        for (int i = 0; i < mcb.choices.Length; i++)
        {
            using (new GUILayout.HorizontalScope())
            {
                mcb.choices[i].text = HandyFields.StringField("Choice Text: ", mcb.choices[i].text);
                EditorGUILayout.LabelField("Is Answer: ", GUILayout.Width(70));
                mcb.choices[i].isAnswer = EditorGUILayout.Toggle(mcb.choices[i].isAnswer);
                
            }
            if (!mcb.choices[i].isAnswer)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("On Wrong Dialogue: ", GUILayout.Width(120));
                    mcb.choices[i].wrongDialogue = HandyFields.UnityField(mcb.choices[i].wrongDialogue, 170, 25);
                }
            }
            else if(mcb.choices[i].isAnswer && mcb.choices[i].wrongDialogue != null)
            {
                mcb.choices[i].wrongDialogue = null;
            }
        }
    }
}
