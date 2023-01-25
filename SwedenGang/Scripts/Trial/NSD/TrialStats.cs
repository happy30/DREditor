//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DREditor.PlayerInfo;
using DG.Tweening;
/// <summary>
/// This file is to visually display the characters HP and Influence Gauge to the player.
/// The information on player stats is from the singleton PlayerInfo.instance
/// </summary>
public class TrialStats : MonoBehaviour
{

    [SerializeField] Canvas canvas = null;
    [Header("Health Bar Properties")]
    [Tooltip("The time in seconds waited after the Stat UI shows to take damage")]
    [SerializeField] float LoseHealthDelay = 1;
    [Tooltip("This should be the same as this objects x position and hidden from view")]
    [SerializeField] float UIOffset = 460;
    [Tooltip("This is the color of all health bars when at full health")]
    [SerializeField] Color HealthColor;
    Color CurrentColor;
    [Tooltip("This is the color of all health bars when at low health")]
    [SerializeField] Color EndColor = Color.red;
    [Tooltip("Whenever health is lost, the remaining health bars pulse, use these to scale")]
    [SerializeField] float PulseScaleIn = 0.85f;
    [SerializeField] float PulseScaleOut = 1.15f;
    [SerializeField] float BlinkDuration = 0.5f;
    [Tooltip("# of times the bars pulse")]
    [SerializeField] int HealthBlink = 3;
    readonly int lowHealth = 3;

    [SerializeField] List<RawImage> HPBorders = new List<RawImage>();
    [SerializeField] List<RawImage> HPBars = new List<RawImage>();
    RectTransform Rect => GetComponent<RectTransform>();

    [SerializeField] Color RegenColor;
    [SerializeField] float RegenDuration = 0.5f;

    [Header("Influence Bar Properties")]
    [SerializeField] RawImage InfluenceBar = null;
    [SerializeField] RectTransform InfluenceRect = null;
    [SerializeField] Color InfluenceColor;
    [SerializeField] Color DrainColor;
    [Header("Animation")]
    [SerializeField] Animator animator = null;
    [SerializeField] string showString = "Show";
    [SerializeField] string hideString = "Hide";
    [SerializeField] AudioClip takeDmg = null;
    [SerializeField] AudioClip regenHealth = null;
    //[SerializeField] float VisualDrainMultiplier = 5;
    public static TrialStats Instance = null;
    public static bool showing = false;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }
    public void ShowNSDStats()
    {
        if (showing)
            return;
        showing = true;
        animator.Play(showString);
        Rect.DOMoveX(Rect.position.x - UIOffset * canvas.scaleFactor, 0.5f);
        if (DividedColorValue == new Color(0, 0, 0, 0) && PlayerInfo.instance.CurrentHealth < PlayerInfo.instance.MaxHealth)
            CalculateColor();
    }
    public void HideNSDStats()
    {
        if (!showing)
            return;
        showing = false;
        animator.Play(hideString);
        Rect.DOMoveX(Rect.position.x + UIOffset * canvas.scaleFactor, 0.5f);
    }

    public void TakeDamage()
    {
        ShowNSDStats();
        int currentHealth;
        SoundManager.instance.PlaySFX(takeDmg);
        try
        {
            currentHealth = PlayerInfo.instance.CurrentHealth;
        }
        catch { Debug.LogError("Either the GameManager or the PlayerInfo Component to the GameManager is Missing!"); return; }
        
        if (currentHealth <= lowHealth)
        {
            // Low Health Change
        }
        RemoveHealthAnim(currentHealth);
    }
    
    void RemoveHealthAnim(int barNum)
    {
        if(HPBars.Count != PlayerInfo.instance.MaxHealth)
        {
            Debug.LogError("There's inconsistant health bars, either add more maxhealth in PlayerInfo or add HPBars");
        }

        if (HPBorders[0].GetComponentInParent<Mask>().enabled == true)
                ResetHPMasks();

        CalculateColor();
        HPBorders[barNum].transform.DOMoveY(HPBorders[barNum].transform.position.y - 50, 1)
            .SetDelay(LoseHealthDelay);
        HPBorders[barNum].DOFade(0, 1)
            .SetDelay(LoseHealthDelay);
        HPBars[barNum].DOFade(0, 1)
            .SetDelay(LoseHealthDelay);
        for(int i = barNum-1; i >= 0; i--)
        {
            float wait = 0;
            HPBars[i].DOColor(CurrentColor, BlinkDuration)
                .SetDelay(LoseHealthDelay + wait - (BlinkDuration / 2));

            for (int j = 1; j < HealthBlink+1; j++)
            {
                HPBorders[i].transform.DOScale(PulseScaleIn, BlinkDuration/2)
                .SetDelay(LoseHealthDelay + wait - (BlinkDuration / 2));
                HPBorders[i].transform.DOScale(PulseScaleOut, BlinkDuration / 2)
                .SetDelay(LoseHealthDelay + wait);

                HPBorders[i].transform.DOScale(1f, BlinkDuration)
                .SetDelay(LoseHealthDelay + BlinkDuration + wait - (BlinkDuration / 2));
                wait += BlinkDuration * 2;
            }
            
        }
        HPBorders[barNum].transform.DOMoveY(0, 0)
            .SetDelay(LoseHealthDelay*2);
    }

    Color DividedColorValue;
    public void CalculateColor()
    {
        if (PlayerInfo.instance.CurrentHealth == PlayerInfo.instance.MaxHealth - 1)
        {
            CurrentColor = (EndColor - HealthColor) / PlayerInfo.instance.MaxHealth;
            DividedColorValue = CurrentColor;
            CurrentColor += HealthColor;
        }
        else if (DividedColorValue == new Color(0, 0, 0, 0))
        {
            CurrentColor = (EndColor - HealthColor) / PlayerInfo.instance.MaxHealth;
            DividedColorValue = CurrentColor;
            CurrentColor += HealthColor;
            for(int i = PlayerInfo.instance.MaxHealth-1; i > PlayerInfo.instance.CurrentHealth-1; i--)
            {
                for (int j = 0; j < PlayerInfo.instance.MaxHealth; j++)
                {
                    HPBars[j].DOColor(CurrentColor, 0);
                }
                CurrentColor += DividedColorValue;
            }
            //Debug.Log(PlayerInfo.instance.CurrentHealth - 1);
            for (int i = PlayerInfo.instance.MaxHealth-1; i > PlayerInfo.instance.CurrentHealth-1; i--)
            {
                HPBorders[i].DOFade(0, 0);
                HPBars[i].DOFade(0, 0);
            }
        }
        else
        {
            CurrentColor += DividedColorValue;
        }
    }

    public void RegenHealthVisual()
    {
        SoundManager.instance.PlaySFX(regenHealth);
        float wait = 0;
        for (int i = 0; i < HPBorders.Count; i++)
        {
            
            Mask maskMask = HPBorders[i].GetComponentInParent<Mask>();
            RawImage maskImage = maskMask.GetComponent<RawImage>();
            maskMask.enabled = true;
            maskImage.color = new Color(1, 1, 1, 1);

            HPBorders[i].transform.DOLocalMoveX(-60, 0);
            HPBorders[i].transform.DOLocalMoveY(0, 0);

            HPBorders[i].DOFade(1, 0)
                .SetDelay(RegenDuration/2);
            HPBars[i].DOColor(RegenColor, 0)
                .SetDelay(RegenDuration/2);

            HPBorders[i].transform.DOLocalMove(new Vector3(0,0,0), RegenDuration)
                .SetDelay(wait);
            wait += RegenDuration/2;
        }
        for (int i = 0; i < HPBorders.Count; i++)
        {
            HPBars[i].DOColor(HealthColor, RegenDuration)
                .SetDelay(wait);
        }
        
    }

    void ResetHPMasks()
    {
        for (int i = 0; i < HPBorders.Count; i++)
        {
            Mask maskMask = HPBorders[i].GetComponentInParent<Mask>();
            RawImage maskImage = maskMask.GetComponent<RawImage>();
            maskMask.enabled = false;
            maskImage.color = new Color(1, 1, 1, 0);
        }
    }

    #region Influence Gauge
    // Influence Position is designed to be at x 0 when at full stamina
    Vector3 influencePosition; // -238
    public void DrainStaminaVisual()
    {
        InfluenceBar.color = DrainColor;
        influencePosition = InfluenceRect.localPosition;
        influencePosition.x -= 238 / PlayerInfo.instance.MaxStamina * Time.unscaledDeltaTime * PlayerInfo.instance.StaminaRatio;
        InfluenceRect.localPosition = influencePosition;
    }
    public bool StaminaVisualWrong() => influencePosition.x > 0;
    public void RegenStaminaVisual()
    {
        
        InfluenceBar.color = InfluenceColor;
        influencePosition = InfluenceRect.localPosition;
        influencePosition.x += 238 / PlayerInfo.instance.MaxStamina * Time.unscaledDeltaTime * PlayerInfo.instance.StaminaRatio;
        InfluenceRect.localPosition = influencePosition;
        if (influencePosition.x > 0)
        {
            influencePosition.x = 0;
            InfluenceRect.localPosition = influencePosition;
        }
    }
    public void ResetStamina()
    {
        InfluenceBar.color = InfluenceColor;
        influencePosition.x = 0;
        InfluenceRect.localPosition = influencePosition;
    }
    #endregion
}
