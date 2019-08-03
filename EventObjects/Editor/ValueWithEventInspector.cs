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
