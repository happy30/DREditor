#if UNITY_EDITOR
// Verify Dialogue Expressions By Sweden#6386 
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using DREditor.Dialogues;
using DREditor.Characters;
using DREditor.Utility.Editor;
using System.Linq;
using DREditor.Dialogues.Events;

namespace DREditor.Toolbox
{

    using Debug = UnityEngine.Debug;
    /// <summary>
    /// If someone changed a characters list of expressions, this tool corrects all dialogue 
    /// assets affected by the change.
    /// This could prob be moved to a button on the Character asset itself
    /// </summary>
    public class VerifyDialogues : ScriptableWizard
    {

        [MenuItem("Tools/DREditor/Verify Dialogues")]
        public static void CreateWizard()
        {
            DisplayWizard<VerifyDialogues>("Verify Dialogues", "Verify");
        }
        private void OnWizardCreate()
        {
            BeginVerification();
        }
        string text;
        private void OnGUI()
        {
            if (GUILayout.Button("Get Dialogues that have \n DirectTo + SceneTransition no end and no tomenu"))
                GetInfo();
            if (GUILayout.Button("Get Dialogues that have \n what I'm looking for."))
                FindIt();
            
            if (GUILayout.Button("Verify"))
                OnWizardCreate();

            GUILayout.Space(20);
            if (GUILayout.Button("Get Trial Dialogues that contain \n the following text."))
                FindTrialDialogueWithText();
            if (GUILayout.Button("Get Dialogues that contain \n the following text."))
                FindDialogueWithText();
            text = EditorGUILayout.TextArea(text);
        }
        #region Verify Dialogues
        void BeginVerification()
        {
            var assets = FindAssetsByType<Dialogue>();
            for(int j = 0; j < assets.Count; j++)
            {
                Dialogue dia = assets[j];
                bool changed = false;
                if (dia.Lines != null)
                {
                    for (int i = 0; i < dia.Lines.Count; i++)
                    {
                        var currentLine = dia.Lines[i];
                        if (currentLine.Speaker != null && currentLine.ExpressionNumber != 0)
                        {
                            // If sprite moved position on characters expression list
                            if (currentLine.Speaker.Expressions[currentLine.ExpressionNumber - 1].Name != currentLine.Expression.Name)
                            {
                                if (currentLine.Speaker.Expressions.Contains(currentLine.Expression))
                                {
                                    currentLine.ExpressionNumber = currentLine.Speaker.Expressions.IndexOf(currentLine.Expression) + 1;
                                    Debug.Log("By Expression Changed " + dia.name + "'s Line " + i + " " +
                                            currentLine.Speaker.FirstName + "'s  expression number for " + currentLine.Expression.Name);
                                    changed = true;
                                    continue;
                                }
                                foreach (Expression e in currentLine.Speaker.Expressions)
                                {
                                    if (e.Name == currentLine.Expression.Name)
                                    {
                                        Debug.Log("Changing: " + dia.name);
                                        currentLine.ExpressionNumber = currentLine.Speaker.Expressions.IndexOf(e) + 1;
                                        Debug.Log("By Name Changed " + dia.name + "'s Line " + i + " " +
                                            currentLine.Speaker.FirstName + "'s expression number for " + e.Name);
                                        changed = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (changed)
                    EditorUtility.SetDirty(dia);
            }
        }
        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }
        #endregion

        void GetInfo()
        {

            var assets = FindAssetsByType<Dialogue>();
            for (int j = 0; j < assets.Count; j++)
            {
                Dialogue dia = assets[j];
                if (dia.DirectTo != null && dia.SceneTransition.Enabled && !dia.SceneTransition.AtEnd && !dia.SceneTransition.ToMenu)
                {
                    Debug.Log("Asset Found Called: " + dia.name);
                }
            }
            Close();
        }
        void FindIt()
        {
            var assets = FindAssetsByType<Dialogue>();
            for (int j = 0; j < assets.Count; j++)
            {
                Dialogue dia = assets[j];
                if(dia.Lines[0].Speaker != null && dia.Lines[0].Speaker.FirstName.Contains("Damon"))
                {
                    Debug.Log("Asset Found Called: " + dia.name + " on line: " + 0);
                }
                for(int x = 0; x < dia.Lines.Count; x++)
                {
                    Line currentLine = dia.Lines[x];
                    /* Dialogues with Last Actor Event
                    var vid = currentLine.DiaEvents.Where(n => n.GetType() == typeof(LastActorSprite));
                    if (vid.Count() > 0)
                    {
                        LastActorSprite v = (LastActorSprite)vid.ElementAt(0);
                        if (v != null)
                        {
                            Debug.Log("Asset Found Called: " + dia.name + " on line: " + x);
                        }
                    }
                    */
                    /*
                    var vid = currentLine.DiaEvents.Where(n => n.GetType() == typeof(VideoDisplay));
                    if (vid.Count() > 0)
                    {
                        VideoDisplay v = (VideoDisplay)vid.ElementAt(0);
                        if (v.SVValue.waitToEnd)
                        {
                            Debug.Log("Asset Found Called: " + dia.name + " on line: " + x);
                        }
                    }
                    */
                    
                }
            }
            Close();
        }
        void FindDialogueWithText()
        {
            var assets = FindAssetsByType<Dialogue>();
            bool foundOne = false;
            for (int j = 0; j < assets.Count; j++)
            {
                Dialogue dia = assets[j];
                for (int x = 0; x < dia.Lines.Count; x++)
                {
                    Line currentLine = dia.Lines[x];
                    if(currentLine.Text.Contains(text))
                    {
                        foundOne = true;
                        Debug.Log("Asset Found Called: " + dia.name + " on line: " + x);
                    }
                }
            }
            if (!foundOne)
            {
                Debug.Log("Couldn't find a trial dialogue that fit.");
            }
            Close();
        }
        void FindTrialDialogueWithText()
        {
            var assets = FindAssetsByType<TrialDialogue>();
            bool foundOne = false;
            for (int j = 0; j < assets.Count; j++)
            {
                TrialDialogue dia = assets[j];
                for (int x = 0; x < dia.Lines.Count; x++)
                {
                    TrialLine currentLine = dia.Lines[x];
                    if (currentLine.Text.Contains(text))
                    {
                        foundOne = true;
                        Debug.Log("Asset Found Called: " + dia.name + " on line: " + x);
                    }
                }
            }
            if (!foundOne)
            {
                Debug.Log("Couldn't find a trial dialogue that fit.");
            }
            Close();
        }
    }
}
#endif