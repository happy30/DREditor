//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
/// <summary>
/// Script that holds DifficultyOptions, for setting up the difficulty settings
/// </summary>
public class DifficultyGroup : MonoBehaviour
{
    //PlayerInput Input => GameManager.instance.GetInput();

    [SerializeField] Image cover = null;
    [SerializeField] Color activeColor;
    [SerializeField] Color inactiveColor;
    [SerializeField] Image holder = null;
    //[SerializeField] Sprite active = null;
    //[SerializeField] Sprite inactive = null;
    [SerializeField] DifficultyGroup nextGroup = null;
    [SerializeField] bool navUp = false;
    [SerializeField] bool navDown = false;
    [SerializeField] List<DifficultyOption> options = new List<DifficultyOption>();
    [Header("If not Logic then is Action")]
    [SerializeField] bool isLogic = true;
    [SerializeField] GameManager.Difficulty difficulty;
    [HideInInspector] public DifficultyOption lastOption;
    [Tooltip("If true then load PlayerInfo settengs")]
    [SerializeField] bool loadCurrent = false;
    public void SetupGroup()
    {
        UIHandler.instance.OnChange.AddListener(Evaluate);
        if(lastOption == null)
        {
            if (loadCurrent)
            {
                GameManager.Difficulty diff = isLogic ? GameManager.instance.logicDifficulty : GameManager.instance.actionDifficulty;
                lastOption = GetOptionFromDifficulty(diff);
                // Set difficulty to diff
                difficulty = diff;
                nextGroup.ChangeGroupNav(lastOption);
            }
            else
            {
                lastOption = options[0];
            }
            lastOption.KeepImage(false);
        }
    }
    DifficultyOption GetOptionFromDifficulty(GameManager.Difficulty diff)
    {
        foreach(DifficultyOption o in options)
            if(o.difficulty == diff)
                return o;
        Debug.LogWarning("Couldn't find option from Difficulty!");
        return options[0];
    }
    public void StartGroup()
    {
        cover.color = activeColor;
        if(lastOption != null)
        {
            EventSystem.current.SetSelectedGameObject(lastOption.gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(options[0].gameObject);
        }
    }
    public void UnSetupGroup()
    {
        cover.color = inactiveColor;
        UIHandler.instance.OnChange.RemoveListener(Evaluate);
        lastOption.KeepImage(false);
        SetGameDifficulty();
    }
    public bool Check()
    {
        GameObject g = EventSystem.current.currentSelectedGameObject;
        if (g == null)
        {
            Debug.LogWarning("The Current Selected object was null");
            return false;
        }
        foreach (DifficultyOption o in options)
        {
            if (g == o.gameObject)
            {
                lastOption = o;
                nextGroup.ChangeGroupNav(lastOption);
                return true;
            }
        }
        return false;
    }
    void Evaluate()
    {
        if (Check())
        {
            //ChangeSprite(active);
            cover.color = activeColor;
        }
        else
        {
            //ChangeSprite(inactive);
            cover.color = inactiveColor;
            lastOption.KeepImage(false);
        }
    }
    void ChangeSprite(Sprite sprite)
    {
        if (holder.sprite != sprite)
            holder.sprite = sprite;
    }
    public void SetGroupDifficulty(DifficultyOption op)
    {
        difficulty = op.difficulty;
        SetGameDifficulty();
    }
    public void SetGameDifficulty()
    {
        if (isLogic)
        {
            GameManager.instance.logicDifficulty = difficulty;
        }
        else
        {
            GameManager.instance.actionDifficulty = difficulty;
        }
    }
    public void ChangedDiffGroup(DifficultyOption last)
    {
        last.KeepImage(Check());
    }
    public void ChangeGroupNav(DifficultyOption o)
    {
        if (navUp)
        {
            foreach(DifficultyOption option in options)
            {
                UIHelper.SelectOnUp(option, o);
            }
        }
        if (navDown)
        {
            foreach (DifficultyOption option in options)
            {
                UIHelper.SelectOnDown(option, o);
            }
        }
    }
}
