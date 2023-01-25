//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using DREditor.EventObjects;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    // Something to Identify a location? ooga booga?
    PlayerInput playerInput;
    public int currentChapter = 0; // This can be obtained in the progression manager so might get rid of this
    public State currentState = State.Daily;
    public Mode currentMode = Mode.ThreeD;
    public Difficulty logicDifficulty = Difficulty.Kind;
    public Difficulty actionDifficulty = Difficulty.Kind;
    public AddState addState = AddState.None;
    string location;
    public string GetLocation() => location;
    public void SetLocation(string t) => location = t;

    public bool FTEMode = false;
    public bool TrialMode = false;

    public delegate void ModeChanger(Mode mode);
    public static event ModeChanger ModeChange;

    public delegate void StateChanger(State state);
    public static event StateChanger StateChange;

    public delegate void VoidDel();
    public static event VoidDel StartedNewGame;

    public delegate void StringDel(string key);
    public static event StringDel OnSetMarker;
    public static void SetControls(string s) => OnSetMarker?.Invoke(s);

    public delegate void BoolDel(bool value);
    public static event BoolDel OnCallControls;
    public static void CallControls(bool value) => OnCallControls?.Invoke(value);

    public BoolWithEvent InDialogue = null;
    public BoolWithEvent InTPFD = null;
    public BoolWithEvent InMenu = null;
    public bool cantBeInMenu = false;

    [Header("Debug Options")]
    public bool DebugSaveFiles = false;

    public PlayerInput GetInput() => playerInput;
    public enum State
    {
        Daily, Night, Deadly
    }

    public enum Mode
    {
        ThreeD, TPFD, Trial
    }
    public enum Difficulty
    {
        Kind, Normal, Mean
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        playerInput = (PlayerInput)FindObjectOfType(typeof(PlayerInput));
    }
    private void Start()
    {
        ModeChange += CheckTPFDChange;
        ChangeMode(currentMode);
        playerInput.actions.FindActionMap("Player").Enable();
        playerInput.actions.FindActionMap("UI").Enable();
        playerInput.actions.FindActionMap("Minigame").Enable();
    }
    public void ChangeMode(Mode mode)
    {
        
        currentMode = mode;
        try
        {
            ModeChange.Invoke(mode);
            Debug.Log("Mode Changed");
        }
        catch
        {
            Debug.LogWarning("There was an error trying to change the Mode");
        }
    }
    public void ChangeState(State state)
    {
        currentState = state;
        try
        {
            StateChange?.Invoke(state);
            Debug.Log("State Changed");
        }
        catch
        {
            Debug.LogWarning("There was an error trying to change the State");
        }
    }
    void CheckTPFDChange(Mode mode)
    {
        if (mode == Mode.TPFD)
            InTPFD.Value = true;
        else
            InTPFD.Value = false;
        Debug.LogWarning("Setting TPFD to: " + InTPFD.Value);
    }
    
    public bool CheckState(State state)
    {
        return currentState == state;
    }

    public static void StartNewGameFile()
    {
        instance.ChangeState(State.Daily);
        instance.currentMode = Mode.TPFD;
        instance.addState = AddState.None;
        instance.location = "";
        StartedNewGame?.Invoke();
    }

    public static MainData Save()
    {
        MainData data = new MainData();
        data.state = instance.currentState;
        data.mode = instance.currentMode;
        data.logic = instance.logicDifficulty;
        data.action = instance.actionDifficulty;
        data.addedState = instance.addState;
        data.location = instance.location;
        return data;
    }
    public static void Load(MainData data)
    {
        instance.ChangeState(data.state);
        instance.currentMode = data.mode;
        instance.logicDifficulty = data.logic;
        instance.actionDifficulty = data.action;
        instance.addState = data.addedState;
        instance.location = data.location;
    }
    
}

[Serializable]
public class MainData
{
    public GameManager.State state;
    public GameManager.Mode mode;
    public GameManager.Difficulty logic;
    public GameManager.Difficulty action;
    public AddState addedState;
    public string location;
}
[Serializable]
public enum AddState
{
    None, Investigation, Preparation, Conclusion, END
}
