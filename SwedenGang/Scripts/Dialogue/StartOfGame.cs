//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
/// <summary>
/// This is a Monobehaviour to start the beginning of Project Eden's Garden
/// </summary>
public class StartOfGame : MonoBehaviour
{
    [SerializeField] Dialogue[] dialogues = new Dialogue[1];
    //[SerializeField] Image startFade = null;
    [SerializeField] Actor actor = null;
    //[SerializeField] Canvas MainUI = null;
    private void Awake()
    {
        ObserveManager.CanChange = false;
        GameManager.instance.ChangeMode(GameManager.Mode.TPFD);
    }
    void Start()
    {
        //GameManager.instance.ChangeMode(GameManager.Mode.TPFD);
        //BeginGame();
        RoomLoader.EndLoad += BeginGame;
    }
    void BeginGame() => StartCoroutine(StartIntro());
    IEnumerator StartIntro()
    {
        
        Debug.LogWarning("Start of Game Called");
        if (dialogues.Length == 0 || !dialogues[0])
        {
            Debug.LogWarning("Start Of Game Didn't activate because it didn't have dialogue.");
            yield break;
        }
        
        yield return new WaitForSeconds(1);
        //GameManager.instance.ChangeMode(GameManager.Mode.TPFD);

        DialogueAnimConfig.instance.inDialogue.Value = true;
        ObserveManager.CanChange = true;
        DialogueAnimConfig.instance.StartDialogue(actor, dialogues);
        
        yield break;
    }
    private void OnDestroy()
    {
        RoomLoader.EndLoad -= BeginGame;
    }
}
