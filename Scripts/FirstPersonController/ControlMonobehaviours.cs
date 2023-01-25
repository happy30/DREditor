using System.Collections;
using System.Collections.Generic;
using DREditor.EventObjects;
using UnityEngine;

namespace DREditor.FPC
{

    public class ControlMonobehaviours : MonoBehaviour
    {
        public BoolWithEvent InMenu;
        public BoolWithEvent InDialogue;
        public BoolWithEvent InTPFD;
        public BoolWithEvent InLoading;

        public bool setting;
        private bool tpfdSetting;

        public bool disable;

        private bool valueChanged;

        public List<MonoBehaviour> MonoBehaviours = new List<MonoBehaviour>();
        public MonoBehaviour RaycastScript;
        public MonoBehaviour TPFDScript;
        

        public static ControlMonobehaviours instance = null; //*
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
            // For singleton ^
            DontDestroyOnLoad(this); //*

        }

        private void Start()
        {
            //InMenu.Value = false;
            //UnityEngine.Debug.Log("Called Start Of Control MonoBehaviours");
            //InDialogue.Value = false; In a build if this mono is enabled then start is called so be careful
        }

        void Update()
        {
            disable = InMenu.Value || InDialogue.Value;

            if (InLoading.Value != inLoadCheck && InLoading.Value)
            {
                //SaveBool();
                inLoadCheck = InLoading.Value;
            }

            if (InLoading.Value != inLoadCheck && !InLoading.Value)
            {
                //LoadBool();
                inLoadCheck = InLoading.Value;
            }
            if (!InLoading.Value && (disable != setting || tpfdSetting != InTPFD.Value || (TPFDScript.enabled != InTPFD.Value && !InDialogue.Value 
                && !InMenu.Value))) //  && disable != Test(disable)
            {
                if (!InTPFD.Value)
                {
                    foreach (var beh in MonoBehaviours)
                    {
                        beh.enabled = !disable;
                    }
                    if (TPFDScript.enabled)
                        TPFDScript.enabled = false;
                    UnityEngine.Debug.LogWarning("Called setmono");
                    UnityEngine.Debug.Log("TPFFD Value is: " + InTPFD.Value);
                    setting = disable;
                }
                else
                {
                    foreach (var beh in MonoBehaviours)
                    {
                        beh.enabled = false;
                    }

                    UnityEngine.Debug.LogWarning("TPFD Edited");
                    TPFDScript.enabled = !disable;
                    RaycastScript.enabled = !disable;
                    MonoBehaviours[0].enabled = false;
                }
                setting = disable;
                tpfdSetting = InTPFD.Value;

            }

        }
        bool Test(bool disable)
        {
            foreach (var beh in MonoBehaviours)
            {
                if(beh.enabled != disable)
                    return false;
            }
            return true;
        }
        List<bool> saved = new List<bool>();
        bool inLoadCheck = false;
        public void SaveBool() // Save current monobehaviors enabled setting when in loading. Then when not in loading put it back
        {
            UnityEngine.Debug.LogWarning("SAVE BOOL CALLED");
            foreach (var beh in MonoBehaviours)
            {
                saved.Add(beh.enabled);
                beh.enabled = false;
            }
            RaycastScript.enabled = false;
            //MonoBehaviours[0].enabled = true; // This and below is so player's state/position is correct
            MonoBehaviours[2].enabled = true;
        }
        public void LoadBool()
        {
            UnityEngine.Debug.LogWarning("LOAD BOOL CALLED");
            for (int i = 0; i < MonoBehaviours.Count; i++)
            {
                //if (i == 0 || i == 4 )
                    //continue;
                MonoBehaviours[i].enabled = saved[i];
            }
            RaycastScript.enabled = true;
            MonoBehaviours[0].enabled = !InTPFD.Value;
            //MonoBehaviours[2].enabled = !InTPFD.Value;
            MonoBehaviours[4].enabled = !InTPFD.Value;
            
            TPFDScript.enabled = InTPFD.Value;
            saved.Clear();
        }
        public void Enable(bool to)
        {
            for (int i = 0; i < MonoBehaviours.Count; i++)
                MonoBehaviours[i].enabled = to;
        }
        public void EnableTPFD(bool to) => TPFDScript.enabled = to;
    }
}
    
    

