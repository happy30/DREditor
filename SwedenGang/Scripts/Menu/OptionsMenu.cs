//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.PlayerInfo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Events;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer music = null; // If using unity audio
    public List<AudioClip> Lines = new List<AudioClip>();

    [Header("Sliders")]
    [SerializeField] Slider MusicSlider = null;
    [SerializeField] Slider SFXSlider = null;
    [SerializeField] Slider VoiceSlider = null;
    [SerializeField] Slider SpeedSlider = null;
    [Header("Toggles")]
    [SerializeField] Toggle Bob = null;
    [SerializeField] Toggle Look = null;
    [SerializeField] Toggle Invert = null;
    [SerializeField] Toggle DRCam = null;
    [SerializeField] AudioClip dialogueTest = null;
    /* If using FMOD
    public List<string> Lines = new List<string>();
    readonly string masterBusString = "Bus:/Master/";
    Bus music;
    Bus sfx;
    Bus dialogue;
    */
    public delegate void SettingsDel();
    public static event SettingsDel OnUpdateSettings;

    bool updating = false;

    private void Start()
    {
        //music = RuntimeManager.GetBus(masterBusString + "Music");
        //sfx = RuntimeManager.GetBus(masterBusString + "SFX");
        //dialogue = RuntimeManager.GetBus(masterBusString + "Dialogue");
        OnUpdateSettings += UpdateSettings;
    }

    public static void CallUpdateSettings()
    {
        OnUpdateSettings?.Invoke();
    }

    /// <remark>
    /// If when this is called, if the wrong UIOption message 
    /// is displayed on load then add the first Option's UIOptionMessage Display 
    /// after the menu group calls this function, it will override any
    /// options onValueChanged events
    /// - Note from the future, I forgot what this meant
    /// </remark>
    public void UpdateSettings()
    {
        updating = true;
        try
        {
            MusicSlider.value = PlayerInfo.instance.settings.BGMVolume;
            SFXSlider.value = PlayerInfo.instance.settings.SFXVolume;
            VoiceSlider.value = PlayerInfo.instance.settings.VoiceVolume;
            SpeedSlider.value = PlayerInfo.instance.settings.TextSpeed;
            Bob.isOn = PlayerInfo.instance.settings.MovementBob;
            Look.isOn = PlayerInfo.instance.settings.LookInvert;
            Invert.isOn = PlayerInfo.instance.settings.InvertX;
            DRCam.isOn = PlayerInfo.instance.settings.DRCameraPan;
        }
        catch
        {
            Debug.LogWarning("Your options menu is missing sliders and toggles");
        }
        updating = false;
    }
    public void SetMusic(float num)
    {
        music.SetFloat("Music", num);
        //music.setVolume(num);
        PlayerInfo.instance.settings.BGMVolume = num;
    }
    public void SetSFX(float num)
    {
        music.SetFloat("SFX", num);
        //sfx.setVolume(num);
        if (!updating)
        {
            SoundManager.instance.PlaySelect();
        }
        PlayerInfo.instance.settings.SFXVolume = num;

    }
    public void SetVoice(float num)
    {
        music.SetFloat("Voice", num);
        //dialogue.setVolume(num);
        //SoundManager.instance.PlayVoiceTest();
        if (!updating)
        {
            PlayVoice();
        }
        
        PlayerInfo.instance.settings.VoiceVolume = num;
    }

    public void PlayVoice()
    {
        if (Lines.Count != 0)
            SoundManager.instance.PlayVoiceLine(Lines[Random.Range(0, Lines.Count - 1)]);
        else
            SoundManager.instance.PlayVoiceLine(dialogueTest);
    }
    public void SetTextSpeed(float num)
    {
        PlayerInfo.instance.settings.TextSpeed = num;
    }
    public void SetMovementBob(bool b)
    {
        PlayerInfo.instance.settings.MovementBob = b;
    }
    public void SetLookInvert(bool b)
    {
        PlayerInfo.instance.settings.LookInvert = b;
    }
    public void SetInvertX(bool b)
    {
        PlayerInfo.instance.settings.InvertX = b;
    }
    public void SetDRCamPan(bool b)
    {
        PlayerInfo.instance.settings.DRCameraPan = b;
    }
    private void OnDestroy()
    {
        OnUpdateSettings -= UpdateSettings;
    }
}
