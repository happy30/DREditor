//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Utility.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NSD.Editor
{
    [CustomEditor(typeof(CBCAnim))]
    public class CBCAnimEditor : UnityEditor.Editor
    {
        CBCAnim cbc;
        public void OnEnable()
        {
            cbc = target as CBCAnim;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CreateForm();
            EditorUtility.SetDirty(cbc);
            serializedObject.ApplyModifiedProperties();
        }

        void CreateForm()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("IsVisible: ");
                cbc.isVisible = EditorGUILayout.Toggle(cbc.isVisible);
            }

            cbc.startPosition = HandyFields.Vector3Field("Start Position: ", cbc.startPosition);

            if(cbc.Anims.Count != 0)
            {
                for(int i = 0; i < cbc.Anims.Count; i++)
                {
                    DisplayAnim(cbc.Anims[i]);
                }
            }

            if(GUILayout.Button("Add Anim"))
            {
                cbc.Anims.Add(new CBCAnim.AnimModule());
            }
            
        }
        
        void DisplayAnim(CBCAnim.AnimModule module)
        {
            CBCAnim.CBCType old = module.type;
            module.type = (CBCAnim.CBCType)EditorGUILayout.EnumPopup(module.type);

            if(module.data == null || module.type != old)
            {
                ConfigType(module);
            }
            else
            {
                DisplayBaseData(module.data);
                switch (module.type)
                {
                    case CBCAnim.CBCType.Transform:
                        DisplayTransform((CBCAnim.TransformData)module.data);
                        break;

                    case CBCAnim.CBCType.Rotation:
                        DisplayRotate((CBCAnim.RotateData)module.data);
                        break;

                    case CBCAnim.CBCType.Scale:
                        DisplayScale((CBCAnim.ScaleData)module.data);
                        break;

                    case CBCAnim.CBCType.Fade:
                        DisplayFade((CBCAnim.FadeData)module.data);
                        break;
                }
            }

            if (GUILayout.Button("Remove Anim"))
            {
                cbc.Anims.Remove(module);
            }
        }

        void ConfigType(CBCAnim.AnimModule module)
        {
            switch (module.type)
            {
                case CBCAnim.CBCType.Transform:
                    module.data = new CBCAnim.TransformData();
                    break;

                case CBCAnim.CBCType.Rotation:
                    module.data = new CBCAnim.RotateData();
                    break;

                case CBCAnim.CBCType.Scale:
                    module.data = new CBCAnim.ScaleData();
                    break;

                case CBCAnim.CBCType.Fade:
                    module.data = new CBCAnim.FadeData();
                    break;
            }
            
        }
        void DisplayBaseData(CBCAnim.AnimModuleData data)
        {
            data.delay = HandyFields.FloatField("Delay: ", data.delay);
            data.duration = HandyFields.FloatField("Duration: ", data.duration);
            data.ease = (DG.Tweening.Ease)EditorGUILayout.EnumPopup(data.ease);
        }
        void DisplayTransform(CBCAnim.TransformData data)
        {
            data.endValue = HandyFields.Vector3Field("End Value: ", data.endValue);
        }
        void DisplayScale(CBCAnim.ScaleData data)
        {
            data.endValue = HandyFields.FloatField("End Value: ", data.endValue);
        }
        void DisplayRotate(CBCAnim.RotateData data)
        {
            data.endValue = HandyFields.Vector3Field("End Value: ", data.endValue);
            data.mode = (DG.Tweening.RotateMode)EditorGUILayout.EnumPopup(data.mode);
        }
        void DisplayFade(CBCAnim.FadeData data)
        {
            data.endValue = HandyFields.FloatField("End Value: ", data.endValue);
        }
    }
}
