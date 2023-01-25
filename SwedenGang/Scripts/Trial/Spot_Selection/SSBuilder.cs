//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Spot Selection Builder
/// </summary>
[System.Serializable]
[CreateAssetMenu(menuName = "DREditor/Minigames/Spot Selection", fileName = "SS Asset")]
public class SSBuilder : MinigameBuilderBase
{
    public Texture2D texture = null;
    public List<Spot> spots = new List<Spot>();

    public string question = "";
    public int damageOnWrong = 1;
    public float timerMinutes;
    [Range(0, 59)]
    public float timerSeconds;

    [System.Serializable]
    public class Spot
    {
        public string spotName;

        public Vector3 position;
        public Vector3 rotation;

        public Vector2 size;
        public Vector2 center;

        public bool isAnswer = false;
        public TrialDialogue wrongDialogue;
    }
#if UNITY_EDITOR
    public string testCanvas;
    public string testCG;
#endif
}
