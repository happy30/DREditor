//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using DREditor.Gates;
using DREditor.PlayerInfo;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string GoToScene = "";
    [SerializeField] string mainUIObject = "Main UI";
    [SerializeField] string creditsScene = "GYM_Credits";

    //[SerializeField] Image introBlack = null;
    [SerializeField] Animator LogoAnim = null;
    [SerializeField] GameObject first = null;
    [SerializeField] MenuGroup firstGroup = null;
    [SerializeField] AudioClip Music = null;
    [SerializeField] AudioClip continueGame = null;
    
    [HideInInspector] public GameObject current;

    [SerializeField] int SaveSlotNums = 31;

    [SerializeField] MenuGroup LoadGroup = null;
    [SerializeField] Selectable Continue = null;

    [Header("Options")]
    [SerializeField] Selectable NewGame = null;
    [SerializeField] Selectable LoadGame = null;
    [SerializeField] Selectable QuitGame = null;
    //[SerializeField] Selectable NewGameT = null;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        GameObject o = GameObject.Find(mainUIObject);
        if(o)
            o.GetComponent<Animator>().Play("HideMainUI");

    }
    private void Start()
    {
        
        GameManager.instance.cantBeInMenu = true;
        UIHandler.ReachedTitleScreen();
        if (!GameSaver.FirstTimeLoaded && GameSaver.CurrentGameData == null) // Not loading from an already started session of the game
        {
            Debug.Log("Calling Intro Anim");
            GameSaver.FirstTimeLoaded = true;
            StartCoroutine(IntroAnim());
        }
        else if (GameSaver.StartedNewWithLoaded)
        {
            GameSaver.StartedNewWithLoaded = false;
            GameSaver.ApplyCurrentData();
            LoadedFile();
        }
        else if (GameSaver.CurrentGameData != null)
        {
            Debug.Log("Called LoadedFile()");
            LoadedFile();
        }
        else
        {
            Debug.Log("MainMenu Start: No CurrentGameData");
            GameSaver.FirstTimeLoaded = true;
            StartCoroutine(IntroAnim());
        }
    }
    
    IEnumerator IntroAnim()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1;
        
        
        //yield return new WaitForSeconds(1);
        Debug.Log("Animator triggers called");
        if(LogoAnim)
            LogoAnim.SetTrigger("LogoBG");
        
        SoundManager.instance.ClearText();
        yield return new WaitForSeconds(1f);
        if (GlobalFade.instance.IsDark)
            GlobalFade.instance.FadeOut(1);
        SoundManager.instance.PlayMusic(Music, true);
        yield return new WaitForSeconds(1f);
        firstGroup.Reveal();
        EventSystem.current.SetSelectedGameObject(first);
        UIHandler.instance.current = first;
        yield break;
    }
    public void CheckIfLoad() // When loading file from Main menu set first to continue
    {
        if (StartedOnNewGame)
        {
            GameSaver.CurrentGameData = null;
            return;
        }
        if (GameSaver.CurrentGameData != null && !StartedOnNewGame)
        {
            EventSystem.current.SetSelectedGameObject(Continue.gameObject);
        }
    }
    public void ContinueEnable() => Continue.interactable = true;
    public void ContinueDisable() => Continue.interactable = false;
    bool StartedOnNewGame = false;
    public void StartedOnNew() // Called on click for New Game option for the title group
    {
        if (GameSaver.CurrentGameData != null)
        {
            GameSaver.CurrentGameData = null;
            PlayerInfo.instance.Info.Clear();
        }
        StartedOnNewGame = true;
        GameManager.StartNewGameFile();
        //if(NewGameT != null)
            //SelectMenuItem(NewGameT.gameObject);
    }
    public void CheckForLoadFiles()
    {
        if (LoadGameCheck() && !StartedOnNewGame)
        {
            EventSystem.current.SetSelectedGameObject(LoadGame.gameObject);
        }
        else
            StartedOnNewGame = false;
    }
    void LoadedFile() // Sets up the Main Menu when loading a file
    {
        /* 
         * The MenuGroup (LoadGroup) 's start function turns off all buttons until the 
         * group is revealed. 
         * 
         * But because this function can be called during Start() 
         * I have to make it a coroutine and wait for a frame so that the menu group will
         * reveal correctly.
         */
        StartCoroutine(LoadGroupStart());
    }
    IEnumerator LoadGroupStart()
    {
        Time.timeScale = 1;
        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        if (GlobalFade.instance.isTransitioning)
            yield return new WaitUntil(() => !GlobalFade.instance.isTransitioning);
        GlobalFade.instance.FadeOut(1);
        if (LogoAnim)
            LogoAnim.SetTrigger("LogoBG");

        LoadGroup.Reveal();

        if (GameSaver.CurrentGameData != null)
        {
            Debug.Log("Has GameSaver.CurrentGameData");
            GameSaver.ApplyCurrentData();
            if (!Continue.enabled)
                Continue.enabled = true;
            Continue.interactable = true;
            EventSystem.current.SetSelectedGameObject(Continue.gameObject);
            UIHandler.instance.current = Continue.gameObject;

        }
        else
        {
            Debug.Log("GameData Not found :(");
            DialogueAnimConfig.instance.DisableVisuals();
        }
        SoundManager.instance.PlayMusic(Music, true);
        yield break;
    }
    bool continuing = false;
    public void ContinueGame() // On confirm of Do you want to Continue? Pop Up
    {
        if (continuing || MenuGroup.Changing)
            return;
        continuing = true;
        StartCoroutine(EndPain());
        EventSystem.current.SetSelectedGameObject(null);
        GameSaver.ApplyMainData();
        StartCoroutine(FadeIntoLoadedGame());
        SoundManager.instance.PlaySFX(continueGame);
        
        // Fade into game
        // Load the Room of RoomInstanceManager.data.CurrentRoom.name
    }
    IEnumerator EndPain()
    {
        while (continuing)
        {
            EventSystem.current.SetSelectedGameObject(null);
            yield return null;
        }
        yield break;
    }
    IEnumerator FadeIntoLoadedGame() // For Continue Scenario
    {
        GameSaver.LoadingFile = true;
        RoomLoader.instance.RoomsCanLoad();
        if (GlobalFade.instance.isTransitioning)
            yield return new WaitUntil(() => !GlobalFade.instance.isTransitioning);
        GlobalFade.instance.FadeTo(1.5f);
        SoundManager.instance.FadeMusic(2f);
        yield return new WaitForSeconds(2.5f);
        GateDatabase ADB = (GateDatabase)Resources.Load("Gates/GateDatabase");

        Debug.Log(GameSaver.CurrentGameData.RoomData.CurrentRoom.Name);
        RoomInstanceManager.instance.data = GameSaver.CurrentGameData.RoomData;
        Debug.Log(RoomInstanceManager.instance.data.CurrentRoom.Name);

        RoomLoader.instance.SetCurrentRoomData(GameSaver.CurrentGameData.RoomData, ADB.GetArea(RoomInstanceManager.instance.data.CurrentRoom.Name));
        SceneManager.LoadScene(RoomInstanceManager.instance.data.CurrentRoom.Name);
        
        yield break;
    }
    IEnumerator FadeIntoGame() // For New Game Scenario
    {
        GameManager.StartNewGameFile();
        ProgressionManager.instance.ResetProgression();
        RoomInstanceManager.instance.ClearData();
        DialogueAssetReader.Backlog.Lines.Clear();
        RoomInstanceManager.instance.ClearRoomData();
        GlobalFade.instance.FadeTo(1);
        SoundManager.instance.FadeMusic(1f);
        yield return new WaitForSeconds(1f);
        RoomLoader.instance.RoomsCanLoad();
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GoToScene);
        Debug.Log(asyncOperation.priority);
        
        yield break;
    }
    public void StartNewGame()
    {
        RaycastReticle.canSelectOverride = false;
        PlayerInfo.instance.Info.Clear();
        if (GameSaver.CurrentGameData != null)
        {
            GameSaver.StartedNewWithLoaded = true;
            GameManager.StartNewGameFile();
        }
        EventSystem.current.SetSelectedGameObject(null);
        SoundManager.instance.PlaySFX(continueGame);
        StartCoroutine(FadeIntoGame());
    }
    public void SelectMenuItem(GameObject to)
    {
        EventSystem.current.SetSelectedGameObject(to);
    }
    public void PlayMenuSFX(AudioClip clip) => SoundManager.instance.PlaySFX(clip);
    public void QuitConfirm()
    {
        Application.Quit();
    }

    public void CheckForLoadGame(MenuGroup group) // Referenced in Event in Inspector
    {
        if (LoadGameCheck())
            EnableLoadOption(group);
        else
        {
            LoadGame.interactable = false;
            group.first = NewGame.gameObject;
            group.SetLastSelected(null);
            //SelectMenuItem(NewGame.gameObject);
        }
    }
    bool LoadGameCheck()
    {
        for(int i = 0; i < SaveSlotNums; i++)
        {
            if (SaveSystem.CheckSave("save" + i))
                return true;
        }
        return false;
    }
    void EnableLoadOption(MenuGroup group)
    {
        group.first = LoadGame.gameObject;
        UIHelper.SelectOnDown(NewGame, LoadGame);

        UIHelper.SelectOnUp(QuitGame, LoadGame);
        LoadGame.interactable = true;
        group.SetLastSelected(null);
    }
    public void ToCredits() => StartCoroutine(ToCreditsRoutine());
    IEnumerator ToCreditsRoutine()
    {
        GlobalFade.instance.FadeTo(0.5f);
        yield return new WaitForSeconds(0.5f);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(creditsScene);
        yield break;
    }
}
