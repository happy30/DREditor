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
    /// <summary>
    /// Functionality in ShowItemImage.cs
    /// </summary>
    [Serializable]
    public class ShowItem : IDialogueEvent
    {
        public bool _ShowHelp = false;
        public Texture2D Image = null;
        public void TriggerDialogueEvent()
        {
            DialogueEventSystem.TriggerEvent("ShowItem", Image);
        }

#if UNITY_EDITOR
        public void EditorUI(object value = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Item Image: ");
                Image = TextureField(Image);
            }
        }
        public static Texture2D TextureField(Texture2D texture, int width = 120, int height = 120)
        {
            Texture2D result;
            using (new EditorGUILayout.VerticalScope())
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    fixedWidth = 70
                };
                result = UnityField(texture, width, height);
            }
            return result;
        }
        public static T UnityField<T>(T data, int width = 120, int height = 120) where T : UnityEngine.Object
        {
            T result;
            using (new EditorGUILayout.VerticalScope())
            {
                result = (T)EditorGUILayout.ObjectField(data, typeof(T), false, GUILayout.Width(width), GUILayout.Height(height));
            }
            return result;
        }
        
        public void ShowHelpBox()
        {
            if (_ShowHelp) EditorGUILayout.HelpBox("Show an image of an item.", MessageType.Info, true);
        }

        public void ToggleHelpBox()
        {
            _ShowHelp = !_ShowHelp;
        }
#endif
    }
}
