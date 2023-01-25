//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class SoundManager : MonoBehaviour
{
    /* If using FMOD
    readonly string masterBusString = "Bus:/Master/";
    Bus music;
    Bus sfx;
    Bus dialogue;
    [Header("Debugging & Testing")]
    [SerializeField] bool useDebugVolume = false;
    [Range(0,2)]
    [SerializeField] float debugMusic = 1;
    [Range(0, 2)]
    [SerializeField] float debugSFX = 1;
    [Range(0, 2)]
    [SerializeField] float debugDia = 1;
    */

    [Header("Main UI Music Box")]
    [SerializeField] TMP_Text boxText = null;
    [SerializeField] Playlist jukeBox = null;
    //[SerializeField] bool startWithMusic = true;
    //[SerializeField] AudioClip startMusic = null;
    //[SerializeField] TMPMarquee tmp = null;
    [SerializeField] bool playOnStart = false;
    public AudioClip StartMusic = null;
    [SerializeField] string stopMusicCode = "Null";
    [Header("Audio Sources")]
    public AudioSource MusicSource = null;
    [SerializeField] AudioSource SFXSource = null;
    [SerializeField] AudioSource VoiceSource = null;
    [SerializeField] AudioSource EnvSource = null;
    [SerializeField] AudioSource DialogueNextSource = null;
    [SerializeField] AudioSource SubmitSource = null;
    [SerializeField] AudioSource CancelSource = null;
    [SerializeField] AudioSource SelectSource = null;
    [SerializeField] AudioSource PopUpSource = null;
    [SerializeField] AudioSource VoiceTestSource = null;
    [SerializeField] AudioSource ObserveSource = null;
    [SerializeField] AudioSource InstantDiaSource = null;
    [SerializeField] AudioSource ChoiceSource = null;
    
    [SerializeField] AudioClip LockedSFX = null;
    public static SoundManager instance = null;
    public string LastPlayedMusic = null;
    public string LastPlayedEnv = null;
    public bool VoicePlaying() => VoiceSource.isPlaying;
    public delegate void MusicDel(AudioSource i);
    public static event MusicDel OnMusicChange;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        /*
        if (useDebugVolume)
        {
            music = RuntimeManager.GetBus(masterBusString + "Music");
            sfx = RuntimeManager.GetBus(masterBusString + "SFX");
            dialogue = RuntimeManager.GetBus(masterBusString + "Dialogue");
            music.setVolume(debugMusic);
            sfx.setVolume(debugSFX);
            dialogue.setVolume(debugDia);
        }
        */
        StartCoroutine(MusicStart());
    }
    IEnumerator MusicStart()
    {
        //yield return new WaitUntil(() => FMODUnity.RuntimeManager.IsInitialized);
        if (playOnStart)
        {
            PlayMusic(StartMusic);
        }
        yield break;
    }
    public void StopObserveSFX() => ObserveSource.Stop();
    public void PlayObserve() => ObserveSource.Play();
    public void PlayEnv(AudioClip eventName)
    {
        if (eventName == null)
            return;
        try
        {
            EnvSource.Stop();
            EnvSource.clip = eventName;
            LastPlayedEnv = eventName.name;
            EnvSource.Play();
        }
        catch (Exception e)
        {
            Debug.LogWarning("ERROR OCCURED PLAYING ENVIRONMENT SOUND: " + eventName + "\n " +
                "With Error: " + e.ToString());
        }
        
    }
    public void StopEnv()
    {
        Debug.LogWarning("Reset Env Sound");
        LastPlayedEnv = "";
        EnvSource.Stop();
    }
    public void PlaySFX(AudioClip eventName)
    {
        if (eventName == null)
        {
            //Debug.LogWarning("CANCELED SFX: " + eventName);
            return;
        }
        //Debug.LogWarning("Playing SFX: " + eventName);
        SFXSource.Stop();
        SFXSource.clip = eventName;
        SFXSource.Play();
    }
    public void PlayVoiceLine(AudioClip eventName)
    {
        if (eventName == null)
            return;
        VoiceSource.Stop();
        VoiceSource.clip = eventName;
        VoiceSource.Play();
    }
    public void StopVoiceLine() => VoiceSource.Stop();
    public string GetNullCode() => stopMusicCode;
    public bool CurrentEventInMusicIs(AudioClip eventName) => MusicSource.clip == eventName;
    public void PlayMusic(AudioClip eventName, bool skip = false)
    {
        if (eventName.name == stopMusicCode || eventName == null)
        {
            MusicSource.Stop();
            boxText.text = "";
            ResetLastPlayed();
            return;
        }
        
        // code to set boxText name
        if (boxText != null && jukeBox != null)
        {
            boxText.autoSizeTextContainer = false;
            boxText.text = jukeBox.GetTitleFromClip(eventName);
            boxText.autoSizeTextContainer = true;
            boxText.ForceMeshUpdate();
        }
        MusicSource.Stop();
        MusicSource.clip = eventName;
        MusicSource.Play();
        if(!skip)
            LastPlayedMusic = eventName.name;
        StartCoroutine(WaitForUpdate());
        Debug.Log("Playing Music, event name is: " + eventName);
    }
    public bool MusisIsPlaying() => MusicSource.isPlaying;
    IEnumerator WaitForUpdate()
    {
        //yield return new WaitUntil(() => RuntimeManager.IsInitialized);
        yield return new WaitUntil(() => MusicSource.isPlaying);
        OnMusicChange?.Invoke(MusicSource);
        yield break;
    }
    public void ClearText()
    {
        if (boxText != null)
        {
            boxText.text = "";
        }
    }
    public void PlayNext() => PlaySource(DialogueNextSource);
    public void PlaySubmit()
    {
        //Debug.LogWarning("Calling Submit Sound");
        //if(!SubmitSource.IsPlaying())
            PlaySource(SubmitSource);
    }
    public void PlayCancel() => PlaySource(CancelSource);
    public void PlaySelect() => PlaySource(SelectSource);
    public void PlayPopUp() => PlaySource(PopUpSource);
    public void PlayVoiceTest() => PlaySource(VoiceTestSource);
    public void PlayInstantSound() => PlaySource(InstantDiaSource);
    public void PlayChoiceSound() => PlaySource(ChoiceSource);
    public void PlayLockedUI() => PlaySFX(LockedSFX);

    public void ResetLastPlayed() => LastPlayedMusic = "";
    void PlaySource(AudioSource e)
    {
        if (!(e != null) || e.clip == null)
            return;
        e.Play();
    }
    
    public void StopSound()
    {
        MusicSource.Stop();
        StopAllButMusic();
        //EventInstance instance = 
    }
    public void StopAllButMusic()
    {
        StopSoundFX();
        StopVoiceLine();
    }
    public void StopSoundFX()
    {
        SFXSource.Stop();
    }
    public void FadeMusic(float FadeTime) => StartCoroutine(FadeOut(MusicSource, FadeTime));
    public static IEnumerator FadeOut(AudioSource source, float FadeTime)
    {
        // Code to fade out emitter
        source.Stop();
        yield break;
    }
    public string SaveEnv()
    {
        return LastPlayedEnv;
    }
    public void LoadEnv(string name)
    {
        if (name == "")
        {
            Debug.LogWarning("Couldn't find Saved Ambience, no Environment sound playing");
            return;
        }
        Debug.LogWarning("PLAYING LOADED Environment: " + name);
        Debug.LogWarning("WARNING: SWEDEN HAS NOT SET UP ENVIRONMENT SAVING AND LOADING PLEASE DO NOT USE ENVIRONMENT SOUNDS UNTIL FIXED");
        //PlayEnv(name);
    }
    public string SaveMusic()
    {
        return LastPlayedMusic;
    }
    public void LoadMusic(string name)
    {
        AudioClip loaded = jukeBox.GetAudioClip(name);
        if (loaded == null)
        {
            Debug.LogWarning("Couldn't find Saved Music, no music playing");

            boxText.autoSizeTextContainer = false;
            boxText.text = "";
            boxText.autoSizeTextContainer = true;
            boxText.ForceMeshUpdate();
            ResetLastPlayed();
            OnMusicChange?.Invoke(null);
            return;
        }
        if (!(boxText != null))
        {
            boxText = GameObject.Find("Music Text").GetComponent<TMP_Text>();
        }

        // stuff that sets boxtexts name
        if (boxText != null && jukeBox != null)
        {
            boxText.autoSizeTextContainer = false;
            boxText.text = jukeBox.GetTitleFromClip(loaded);
            boxText.autoSizeTextContainer = true;
            boxText.ForceMeshUpdate();
        }
        
        MusicSource.clip = loaded;
        MusicSource.Play();
        LastPlayedMusic = name;
        StartCoroutine(WaitForUpdate());
        Debug.LogWarning("PLAYING LOADED Music: " + boxText.text + "  " + name);

    }
}
