//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues;
using DREditor.EventObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class TrialManager : MonoBehaviour
{
    //[SerializeField] MinigameTypeDB mgdb = null;
    [SerializeField] List<TrialBuilder> trials = new List<TrialBuilder>();
    private static bool TrialSequenceFinished = false;
    [SerializeField] bool playOnStart = true;

    [Header("Debugging")]
    public bool DebugMode = true;
    public int DebugIndex = 0;
    [SerializeField] bool onlyFirstSequence = false;
    [Tooltip("When enabled uses the trial asset's End scene string instead of it's gate")]
    public bool useStringOnEnd = false;
    public bool startLastIndex = false;
    [Header("Optional")]
    [SerializeField] bool skipAnimatorFadeOut = false;
    [Header("All Video fields must be filled out to work")]
    [SerializeField] VideoPlayer video = null;
    [SerializeField] RawImage videoProjector = null;
    [SerializeField] Canvas videoCan = null;
    [SerializeField] AudioClip endSound = null;
    public static bool InMinigame = false;
    private static int skip = 0;
    static TrialBuilder.TrialSequence currentSequence;
    public static TrialManager instance = null;

    private void OnDisable()
    {
        EnableDiaCanvases(true);
    }
    private void Awake()
    {
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.DisablePlayer();
        }
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    private void Start()
    {
        if (playOnStart)
        {
            if (DebugMode)
            {
                StartCoroutine(Debugging());
                return;
            }
            if (RoomLoader.instance != null && RoomLoader.LoadingStart)
            {
                RoomLoader.EndLoad += StartIniTrial;
            }
            else
            {
                Debug.LogWarning("TrialManager: Either RoomLoader insn't in the scene or RoomLoader.inLoading is false");
                StartIniTrial();
            }
        }
    }
    IEnumerator Debugging()
    {
        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        EnableDiaCanvases(false);
        if (startLastIndex)
            StartTrialAtIndex(trials[GameManager.instance.currentChapter].TrialSequences.Count - 1);
        else
            StartTrialAtIndex(DebugIndex);
        yield break;
    }
    void EnableDiaCanvases(bool to)
    {
        if (DialogueAnimConfig.instance != null)
        {
            DialogueAnimConfig.instance.EnableCanvases(to);
            DialogueAnimConfig.instance.EnableReticleCanvas(to);
            DialogueAnimConfig.instance.EnableFreezeCanvas(false);
        }
    }
    private void OnDestroy()
    {
        RoomLoader.EndLoad -= StartIniTrial;
    }
    void StartIniTrial()
    {
        RoomLoader.EndLoad -= StartIniTrial;
        EnableDiaCanvases(false);
        // TO-DO: Initialize dead characters
        if (!GameSaver.LoadingFile)
            StartCoroutine(InitializeTrial());
    }
    IEnumerator InitializeTrial()
    {
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.DisablePlayer();
        }
        try
        {
            if (trials[GameManager.instance.currentChapter] != null)
                StartCoroutine(StartTrial(trials[GameManager.instance.currentChapter]));
        }
        catch (Exception e)
        {
            Debug.LogError("GameManager may be missing, or current chapter does not have a trial asset.  \n" +
                "Error: " + e.ToString());
        }
        if (GlobalFade.instance.IsDark)
        {
            GlobalFade.instance.FadeOut(0.5f);
        }
        yield break;
    }
    public void StartTrialAtIndex(int startIndex) => StartCoroutine(StartTrial(trials[GameManager.instance.currentChapter], startIndex));
    IEnumerator StartTrial(TrialBuilder trial, int startIndex = 0)
    {
        for(int i = startIndex; i < trial.TrialSequences.Count; i++)
        {
            if(!(skip + i < trial.TrialSequences.Count))
            {
                Debug.LogWarning("TOO BIG");
            }
            if (skip > 0 && skip + i < trial.TrialSequences.Count)
            {
                if (DebugMode)
                    Debug.Log("At: " + skip + " To: " + (skip+i));

                i += skip;
                skip = 0;
            }
            var trialSequence = trial.TrialSequences[i];
            currentSequence = trialSequence;

            GameObject seq = GameObject.Find(trialSequence.MinigameType.Prefab.name);
            if (seq != null && DebugMode)
                Debug.Log(trialSequence.MinigameType.Prefab.name + " Prefab has been found in the Scene");

            if (seq == null)
                Instantiate(trialSequence.MinigameType.Prefab);
            
            try
            {
                trialSequence.MinigameType.manager.Invoke(trialSequence.MinigameAsset);
                InMinigame = !(trialSequence.SequenceType == "Trial Discussion");
                if (trialSequence.SequenceType != "Trial Discussion")
                    GameManager.SetControls(trialSequence.MinigameType.ControlsKey);
                Debug.LogWarning("Should Be Running");
            }
            catch (Exception e)
            {
                Debug.LogError("Something is Missing or is not properly implemented/hooked up \n " +
                    "Error: " + e.ToString());
            }

            while (!TrialSequenceFinished)
            {
                yield return null;
            }
            Debug.Log(trialSequence.SequenceType);
            if (trialSequence.SequenceType == "Trial Discussion" && i+1 < trial.TrialSequences.Count
                && trial.TrialSequences[i+1].SequenceType != "Trial Discussion")
            {
                TrialDialogueManager.instance.HideUI();
            }
            EndTrialSequence();

            

            if (onlyFirstSequence && i == 0)
                break;
        }
        if ((useStringOnEnd && trial.EndSceneName == "") || (!useStringOnEnd && !trial.EndGate))
            yield break;
        EndTrial(trial);
        yield break;
    }
    void EndTrial(TrialBuilder trial)
    {
        StartCoroutine(EndingTrial(trial));
    }
    IEnumerator EndingTrial(TrialBuilder trial) // Keeping passing the builder just incase I need it
    {
        // End Trial Process
        if (video)
        {
            GlobalFade.instance.FadeTo(0.2f);
            yield return new WaitForSeconds(0.2f);
            videoCan.enabled = true;
            video.Prepare();
            yield return new WaitUntil(() => video.isPrepared);
            video.Play();

            yield return new WaitUntil(() => video.isPlaying);
            yield return new WaitUntil(() => video.frame >= 1);
            
            RenderTexture x = new RenderTexture(Screen.width, Screen.height, 32);
            videoProjector.texture = x;
            video.targetTexture = x;
            if(endSound != null)
                SoundManager.instance.PlaySFX(endSound);
            yield return new WaitForSeconds(0.2f);
            GlobalFade.instance.FadeOut(0.2f);
            yield return new WaitUntil(() => !video.isPlaying);
        }

        if (!skipAnimatorFadeOut)
        {
            GlobalFade.instance.FadeTo(0.3f);
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            GlobalFade.instance.FadeTo(0);
        }
        GameManager.instance.addState = AddState.Conclusion;
        if (ProgressionManager.instance.CheckObjective())
        {
            Debug.Log("CLEARED INSTANCE DATA AND CHANGING OBJECTIVE");
            if (!useStringOnEnd)
                RoomLoader.instance.SetToGate(trial.EndGate);
            ProgressionManager.instance.ChangeObjective();
            RoomInstanceManager.instance.ClearRoomData();
            RoomLoader.PreEndLoad += Out;
            if (useStringOnEnd)
                SceneManager.LoadSceneAsync(trial.EndSceneName);
            else
                SceneManager.LoadSceneAsync(trial.EndGate.toAreaName);

            DialogueTextConfig.instance.ResetTrialBox();
            PlayerManager.instance.EnableMainCamera(true);
        }
        
        yield break;
    }
    void Out()
    {
        RoomLoader.PreEndLoad -= Out;
        GlobalFade.instance.FadeOut(0.3f);
    }
    public static void EndTrialSequence() => TrialSequenceFinished = !TrialSequenceFinished;

    public static void SkipSequences(int skipNum) => skip = skipNum;

    public static void ResumeSequence()
    {
        try
        {
            currentSequence.MinigameType.manager.Invoke(currentSequence.MinigameAsset);
            //Debug.Log("Should Be Running");
        }
        catch
        {
            Debug.LogError("Something is Missing or is not properly implemented/hooked up");
        }
    }
    public TrialBuilder GetCurrentTrial()
    {
        return trials[GameManager.instance.currentChapter];
    }
}
