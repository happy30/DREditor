using DREditor.Dialogues;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultipleChoice
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Minigames/Multiple Choice", fileName = "MC Asset")]
    public class MultipleChoiceBuilder : MinigameBuilderBase
    {
        public Choice[] choices = new Choice[4]; // Set to 4 because in DR that's usually how it is, if there's more the Manager
                                                 // must be changed to accomodate this
        public string question = "";
        public int damageOnWrong = 1;
        public float timerMinutes;
        [Range(0,59)]
        public float timerSeconds;
        public TrialDialogue startDialogue;
        [System.Serializable]
        public class Choice
        {
            public string text = "";
            public bool isAnswer = false;
            public TrialDialogue wrongDialogue;
        }
    }
}
