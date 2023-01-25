//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Utility.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MultipleChoice;
using UnityEngine.UI;

[CustomEditor(typeof(SSBuilder))]
public class SSBuilderEditor : Editor
{
    SSBuilder ssb;
    public void OnEnable()
    {
        ssb = target as SSBuilder;
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CreateForm();
        EditorUtility.SetDirty(ssb);
        serializedObject.ApplyModifiedProperties();
    }
    void CreateForm()
    {
        BaseInfo();

        BuildTools();

        
        using (new GUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Room Image: ", GUILayout.Width(100));
            ssb.texture = HandyFields.TextureField(ssb.texture);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("", GUILayout.Width(120));
        }
        if(ssb.spots.Count > 0)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(60);
                if (GUILayout.Button("Delete All Spots", GUILayout.Width(120))
            && EditorUtility.DisplayDialog("Clear All", "Are you sure you want to Delete ALL spots from the list?", "Yes", "No"))
                {
                    ssb.spots.Clear();
                }
            }
        }
        GUILayout.Space(30);
        for (int i = 0; i < ssb.spots.Count; i++)
        {
            using (new GUILayout.HorizontalScope())
            {
                //ssb.spots[i].spotName = HandyFields.StringField("Name: ", ssb.spots[i].spotName, null, 125, 40);
                GUILayout.Label("Name: ", GUILayout.Width(40));
                ssb.spots[i].spotName = EditorGUILayout.TextField(ssb.spots[i].spotName, GUILayout.Width(125));

                GUILayout.Space(10);
                if (GUILayout.Button("Import Spot", GUILayout.Width(100))
                && EditorUtility.DisplayDialog("Import Spot", "Are you sure you want to Import data for this spot?", "Yes", "No"))
                {
                    Import(ssb.spots[i]);
                }
                if (ssb.spots[i].position != Vector3.zero)
                {
                    if (GUILayout.Button("Clear Spot", GUILayout.Width(100))
                && EditorUtility.DisplayDialog("Clear Spot", "Are you sure you want to Clear data for this spot?", "Yes", "No"))
                    {
                        ClearSpot(ssb.spots[i]);
                    }
                    GUILayout.Label("Has Imported Values");
                }
            }
            GUILayout.Space(5);
            using (new GUILayout.HorizontalScope())
            {
                
                EditorGUILayout.LabelField("Is Answer: ", GUILayout.Width(70));
                ssb.spots[i].isAnswer = EditorGUILayout.Toggle(ssb.spots[i].isAnswer);

            }
            if (!ssb.spots[i].isAnswer)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("On Wrong Dialogue: ", GUILayout.Width(120));
                    ssb.spots[i].wrongDialogue = HandyFields.UnityField(ssb.spots[i].wrongDialogue, 170, 25);
                }
            }
            else if (ssb.spots[i].isAnswer && ssb.spots[i].wrongDialogue != null)
            {
                ssb.spots[i].wrongDialogue = null;
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Remove Spot", GUILayout.Width(100)) 
                && EditorUtility.DisplayDialog("Remove Spot", "Are you sure you want to remove this spot?", "Yes", "No"))
            {
                ssb.spots.RemoveAt(i);
            }

            GUILayout.Space(25);
        }
        
        if (GUILayout.Button("Add Spot", GUILayout.Width(100)))
        {
            ssb.spots.Add(new SSBuilder.Spot());
        }
    }
    void BaseInfo()
    {
        ssb.question = HandyFields.StringArea("Question: ", ssb.question);
        ssb.damageOnWrong = HandyFields.IntField("Damage on Wrong: ", ssb.damageOnWrong, 25, 7);
        ssb.times = BuilderEditor.DisplayTimerSettings(ssb.times);
        //ssb.timerMinutes = HandyFields.FloatField("Timer Minutes: ", ssb.timerMinutes, 25, 7);
        //ssb.timerSeconds = HandyFields.FloatField("Timer Seconds: ", ssb.timerSeconds, 25, 7);
    }
    Canvas canvas;
    RawImage cg;
    //string testCanvas;

    void BuildTools()
    {
        
        GUILayout.Label("Testing Tools", GUILayout.Width(50));
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("Test Canvas: ", GUILayout.Width(90));
            ssb.testCanvas = EditorGUILayout.TextField(ssb.testCanvas, GUILayout.Width(125));
            
            if (GUILayout.Button("Export Spots", GUILayout.Width(100)))
            {
                for (int i = 0; i < ssb.spots.Count; i++)
                    ExportSpot(ssb.spots[i]);
            }
            if (GUILayout.Button("Clear Canvas Spots", GUILayout.Width(150))
            && EditorUtility.DisplayDialog("Clear Canvas Spots", "Are you sure you want to Clear All Canvas Spots?", "Yes", "No"))
            {
                ClearDebugCanvas();
            }
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("Test CG: ", GUILayout.Width(90));
            ssb.testCG = EditorGUILayout.TextField(ssb.testCG, GUILayout.Width(125));
            if (GUILayout.Button("Set CG", GUILayout.Width(100)))
            {
                cg = GameObject.Find(ssb.testCG).GetComponent<RawImage>();
                cg.texture = ssb.texture;
            }
        }
        
    }
    void ExportSpot(SSBuilder.Spot spot)
    {
        if (canvas == null)
            canvas = GameObject.Find(ssb.testCanvas).GetComponent<Canvas>();
        if (canvas != null)
        {
            GameObject s = new GameObject();
            s.transform.parent = canvas.transform;
            BoxCollider2D c = s.AddComponent<BoxCollider2D>();
            c.size = spot.size;
            c.offset = spot.center;
            s.transform.position = spot.position;
            s.transform.eulerAngles = spot.rotation;
            s.name = spot.spotName;
        }
        else
            Debug.LogWarning("The Canvas by that name was not found in the scene!");
    }
    void ClearDebugCanvas()
    {
        if (canvas == null)
            canvas = GameObject.Find(ssb.testCanvas).GetComponent<Canvas>();
        if (canvas != null)
        {
            BoxCollider2D[] boxes = canvas.GetComponentsInChildren<BoxCollider2D>();
            for (int i = 0; i < boxes.Length; i++)
                if (boxes[i].GetComponent<Rigidbody2D>() != null)
                    continue;
                else
                    DestroyImmediate(boxes[i].gameObject);
            RawImage cg = GameObject.Find(ssb.testCG).GetComponent<RawImage>();
            cg.texture = null;
        }
        else
            Debug.LogWarning("The Canvas by that name was not found in the scene!");
    }
    void Import(SSBuilder.Spot s)
    {
        BoxCollider2D c = Selection.gameObjects[0].GetComponent<BoxCollider2D>();
        if(c != null)
        {
            s.center = c.offset;
            s.size = c.size;
            s.position = c.transform.position;
            s.rotation = c.transform.eulerAngles;
        }
        else
        {
            Debug.LogWarning("You must lock the inspector and select an object with a BoxCollider2D Component!");
        }
    }
    void ClearSpot(SSBuilder.Spot s)
    {
        s.center = Vector2.zero;
        s.size = Vector2.zero;
        s.position = Vector3.zero;
        s.rotation = Vector3.zero;
    }
}
