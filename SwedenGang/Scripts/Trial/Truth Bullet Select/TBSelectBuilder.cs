//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DREditor.TrialEditor;

namespace TruthBulletSelect
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Minigames/Truth Bullet Select", fileName = "TBS Asset")]
    public class TBSelectBuilder : MinigameBuilderBase
    {
        [Range(0,6)]
        public int chapter = 0;
        public int damageOnWrong = 1;
        public float timerMinutes;
        [Range(0,59)]
        public float timerSeconds;
        public string question;
        //public TrialDialogue endDialogue;
        public List<Selection> selections = new List<Selection>();

        [System.Serializable]
        public class Selection
        {
            public TruthBullet bulletOption;
            public bool isAnswer = false;
            public TrialDialogue wrongDialogue;
        }
    }
}
