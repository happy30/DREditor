using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DREditor.Dialogues.Events;
using DREditor.Characters;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
    [Serializable]
    public struct TTTuple
    {
        public TrialTutorialAsset asset;
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TrialTutorial : IDialogueEvent
    {
        public TrialTutorialAsset asset;
        public bool _ShowHelp = false;
        public void TriggerDialogueEvent()
        {
            DialogueEventSystem.TriggerEvent("TrialTutorial", asset);
        }

#if UNITY_EDITOR
        public void EditorUI(object value = null)
        {
            asset = EditorFields.UnityField(asset, 120, 50);
        }

        public void ShowHelpBox()
        {
            if (_ShowHelp) EditorGUILayout.HelpBox("Starts Trial Tutorial, leave empty to turn off active tutorial", MessageType.Info, true);
        }

        public void ToggleHelpBox()
        {
            _ShowHelp = !_ShowHelp;
        }
#endif
    }
}
