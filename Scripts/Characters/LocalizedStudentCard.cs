using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Characters
{
    [System.Serializable]
    public class LocalizedStudentCard 
    {
        public Student originalStudent;

        [SerializeField]
        private string _Talent;
        public string Talent
        {
            set { _Talent = value; }
            get
            {
                string retVal = _Talent;
                if(string.IsNullOrEmpty(retVal) && originalStudent != null
                    && originalStudent.StudentCard != null)
                {
                    retVal = originalStudent.StudentCard.Talent;
                }
                return retVal;
            }
        }

        [SerializeField]
        private string _Height;
        public string Height
        {
            set { _Height = value; }
            get
            {
                string retVal = _Height;
                if (string.IsNullOrEmpty(retVal) && originalStudent != null
                    && originalStudent.StudentCard != null)
                {
                    retVal = originalStudent.StudentCard.Height;
                }
                return retVal;
            }
        }

        [SerializeField]
        private string _Weight;
        public string Weight
        {
            set { _Weight = value; }
            get
            {
                string retVal = _Weight;
                if (string.IsNullOrEmpty(retVal) && originalStudent != null
                    && originalStudent.StudentCard != null)
                {
                    retVal = originalStudent.StudentCard.Weight;
                }
                return retVal;
            }
        }

        [SerializeField]
        private string _Chest;
        public string Chest
        {
            set { _Chest = value; }
            get
            {
                string retVal = _Chest;
                if (string.IsNullOrEmpty(retVal) && originalStudent != null
                    && originalStudent.StudentCard != null)
                {
                    retVal = originalStudent.StudentCard.Chest;
                }
                return retVal;
            }
        }

        [SerializeField]
        private string _BloodType;
        public string BloodType
        {
            set { _BloodType = value; }
            get
            {
                string retVal = _BloodType;
                if (string.IsNullOrEmpty(retVal) && originalStudent != null
                    && originalStudent.StudentCard != null)
                {
                    retVal = originalStudent.StudentCard.BloodType;
                }
                return retVal;
            }
        }

        [SerializeField]
        private string _DateOfBirth;
        public string DateOfBirth
        {
            set { _DateOfBirth = value; }
            get
            {
                string retVal = _DateOfBirth;
                if (string.IsNullOrEmpty(retVal) && originalStudent != null
                    && originalStudent.StudentCard != null)
                {
                    retVal = originalStudent.StudentCard.DateOfBirth;
                }
                return retVal;
            }
        }

        [SerializeField]
        private string _Likes;
        public string Likes
        {
            set { _Likes = value; }
            get
            {
                string retVal = _Likes;
                if (string.IsNullOrEmpty(retVal) && originalStudent != null
                    && originalStudent.StudentCard != null)
                {
                    retVal = originalStudent.StudentCard.Likes;
                }
                return retVal;
            }
        }

        [SerializeField]
        private string _Dislikes;
        public string Dislikes
        {
            set { _Dislikes = value; }
            get
            {
                string retVal = _Dislikes;
                if (string.IsNullOrEmpty(retVal) && originalStudent != null
                    && originalStudent.StudentCard != null)
                {
                    retVal = originalStudent.StudentCard.Dislikes;
                }
                return retVal;
            }
        }

        [SerializeField]
        private string _Notes;
        public string Notes
        {
            set { _Notes = value; }
            get
            {
                string retVal = _Notes;
                if (string.IsNullOrEmpty(retVal) && originalStudent != null
                    && originalStudent.StudentCard != null)
                {
                    retVal = originalStudent.StudentCard.Notes;
                }
                return retVal;
            }
        }
    }
}
