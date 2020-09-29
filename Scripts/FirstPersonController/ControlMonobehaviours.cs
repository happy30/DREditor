using System.Collections;
using System.Collections.Generic;
using EventObjects;
using UnityEngine;

namespace DREditor.FPC
{

    public class ControlMonobehaviours : MonoBehaviour
    {
        public BoolWithEvent InMenu;
        public BoolWithEvent InDialogue;

        private bool setting;

        private bool disable;


        private bool valueChanged;

        public List<MonoBehaviour> MonoBehaviours = new List<MonoBehaviour>();

        private void Awake()
        {
            InMenu.Value = false;
            InDialogue.Value = false;
        }

        void Update()
        {
            disable = InMenu.Value || InDialogue.Value;



            if (disable != setting)
            {
                foreach (var beh in MonoBehaviours)
                {
                    beh.enabled = !disable;
                    setting = disable;
                }
            }

        }
    }
}
    
    

