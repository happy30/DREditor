//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues.Events;
using DREditor.PlayerInfo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvestigationHandler : MonoBehaviour
{
    [Header("Start Investigation")]
    [SerializeField] Canvas startCanvas = null;
    [SerializeField] Animator startAnimator = null;
    [SerializeField] string startTriggerName = "Start";

    [Header("Truth Bullet Get")]
    [SerializeField] Canvas getBulletCanvas = null;
    [SerializeField] Animator getAnimator = null;
    [SerializeField] string getBoolName = "Show";
    [SerializeField] Image getImage = null;

    public static InvestigationHandler Instance = null;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(this);
    }
    void Start()
    {
        UIHandler.ToTitle += ResetInvestigationVisuals;
        DialogueEventSystem.StartListening("TruthBulletGet", TruthBulletGet);
        DialogueEventSystem.StartListening("TruthBulletGetEnd", TruthBulletGetEnd);
    }

    private void OnDisable()
    {
        UIHandler.ToTitle -= ResetInvestigationVisuals;
        DialogueEventSystem.StopListening("TruthBulletGet", TruthBulletGet);
        DialogueEventSystem.StopListening("TruthBulletGetEnd", TruthBulletGetEnd);
    }
    void ResetInvestigationVisuals()
    {
        getBulletCanvas.enabled = false;
        getAnimator.Rebind();
    }
    public void InvestigationStart()
    {
        if (startAnimator != null && startCanvas != null)
        {
            startAnimator.SetTrigger(startTriggerName);
            startCanvas.enabled = true;
        }
        else
        {
            Debug.LogWarning("The Investigation Start Animator or Canvas was not found! Set up an animator with a Trigger named: "
                + startTriggerName);
            //return 0;
        }
        //Debug.LogWarning(startAnimator.GetCurrentAnimatorStateInfo(0).length);
        
        //return startAnimator.GetCurrentAnimatorStateInfo(0).length;
    }
    public float GetStartLength() => startAnimator.GetCurrentAnimatorStateInfo(0).length;
    public void TurnOffStartCanvas()
    {
        if (startCanvas)
            startCanvas.enabled = false;
    }

    public void TruthBulletGet(object ob)
    {
        TBGTuple data = (TBGTuple)ob;
        if (!data.TB)
        {
            Debug.LogError("Don't make a TruthBulletGet Event without referencing a Truth Bullet you moron.");
            return;
        }

        if (PlayerInfo.instance != null)
        {
            if (!PlayerInfo.instance.Info.foundBullets.Contains(data.TB.Title))
                PlayerInfo.instance.Info.AddBullet(data.TB.Title); // Add truth bullets to players found bullets
            else
                Debug.LogWarning("" + data.TB.Title + " has already been added!");
        }
        else
            Debug.LogWarning("YOU ARE MISSING A PLAYERINFO SCRIPT ON YOUR GAMEMANAGER, ADD IT, OTHERWISE YOUR TRUTH BULLET MENU" +
                " WONT WORK!");

        getImage.sprite = data.TB.Picture;
        if (getAnimator != null && getBulletCanvas != null)
        {
            getAnimator.SetBool(getBoolName, true);
            getBulletCanvas.enabled = true;
        }
        else
            Debug.LogWarning("The Bullet Get Animator or Canvas was not found! Set up an animator with a Trigger named: "
                + getBoolName);
    }
    public void TruthBulletGetEnd(object ob)
    {
        if (getAnimator != null && getBulletCanvas != null)
        {
            StartCoroutine(GetEnd());
        }
    }
    IEnumerator GetEnd()
    {
        getAnimator.SetBool(getBoolName, false);
        if (getAnimator.GetCurrentAnimatorStateInfo(0).length == float.PositiveInfinity)
        {
            yield return new WaitUntil(() => getAnimator.GetCurrentAnimatorStateInfo(0).length != float.PositiveInfinity);
        }
        yield return new WaitForSeconds(getAnimator.GetCurrentAnimatorStateInfo(0).length);
        getBulletCanvas.enabled = false;
        yield break;
    }
}
