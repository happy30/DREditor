//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.PlayerInfo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MinigameManagerBase : MonoBehaviour
{
    [HideInInspector] public bool canUseStamina = false;
    
    public void TakeDamage(int damage) => PlayerInfo.instance.TakeDamage(damage);
    public void ResetHealth() => PlayerInfo.instance.ResetHealth();
    public void DrainStamina() => PlayerInfo.instance.DrainStamina();
    public void RegenStamina() => PlayerInfo.instance.RegenStamina();
    public void EndMinigame() => TrialManager.EndTrialSequence();
    public void PlayMusic(AudioClip eventString) => SoundManager.instance.PlayMusic(eventString);
    public void PlaySFX(AudioClip eventString) => SoundManager.instance.PlaySFX(eventString);
    public void PlayVoiceLine(AudioClip eventString) => SoundManager.instance.PlayVoiceLine(eventString);

    
}
