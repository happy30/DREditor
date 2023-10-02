//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Video;
using DREditor.Dialogues.Events;
using TMPro;
/// <summary>
/// Main Player or Handler for CG's and Animated CG's
/// Holds Dialogue Events too
/// Requires Dialogue Reader Trio
/// </summary>
public class CGPlayer : MonoBehaviour
{
    public static CGPlayer instance = null;
    [SerializeField] Canvas canvas = null;
    [Header("CGs")]
    [SerializeField] Image CG = null;
    [SerializeField] Image SubCG = null;
    [SerializeField] Image FlashWhiteImage = null;
    [SerializeField] RawImage dialogueBox = null;
    [SerializeField] Image CGBox = null;
    public TextMeshProUGUI cgText = null;
    [SerializeField] float CGStartFadeTime = 1;
    public static bool cgDone = false;
    public static bool inCG = false;

    [Header("Video")]
    [SerializeField] RawImage videoProjector = null; // Same object as videoPlayer where cg box can be used (position)
    [SerializeField] RawImage videoBack = null; // Behind videoProjector
    [SerializeField] VideoPlayer videoPlayer = null; // Same object as videoProjector
    [SerializeField] float VidEndFadeTime = 0.5f;

    [Header("Icons")]
    public DialogueIcon mouseIcon = null;
    public DialogueIcon autoIcon = null;

    [Header("Nameplate")]
    public RawImage namePlate = null;

    [Header("Audio")]
    [SerializeField] AudioClip flashbackStartSFX = null;
    [SerializeField] AudioClip flashbackEndSFX = null;

    public static bool vidDone = false;
    public static bool inVid = false;
    public static bool inEndVideo = false;

    bool visible = false;
    RenderTexture rt;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        if (!canvas)
            Debug.LogError("CG Player Requires you reference a Canvas!");
        canvas.enabled = false;
    }
    private void Start()
    {
        DialogueEventSystem.StartListening("CGFlash", FlashWhite);
        DialogueEventSystem.StartListening("ShakeCG", ShakeCG);
        DialogueEventSystem.StartListening("ShowCG", ChangeCG);
        DialogueEventSystem.StartListening("HideCG", EndCG);
        DialogueEventSystem.StartListening("ShowVideo", PlayVideo);
        DialogueEventSystem.StartListening("HideVideo", EndVideo);
        DialogueEventSystem.StartListening("Flashback", Flashback);
        UIHandler.ToTitle += ResetCGPlayer;
    }
    void ResetCGPlayer()
    {
        namePlate.texture = DialogueAssetReader.instance.transparentImage;
        cgText.text = "";
        if (mouseIcon != null)
        {
            mouseIcon.TurnOff();
            mouseIcon.ResetAnimator();
        }
    }
    private void Update()
    {
        //if (rt != null) { Graphics.Blit(rt, (RenderTexture)null); }

    }
    void EnableUI(bool to)
    {
        CGBox.enabled = to;
        if (GameManager.instance.currentMode == GameManager.Mode.Trial)
            TrialDialogueManager.instance.EnableIcons(to);
        else
            DialogueAssetReader.instance.EnableIcons(to);
    }
    void SwapActiveUI(bool to) // Swaps the UI the Dialogue Asset Reader between CG and non CG
    {
        DialogueTextConfig.instance.SwapBox(to);
        
        if (GameManager.instance.currentMode == GameManager.Mode.Trial && TrialDialogueManager.instance != null)
        {
            TrialDialogueManager.instance.SwapNamePlate();
            TrialDialogueManager.instance.SwapIcons();
            TrialDialogueManager.instance.SetNameTransparent();
        }
        else
        {
            DialogueAssetReader.instance.SwapNamePlate(to);
            DialogueAssetReader.instance.SwapIcons(to);
            DialogueAssetReader.instance.SetNameTransparent();
        }
        visible = to;
    }
    void FlashWhite(object values = null)
    {
        if (!FlashWhiteImage)
            return;
        FlashWhiteImage.DOFade(0.15f, 0.1f)
            .SetLoops(2, LoopType.Yoyo);
    }

    void ShakeCG(object values = null)
    {
        SOTuple data = (SOTuple)values;
        if (inVid)
            videoPlayer.GetComponent<RectTransform>().DOShakeAnchorPos(data.duration, data.strength, data.vibrato, data.randomness, data.fadeOut);
        else
            CG.GetComponent<RectTransform>().DOShakeAnchorPos(data.duration, data.strength, data.vibrato, data.randomness, data.fadeOut);

    }

    #region CG
    public void ChangeCG(object value = null) => StartCoroutine(StartCGAnim(value, false));
    public void EndCG(object value = null) => StartCoroutine(StartCGAnim(value, true));

    private GameObject activePrefab = null;
    private Animator prefabAnimator = null;
    private VideoPlayer prefabVideo = null;
    IEnumerator StartCGAnim(object value, bool endCG)
    {
        SCGTuple data = new SCGTuple();
        Sprite sprite = null;
        if (value != null)
        {
            data = (SCGTuple)value;
            sprite = data.CG;
        }
        if (data.toDeadly)
            GameManager.instance.ChangeState(GameManager.State.Deadly);
        //if (GameSaver.LoadingFile)
        //DialogueAnimConfig.instance.HideMainUI();
        if (!endCG && !inCG) // Start of CG
        {
            //Debug.LogWarning("Main UI should hide here");
            DialogueAnimConfig.instance.HideMainUI();
            yield return new WaitForSeconds(1);
            //cgFade.DOFade(1, 1);
            if (!inVid)
            {
                GlobalFade.instance.FadeTo(CGStartFadeTime);
                yield return new WaitForSeconds(CGStartFadeTime);
            }
            
            DialogueAnimConfig.instance.EnableCanvases(false);
            canvas.enabled = true;
            dialogueBox.enabled = false;

            if (inVid)
            {
                videoProjector.DOFade(0, 0.5f);
                videoBack.DOFade(0, 0.5f);
                videoPlayer.playbackSpeed = 1;
                videoPlayer.isLooping = false;
                videoPlayer.Stop();
                rt = null;
                inVid = false;
            }
            else
            {
                
                CGBox.enabled = true;
            }
            if (!visible)
            {
                SwapActiveUI(true);
            }
            if (data.prefab != null) // CG prefab will be instantiated
            {
                //Debug.LogWarning("Prefab CG Instantiated");
                activePrefab = Instantiate(data.prefab);
                prefabAnimator = activePrefab.GetComponent<Animator>();
                prefabVideo = activePrefab.GetComponentInChildren<VideoPlayer>();
                CG.DOFade(0, 0);
                //Debug.LogWarning("Prefab Animator is: " + prefabAnimator != null);
            }

            if (GlobalFade.instance.IsDark && !GameSaver.LoadingFile)
                GlobalFade.instance.FadeOut(0.3f);

            if (prefabAnimator != null && data.playOnly)
            {
                
                yield return new WaitForSeconds(prefabAnimator.GetCurrentAnimatorStateInfo(0).length);
            }
        }



        if (endCG)
        {
            if (!DialogueAnimConfig.Reset)
                yield return new WaitForSeconds(1);
            //cgFade.DOFade(1, 1);
            GlobalFade.instance.FadeTo(1);
            if (!DialogueAnimConfig.Reset)
                yield return new WaitForSeconds(1);

            if (activePrefab != null)
                Destroy(activePrefab);

            canvas.enabled = false;
            DialogueTextConfig.instance.ClearText();
            SwapActiveUI(false);
            if (GameManager.instance.currentMode != GameManager.Mode.Trial)
                DialogueAnimConfig.instance.EnableCanvases(true);
            CG.DOFade(0, 0);
            SubCG.DOFade(0, 0);
            dialogueBox.enabled = true;
            CGBox.enabled = false;
            inCG = false;
            activePrefab = null;
            prefabAnimator = null;
            prefabVideo = null;
        }
        else if (inCG)
        {
            if (data.ScreenFadeOut)
            {
                GlobalFade.instance.FadeTo(1);
                yield return new WaitForSeconds(1);
                if (activePrefab != null)
                {
                    Destroy(activePrefab);
                    activePrefab = null;
                    prefabAnimator = null;
                    prefabVideo = null;
                }

                DialogueAssetReader.instance.SetNameTransparent();
                if (data.eventName != null)
                    SoundManager.instance.PlaySFX(data.eventName);
            }
            if (data.prefab != null) // CG prefab will be instantiated
            {
                activePrefab = Instantiate(data.prefab);
                prefabAnimator = activePrefab.GetComponent<Animator>();
                prefabVideo = activePrefab.GetComponentInChildren<VideoPlayer>();
                if (prefabVideo != null)
                {
                    //Debug.LogWarning("Prefab video waited");
                    yield return new WaitUntil(() => prefabVideo.isPrepared);
                }
                CG.DOFade(0, 1);
                //yield return new WaitForSeconds(1);
            }

            

            if (!(activePrefab != null))
            {
                SubCG.DOFade(1, 0);
                SubCG.sprite = sprite;
                float instant = data.transitionInstant? 0 : 1;
                CG.DOFade(0, instant);
                yield return new WaitForSeconds(instant);
                CG.sprite = sprite;
                CG.DOFade(1, 0);
            }
        }
        else // Start of CG also
        {
            if (visible) // When going from Video to CG
                EnableUI(true);
            inCG = true;
            CG.sprite = sprite;
            if (data.prefab == null)
                CG.DOFade(1, 0);
        }
        if (GlobalFade.instance.IsDark && !GameSaver.LoadingFile && !UIHandler.GoingToTitle())
        {
            yield return new WaitForSeconds(Time.deltaTime);
            GlobalFade.instance.FadeOut(CGStartFadeTime);
            yield return new WaitForSecondsRealtime(CGStartFadeTime);
            if (prefabAnimator != null && data.playOnly)
            {
                //Debug.LogWarning("Prefab animator length: " + prefabAnimator.GetCurrentAnimatorStateInfo(0).length);
                yield return new WaitForSecondsRealtime(prefabAnimator.GetCurrentAnimatorStateInfo(0).length);
            }
        }

        if (DialogueAnimConfig.instance.blurTween != null)
            DialogueAnimConfig.instance.StopBlur();
        
        if (endCG && !DialogueAnimConfig.Reset && GameManager.instance.currentMode != GameManager.Mode.Trial)
            DialogueAnimConfig.instance.ShowMainUI();
        
        cgDone = true;
        yield break;
    }

    #endregion

    #region Play Video

    public void FadeBox(bool to)
    {
        if (to)
            CGBox.DOFade(1, 0.3f);
        else
            CGBox.DOFade(0, 0.3f);
    }
    public void PlayVideo(object value = null) => StartCoroutine(VideoPlay(value, false));
    public void EndVideo(object value = null) => StartCoroutine(VideoPlay(value, true));
    IEnumerator VideoPlay(object value, bool endVid)
    {
        SVTuple data = (SVTuple)value;
        VideoClip v = data.mainClip;
        VideoClip i = data.iniClip;
        bool looping = data.isLooping;
        bool waitEnd = data.waitToEnd;
        if (data.stopSound)
            SoundManager.instance.StopSound();
        if (data.toDeadly)
            GameManager.instance.ChangeState(GameManager.State.Deadly);
        if (data.waitToEnd || data.playOnly)
        {
            if (GameManager.instance.currentMode == GameManager.Mode.Trial && TrialDialogueManager.instance != null)
            {
                TrialDialogueManager.instance.cantToggle = true;
            }
            else
                DialogueAssetReader.instance.cantToggle = true;

            if (autoIcon != null)
            {
                autoIcon.gameObject.SetActive(false);
            }
        }
        if (!endVid && !inVid) // Fade to black at beginning 
        {
            //Debug.Log("Fading original");
            DialogueAnimConfig.instance.HideMainUI();
            yield return new WaitForSeconds(1);
            //cgFade.DOFade(1, 1);
            GlobalFade.instance.FadeTo(CGStartFadeTime);
            yield return new WaitForSeconds(CGStartFadeTime);
            DialogueAnimConfig.instance.EnableCanvases(false);
            canvas.enabled = true;
            if (!visible)
            {
                SwapActiveUI(true);
                Debug.Log("Called vid on");
            }
            if (waitEnd && visible)
            {
                EnableUI(false);
            }
            if (data.eventName != null && !waitEnd) // Added 11-17 to allow for playing sound in middle of fade
                SoundManager.instance.PlaySFX(data.eventName);
            DialogueAssetReader.instance.SetNameTransparent();
            if (!data.playOnly && !waitEnd)
                CGBox.enabled = true;

            if (autoIcon != null && waitEnd)
            {
                autoIcon.gameObject.SetActive(false);
            }
            if (autoIcon != null && waitEnd && data.playOnly)
            {
                autoIcon.gameObject.SetActive(false);
            }
        }

        if (endVid)
        {
            //Debug.LogWarning("END VIDEO CALLED");
            if (!DialogueAnimConfig.Reset)
                yield return new WaitForSeconds(1);
            //cgFade.DOFade(1, 1);
            GlobalFade.instance.FadeTo(1);
            if (!DialogueAnimConfig.Reset)
                yield return new WaitForSeconds(1);
            canvas.enabled = false;
            if (GameManager.instance.currentMode != GameManager.Mode.Trial)
                DialogueAnimConfig.instance.EnableCanvases(true);
            DialogueTextConfig.instance.ClearText();
            if (visible)
                SwapActiveUI(false);
            if (inCG)
            {
                CG.DOFade(0, 0);
                SubCG.DOFade(0, 0);
                if (GameManager.instance.currentMode != GameManager.Mode.Trial)
                    dialogueBox.enabled = true;
                CGBox.enabled = false;
                inCG = false;
            }
            DialogueAssetReader.instance.lastActor = null;
            videoProjector.DOFade(0, 0);
            videoBack.DOFade(0, 0);
            videoPlayer.playbackSpeed = 1;
            videoPlayer.isLooping = false;
            videoPlayer.Stop();
            FadeBox(true);
            //dialogueBox.enabled = true;
            //if (DialogueAnimConfig.instance.inDialogue)
                //CGBox.enabled = true;
            
            rt = null;
            inVid = false;
            
        }
        else if (i) // has initial, wait till done and loop main piece
        {
            if (data.ScreenFadeOut)
            {
                GlobalFade.instance.FadeTo(1);
                yield return new WaitForSeconds(1);
                if (data.eventName != null)
                    SoundManager.instance.PlaySFX(data.eventName);
            }
            inVid = true;
            videoBack.DOFade(1, 0);
            videoPlayer.clip = i;
            rt = new RenderTexture(1920, 1080, 32);
            videoBack.texture = rt;
            videoProjector.texture = rt;
            videoPlayer.targetTexture = rt;
            videoProjector.DOFade(1, 0);


            videoPlayer.Prepare();
            yield return new WaitUntil(() => videoPlayer.isPrepared);
            videoPlayer.Play();

            yield return new WaitUntil(() => videoPlayer.isPlaying);
            yield return new WaitUntil(() => videoPlayer.frame >= 1);
            videoPlayer.playbackSpeed = 0;

            //Debug.Log("Video should be playing");
            //cgFade.DOFade(0, 1);
            GlobalFade.instance.FadeOut(1);
            yield return new WaitForSeconds(1);
            videoPlayer.playbackSpeed = data.speed;
            yield return new WaitUntil(() => videoPlayer.frame == (long)videoPlayer.frameCount - 1);
            //yield return new WaitUntil(() => videoPlayer.isPlaying);
            //yield return new WaitUntil(() => !videoPlayer.isPlaying);
            RenderTexture x = new RenderTexture(1920, 1080, 32);
            //Debug.Log("Initial has finished");
            //videoPlayer.Stop();
            videoProjector.texture = x;
            videoPlayer.targetTexture = x;
            videoPlayer.clip = v;
            videoPlayer.isLooping = true;
            videoPlayer.Play();
        }
        else if (inVid)
        {
            if (data.waitToEnd)
            {
                if (GameManager.instance.currentMode == GameManager.Mode.Trial && TrialDialogueManager.instance != null)
                {
                    TrialDialogueManager.instance.SetNameTransparent();
                }
                else
                    DialogueAssetReader.instance.SetNameTransparent();

                if (autoIcon != null)
                {
                    autoIcon.gameObject.SetActive(false);
                }

                FadeBox(false);
            }
            if (data.ScreenFadeOut)
            {
                GlobalFade.instance.FadeTo(1);
                yield return new WaitForSeconds(1);
                
            }
            
            RenderTexture x = new RenderTexture(1920, 1080, 32);
            videoBack.texture = x;
            //videoProjector.texture = x;
            videoPlayer.targetTexture = x;
            videoPlayer.isLooping = looping;
            videoPlayer.clip = v;

            videoPlayer.Prepare();
            yield return new WaitUntil(() => videoPlayer.isPrepared);

            videoPlayer.Play();

            //yield return new WaitUntil(() => videoPlayer.isPlaying);
            yield return new WaitUntil(() => videoPlayer.frame >= 1);
            yield return new WaitForEndOfFrame();
            videoPlayer.playbackSpeed = 0;
            if (data.eventName != null)
                SoundManager.instance.PlaySFX(data.eventName);
            //cgFade.DOFade(0, 1);
            //yield return new WaitForSeconds(1);
            RenderTexture l = new RenderTexture(1920, 1080, 32);
            
            videoPlayer.targetTexture = l;
            videoPlayer.playbackSpeed = data.speed;
            yield return new WaitForSecondsRealtime(Time.deltaTime);
            videoProjector.texture = l;
        }
        else
        {
            inVid = true;
            videoBack.DOFade(1, 0);
            videoPlayer.clip = v;
            videoPlayer.isLooping = looping;
            rt = new RenderTexture(1920, 1080, 32);
            videoBack.texture = rt;
            videoProjector.texture = rt;
            videoPlayer.targetTexture = rt;
            videoProjector.DOFade(1, 0);


            videoPlayer.Prepare();
            yield return new WaitUntil(() => videoPlayer.isPrepared);
            videoPlayer.Play();

            yield return new WaitUntil(() => videoPlayer.isPlaying);
            yield return new WaitUntil(() => videoPlayer.frame >= 1);
            //videoPlayer.playbackSpeed = 0;
            RenderTexture x = new RenderTexture(1920, 1080, 32);
            videoProjector.texture = x;
            videoPlayer.targetTexture = x;
            // Below If statement was to use the sound so we could not use videos that have sound on 11-17
            if (data.eventName != null && waitEnd)
                SoundManager.instance.PlaySFX(data.eventName);

            GlobalFade.instance.FadeOut(1);
            yield return new WaitForSeconds(1);
            
            videoPlayer.playbackSpeed = data.speed;
            //if (data.eventName != "")
                //SoundManager.instance.PlaySFX(data.eventName);
        }
        if (GlobalFade.instance.IsDark && !DialogueAnimConfig.Reset) // Unfade opening then play
        {
            yield return new WaitForSeconds(Time.deltaTime);
            //cgFade.DOFade(0, 1);
            GlobalFade.instance.FadeOut(1);
            if (GameManager.instance.currentMode != GameManager.Mode.Trial) // So it transitions correctly in trial
                yield return new WaitForSeconds(1);

        }

        if (DialogueAnimConfig.instance.blurTween != null)
            DialogueAnimConfig.instance.StopBlur();

        if (endVid && !DialogueAnimConfig.Reset && GameManager.instance.currentMode != GameManager.Mode.Trial)
        {
            DialogueAnimConfig.instance.ShowMainUI();
        }
        if (waitEnd)
        {
            
            yield return new WaitUntil(() => !videoPlayer.isPlaying);
            videoPlayer.frame = (long)Mathf.Lerp(videoPlayer.frame, videoPlayer.clip.frameCount, 1);
            //yield return new WaitUntil(() => videoPlayer.frame == (long)videoPlayer.clip.frameCount);
            videoPlayer.frame = (long)videoPlayer.clip.frameCount;
            if (data.playOnly)
            {
                GlobalFade.instance.FadeTo(VidEndFadeTime);
                yield return new WaitForSeconds(VidEndFadeTime);
                DialogueAssetReader.instance.EnableIcons(true);
                canvas.enabled = false;
                if (GameManager.instance.currentMode != GameManager.Mode.Trial)
                    DialogueAnimConfig.instance.EnableCanvases(true);
                DialogueTextConfig.instance.ClearText();
                SwapActiveUI(false);
                if (inCG)
                {
                    CG.DOFade(0, 0);
                    SubCG.DOFade(0, 0);
                    dialogueBox.enabled = true;
                    CGBox.enabled = false;
                    inCG = false;
                }
                DialogueAssetReader.instance.lastActor = null;
                videoProjector.DOFade(0, 0);
                videoBack.DOFade(0, 0);
                videoPlayer.Stop();
                videoPlayer.playbackSpeed = 1;
                //dialogueBox.enabled = true;
                //if (DialogueAnimConfig.instance.inDialogue)
                //CGBox.enabled = true;
                videoPlayer.isLooping = false;
                rt = null;
                inVid = false;
                //Debug.Log("Is Play Only");
            }
            else
            {
                EnableUI(true);
                //Debug.Log("Not Play Only");
            }
            if (GameManager.instance.currentMode == GameManager.Mode.Trial && TrialDialogueManager.instance != null)
            {
                TrialDialogueManager.instance.cantToggle = false;
            }
            else
                DialogueAssetReader.instance.cantToggle = false;
        }
        vidDone = true;
        yield break;
    }
    #endregion

    #region EndVideo
    
    public void PlayEndVideo(VideoClip v) => StartCoroutine(EndVideoPlay(v));
    IEnumerator EndVideoPlay(VideoClip v)
    {
        DialogueAnimConfig.instance.HideMainUI();
        yield return new WaitForSeconds(1);
        GlobalFade.instance.FadeTo(1);
        //cgFade.DOFade(1, 1);
        yield return new WaitForSeconds(1);
        DialogueAssetReader.instance.namePlate.enabled = false;
        //dialogueBox.gameObject.SetActive(false);


        inEndVideo = true;
        videoBack.DOFade(1, 0);
        videoPlayer.clip = v;
        rt = new RenderTexture(Screen.width, Screen.height, 32);
        videoBack.texture = rt;
        videoProjector.texture = rt;
        videoPlayer.targetTexture = rt;
        videoProjector.DOFade(1, 0);
        yield return new WaitForSeconds(1);

        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);
        videoPlayer.Play();

        yield return new WaitUntil(() => videoPlayer.isPlaying);
        yield return new WaitUntil(() => videoPlayer.frame >= 1);
        videoPlayer.playbackSpeed = 0;
        //cgFade.DOFade(0, 0);
        GlobalFade.instance.FadeOut(0);
        videoPlayer.playbackSpeed = 1;

        yield return new WaitUntil(() => !videoPlayer.isPlaying);

        yield return new WaitForSeconds(1);
        //cgFade.DOFade(1, 1);
        GlobalFade.instance.FadeTo(1);
        yield return new WaitForSeconds(1);
        videoProjector.DOFade(0, 0);
        videoBack.DOFade(0, 0);
        videoPlayer.Stop();
        DialogueAssetReader.instance.namePlate.enabled = true;
        //dialogueBox.gameObject.SetActive(true);

        rt = null;

        if (GlobalFade.instance.IsDark) // Unfade opening then play
        {
            yield return new WaitForSeconds(Time.deltaTime);
            //cgFade.DOFade(0, 1);
            GlobalFade.instance.FadeOut(1);
            yield return new WaitForSeconds(1);

        }

        DialogueAnimConfig.instance.ShowMainUI();
        inEndVideo = false;
        DialogueAnimConfig.instance.ReticleCanvas.enabled = true;
        DialogueAnimConfig.instance.dialogueCanvas.enabled = false;
        yield break;
    }
    #endregion

    #region Flashbacks
    public void Flashback(object value = null) => StartCoroutine(StartFlashback(value));
    IEnumerator StartFlashback(object value)
    {
        DREditor.Dialogues.Events.FBTuple data;
        if (value != null)
        {
            data = (DREditor.Dialogues.Events.FBTuple)value;
        }
        else
        {
            Debug.LogWarning("There was an issue");
            cgDone = true;
            yield break;
        }

        if (data.Flashbacks.Count == 0)
        {
            Debug.LogWarning("There was an issue");
            cgDone = true;
            yield break;
        }

        float fadeTime = CGStartFadeTime / 2;
        // fade to white
        GlobalFade.instance.FadeTo(fadeTime, true);
        SoundManager.instance.PlaySFX(flashbackStartSFX);
        yield return new WaitForSeconds(fadeTime);
        EnableUI(false);
        if (autoIcon != null)
            autoIcon.gameObject.SetActive(false);
        DialogueAnimConfig.instance.EnableCanvases(false);
        DialogueAssetReader.instance.SetNameTransparent();
        DialogueAssetReader.instance.EnableCGNameplate(false);
        canvas.enabled = true;

        

        CG.sprite = data.Flashbacks[0];
        CG.DOFade(1, 0);

        GlobalFade.instance.FadeOut(fadeTime, true);
        yield return new WaitForSeconds(fadeTime);

        for (int i = 0; i < data.Flashbacks.Count; i++)
        {
            Sprite sprite = data.Flashbacks[i];
            SubCG.DOFade(1, 0);
            SubCG.sprite = sprite;
            CG.DOFade(0, 1);
            yield return new WaitForSeconds(1); // time it takes to fade to the next image
            CG.sprite = sprite;
            CG.DOFade(1, 0);
            yield return new WaitForSeconds(1.5f); // time it takes after you finish going to the next image
        }

        GlobalFade.instance.FadeTo(fadeTime, true);
        SoundManager.instance.PlaySFX(flashbackEndSFX);
        yield return new WaitForSeconds(fadeTime);

        canvas.enabled = false;

        if (GameManager.instance.currentMode != GameManager.Mode.Trial)
            DialogueAnimConfig.instance.EnableCanvases(true);
        CG.DOFade(0, 0);
        SubCG.DOFade(0, 0);
        DialogueAssetReader.instance.EnableCGNameplate(true);
        GlobalFade.instance.FadeOut(fadeTime, true);
        yield return new WaitForSeconds(fadeTime);

        cgDone = true;
        yield break;
    }
    #endregion
}
