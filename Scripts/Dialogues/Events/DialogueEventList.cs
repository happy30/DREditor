//Dialogue Event List script by SeleniumSoul for DREditor
//Used to create automatically create a list of all Dialogue Event Scripts

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace DREditor.Dialogues.Events
{
    [InitializeOnLoad]
    
    public class DialogueEventList
    {
        public static List<Type> ListOfClasses;

        static DialogueEventList()
        {
            if (!SessionState.GetBool("FirstInitDone", false))
            {
                UpdateList();
                SessionState.SetBool("FirstInitDone", true);
            }
        }

        public static List<Type> GetInheritedClasses()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(IDialogueEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();
        }

        public static void UpdateList()
        {
            ListOfClasses = GetInheritedClasses();
        }
    }
}
#endif