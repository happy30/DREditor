//
// EventObjects - A scriptable-object based messaging system for Unity
//
// Copyright (c) 2019 Bart Heijltjes (Wispfire)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System.Text;
using UnityEditor;
using UnityEngine;

namespace EventObjects.Editor
{
    [CustomEditor(typeof(BaseValueWithEvent), true)]
    public class ValueWithEventInspector : UnityEditor.Editor
    {
        private BaseValueWithEvent _eventObject;
        private bool showDefaultInspector;

        private SerializedProperty _initialValue;
        private SerializedProperty _value;
        private void OnEnable()
        {
            _eventObject = target as BaseValueWithEvent;
            _initialValue = serializedObject.FindProperty("InitialValue");
            _value = serializedObject.FindProperty("_value");
        }
        

        public override void OnInspectorGUI()
        {
            if (_initialValue == null) 
            {
                EditorGUILayout.HelpBox("Value of this EventObject is not serializable and can only be set from script", MessageType.Info);
                base.OnInspectorGUI(); // Value is not serializable
                return;
            }
            serializedObject.Update();
            EditorGUILayout.PropertyField(_initialValue, true);
            if (Application.isPlaying)
            {
                var isInit = _eventObject.IsInitialized;
                var initToggle = EditorGUILayout.ToggleLeft("Is Initialized", isInit);
                if (isInit != initToggle)
                {
                    if (initToggle) _eventObject.Init();
                    else _eventObject.Reset();
                }
                EditorGUILayout.BeginHorizontal();
                
                EditorGUI.BeginDisabledGroup(!isInit);
                EditorGUILayout.PropertyField(_value, true);
                EditorGUI.EndDisabledGroup();
                
                if (GUILayout.Button("Invoke",  EditorStyles.miniButton, GUILayout.Width(60f)))
                {
                    _eventObject.Invoke();
                }
                EditorGUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();

            
            EditorGUILayout.Space();
            showDefaultInspector = EditorGUILayout.Foldout(showDefaultInspector, "Default Inspector");
            if (showDefaultInspector)
            {
                EditorGUI.indentLevel++;
                base.OnInspectorGUI();
                EditorGUI.indentLevel--;
            }

        }
    }
}
