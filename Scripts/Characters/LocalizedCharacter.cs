using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Characters
{
    [System.Serializable]
    public class LocalizedCharacter : ScriptableObject 
    {
        public Character original;
        public string translationKey;

        [SerializeField]
        private string _FirstName;

        public string FirstName
        {
            set { _FirstName = value; }
            get
            {
                string retVal = _FirstName;
                if(string.IsNullOrEmpty(retVal) && original != null)
                {
                    retVal = original.FirstName;
                }
                return retVal;
            }
        }

        [SerializeField]
        private string _LastName;

        public string LastName
        {
            set { _LastName = value; }
            get
            {
                string retVal = _LastName;
                if (string.IsNullOrEmpty(retVal) && original != null)
                {
                    retVal = original.LastName;
                }
                return retVal;
            }
        }
    }
}
