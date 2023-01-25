//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;

public class SavePointUI : MonoBehaviour
{
    [SerializeField] bool debugMode = false;
    [SerializeField] bool moveTo = false;
    [SerializeField] string menuSceneName = "GYM_MainMenu";
    [SerializeField] PopUp group = null;
    [Header("Pair an objective with the name of a scene.")]
    ///<summary>
    /// Pair an Objective in the progression database with the scene it's supposed to go to!
    /// When the game loads or you finish saving, it'll load up the scene based on the players current objective!
    /// </summary>
    [SerializeField] SavePoint[] savePoints;

    private Dictionary<string, string> saves = new Dictionary<string, string>();

    #region Controls
    protected DRControls _controls;
    private void Awake()
    {
        GameManager.instance.cantBeInMenu = true;
        //if (GameManager.instance.currentMode != GameManager.Mode.Trial)
            //GameManager.instance.ChangeMode(GameManager.Mode.Trial);
        
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
    }
    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Enable();
#endif
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Disable();
#endif
    }
#endregion

    void Start()
    {
        if (savePoints != null)
        {
            foreach (SavePoint s in savePoints)
                saves.Add(s.objective, s.scene);
        }
        

        if (debugMode)
            StartSavePoint();

        if (!GameSaver.LoadingFile && group != null && !debugMode)
        {
            RoomLoader.PreEndLoad += StartSavePoint;
        }
        else if (moveTo)
        {
            RoomLoader.PreEndLoad += EvaluateSaveDirection;
        }
    }
    void StartSavePoint()
    {
        RoomLoader.PreEndLoad -= StartSavePoint;
        RoomLoader.PreEndLoad -= EvaluateSaveDirection;
        GlobalFade.instance.FadeOut(1);
        group.Reveal();
    }
    /* Idea: Add Implementation to instead of changing the backgroup of the save/load ui
     * see if we can add the back input to closing out and changing the scene.
     * 
     * Then when the game loads into a save point scene, it just auto loads the scene to go to.
     * (This is for a scenario where a save point isn't used for Trial Prep)
     * 
     * Have a progression list that the save point checks so that based on what your current progression is
     * loading into the scene, it can automatically route you to the correct scene.
     * (This is so you can have one save point scene for every save point we have in the game)
     * 
     */
    bool set = false;
    public void AddLeaveAbility(MenuGroup group) // Called from SaveLoad_Canvas's OnSave Event
    {
        if (!set)
        {
            set = true;
            UnityAction a = new UnityAction(group.Hide);
            UnityAction b = new UnityAction(EvaluateSaveDirection);
            group.OnBack.AddListener(a);
            group.OnBack.AddListener(b);
        }
        
    }
    public void EvaluateSaveDirection()
    {
        RoomLoader.PreEndLoad -= EvaluateSaveDirection;
        StartCoroutine(MoveToPoint());
    }
    IEnumerator MoveToPoint()
    {
        string scene = menuSceneName;
        
        try
        {
            scene = saves[ProgressionManager.instance.CurrentObjective.Description];
        }
        catch
        {
            Debug.LogWarning("NOTIFY: There was no scene for the current objective: " + ProgressionManager.instance.CurrentObjective.Description);
        }
        if (GlobalFade.instance != null)
        {
            GlobalFade.instance.FadeTo(0);
        }

        if (scene == menuSceneName && DialogueAssetReader.instance != null)
        {
            DialogueAssetReader.ReturnToMenu(scene);
        }
        else
        {
            EvaluateSavePointSettings();
            SceneManager.LoadSceneAsync(scene);
        }
        
        yield break;
    }
    public void EvaluateSavePointSettings()
    {
        SavePoint s;
        var x = savePoints.Where(n => n.objective == ProgressionManager.instance.CurrentObjective.Description);
        if (x.Count() > 0)
            s = x.ElementAt(0);
        else
            return;
        Debug.LogWarning("NOTIFY: SAVE POINT CHECK OBJECTIVE IS: " + ProgressionManager.instance.CheckObjective());
        if (s.changeObjective && ProgressionManager.instance.CheckObjective())
            ProgressionManager.instance.ChangeObjective();
        if (s.FadeOutOnLoad)
            RoomLoader.OnLoad += GlobalFade.instance.FadeOutOnLoad;
        Door.inLeaveProcess = true;
        GameManager.instance.cantBeInMenu = false;
    }
    [Serializable]
    public struct SavePoint
    {
        public string objective;
        public string scene;
        public bool changeObjective;
        public bool FadeOutOnLoad;
    }
}
