//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues;
using DREditor.Gates;
using DREditor.Progression;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager instance = null;
    [Header("Required to be filled out to work")]
    public Dialogue LeaveAsk = null;
    [SerializeField] ProgressionDatabase Database = null;
    [SerializeField] Chapter CurrentChapter = null;

    [Header("Debug Starting Progression Values")]
    [SerializeField] bool TestLatestObjective = false;
    [SerializeField] int StartObjective = 0;
    [SerializeField] int ClearedFlags = 0;
    [SerializeField] bool GeneratePastData = false;

    [Header("Debug Viewing Values")]
    public Objective CurrentObjective = null;
    //public Dialogue GlobalLock = null;
    //public Dialogue LockedDialogue = null;
    public ProgressionData Data = new ProgressionData(); // Finished Objectives the players done

    public delegate void ChapDel(int i);
    public static event ChapDel OnChapterChange;

    public delegate void FlagDel(string name);
    public static event FlagDel FlagTriggered;

    public Chapter GetChapter() => CurrentChapter;
    public int GetChapterNum()
    {
        for (int i = 0; i < Database.Chapters.Count; i++)
            if (CurrentChapter == Database.Chapters[i])
                return i;
        return 0;
    }
    //GateDatabase ADB => (GateDatabase)Resources.Load("Gates/GateDatabase");
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(this);

        //SetupDatabase();

        /*if (CurrentChapter != null)
        {
            for(int i = 0; i < Database.Chapters.Count; i++)
            {
                Chapter c = Database.Chapters[i];
                Debug.Log(c.name);
                Debug.Log(CurrentChapter.name);
                if(c.name.Contains(CurrentChapter.name))
                {
                    CurrentChapter = c;
                }
            }
        }*/

        
    }
    private void Start()
    {
        ClearChapter(CurrentChapter);
        GameManager.StartedNewGame += Data.Clear;
        StartObjective = TestLatestObjective ? CurrentChapter.Objectives.Count - 1 : StartObjective;
        if (CurrentChapter.Objectives.Count > 0)
            CurrentObjective = (Objective)CurrentChapter.Objectives[StartObjective].Clone();

        if (CurrentObjective != null && ClearedFlags > 0 && ClearedFlags <= CurrentObjective.Flags.Count)
            for (int i = 0; i < ClearedFlags; i++)
                CurrentObjective.Flags[i].triggered = true;
        Debug.LogWarning("NOTIFY: The current Game Objective is: " + CurrentObjective.Description);

        if (GeneratePastData)
            GeneratePast();
    }
    public void ResetProgression()
    {
        CurrentChapter = Database.Chapters[0];
        CurrentObjective = (Objective)CurrentChapter.Objectives[0].Clone();
        Data.Clear();
    }
    //public void ClearLockedDialogue() => LockedDialogue = null;
    
    Objective GetBaseObjective(string s)
    {
        foreach(Objective o in CurrentChapter.Objectives)
        {
            if(o.Description == s)
            {
                return o;
            }
        }
        Debug.LogWarning("GetOFromC Couldn't find the objective from given name");
        return null;
    }
    void ClearChapter(Chapter c)
    {
        Debug.Log("Clear Chapter Called");
        foreach(Objective o in c.Objectives)
        {
            foreach(ProgressionFlag flag in o.Flags)
            {
                flag.triggered = false;
            }
        }
    }
    public void NextChapter()
    {
        for(int i = 0; i < Database.Chapters.Count; i++)
        {
            Chapter c = Database.Chapters[i];
            if (c == CurrentChapter && (i + 1) < Database.Chapters.Count)
            {
                CurrentChapter = Database.Chapters[i + 1];
            }
        }
        OnChapterChange.Invoke(Database.GetChapterIndex(CurrentChapter));
    }
    Objective GetNextObjective(Objective o)
    {
        for(int i = 0; i < CurrentChapter.Objectives.Count; i++)
        {
            Objective ob = CurrentChapter.Objectives[i];
            if (ob.Description == o.Description)
            {
                if (i + 1 < CurrentChapter.Objectives.Count)
                {
                    return CurrentChapter.Objectives[i + 1];
                }
                else
                {
                    NextChapter();
                    return CurrentChapter.Objectives[0];
                }
            }
        }
        Debug.Log("GetNextObjective Evaluated Null");
        return null;
    }
    
    /// <summary>
    /// Checks whether given progression flag is triggered
    /// </summary>
    /// <param name="flagName"></param>
    /// <returns></returns>
    public bool CheckFlag(string flagName)
    {
        ProgressionFlag flag = GetFlag(flagName);
        return flag.triggered;
    }

    public bool CheckObjective()
    {
        int check = 0;
        foreach(ProgressionFlag flag in CurrentObjective.Flags)
        {
            if (flag.triggered && !flag.optional)
                check++;
        }

        return check == CurrentObjective.GetRequiredFlagCount();

    }
    public void ChangeObjective()
    {
        Debug.LogWarning("Changing Objective");
        Data.Add(GetChapterNum(), CurrentObjective);
        Debug.LogWarning(CurrentObjective.Description);
        CurrentObjective = (Objective)GetNextObjective(CurrentObjective).Clone();
    }
    public ProgressionData Save()
    {
        Data.chapter = GetChapterNum();
        return Data;
    }
    /// <summary>
    /// Sets Progression Data from the loaded file
    /// </summary>
    public void Load()
    {
        CurrentChapter = Database.Chapters[GameSaver.CurrentGameData.ProgressionData.chapter];
        CurrentObjective = GameSaver.CurrentGameData.RoomData.CurrentObjective;
        Data = GameSaver.CurrentGameData.ProgressionData;
        Objective baseOb = GetBaseObjective(CurrentObjective.Description);
        if (baseOb.ProgressionGate != null)
            ApplyObjectiveSettings();
    }
    public void ApplyObjectiveSettings()
    {
        Debug.LogWarning("Applied Objective Settings");
        Objective baseOb = GetBaseObjective(CurrentObjective.Description);
        CurrentObjective.ProgressionGate = baseOb.ProgressionGate;
    }
    public void TriggerFlag(int chapter, int objective, List<int> flags)
    {
        int lastNum = -1;
        try
        {
            //Chapter c = Database.Chapters[chapter];
            if (chapter == GetChapterNum() && 
                Database.Chapters[chapter].Objectives[objective].Description == CurrentObjective.Description)
            {
                Objective o = CurrentObjective;
                for (int i = 0; i < flags.Count; i++)
                {
                    lastNum = flags[i];
                    if (!(flags[i] < o.Flags.Count))
                    {
                        RepairCurrentObjective();
                        o = CurrentObjective;
                    }
                    o.Flags[flags[i]].triggered = true;
                }
            }
            else
            {
                for(int i = 0; i < flags.Count; i++)
                {
                    GetFlag(Database.Chapters[chapter].Objectives[objective].Flags[flags[i]].name).triggered = true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("A CRITICAL ERROR HAS OCCURED TRIGGERING PROGRESSION FLAG " +
                "\n PLEASE CHECK THE DIALOGUE ASSET TO FIX OR FIND THE ISSUE.");
            Debug.LogWarning(e);
            Debug.LogWarning(lastNum);
        }
    }
    ProgressionFlag GetFlag(string flagName)
    {
        ProgressionFlag flagOne = SearchCurrentObjective(flagName);

        if (flagOne != null)
            return flagOne;

        Debug.LogWarning("Flag: " + flagName + " could not be found on Objective: " + CurrentObjective.Description + " " +
            "Make sure you spell the flag name correctly!");

        // Below Searches the previous Objectives the player has gone through that contain the flag 
        // if the flag isn't within the players current objective.
        foreach(ProgressionInfo i in Data.Info)
        {
            for(int j = 0; j < i.objectives.Count; j++)
            {
                Objective o = i.objectives[j];
                Objective b = GetBaseObjective(o.Description);
                if (b.Flags.Count != o.Flags.Count)
                {
                    i.objectives[j] = (Objective)b.Clone();

                    for (int x = 0; x < o.Flags.Count; x++)
                    {
                        ProgressionFlag copy = o.Flags[x];
                        foreach (ProgressionFlag f in i.objectives[j].Flags)
                            if (f.name == copy.name && copy.triggered)
                                f.triggered = true;
                    }
                    
                }
            }
        }
        IEnumerable<ProgressionInfo> p = 
            Data.Info.Where(n => n.objectives.Where(j => j.GetFlagNames().Contains(flagName)).Count() > 0);
        Debug.Log(p.Count());

        if (p.Count() > 0)
        {
            Debug.LogWarning("Found the chapter");
            var o =
                p.ElementAt(0).objectives.Where(n => n.GetFlagNames().Contains(flagName)).ElementAt(0);
            var f = o.Flags.Where(n => n.name == flagName);
            
            if (f.Count() > 0 && f.ElementAt(0) != null)
                return f.ElementAt(0);

        }
        
        Debug.LogWarning("After Checking this save files previously completed Objectives, that flag couldn't be found. \n" +
            "Which means one has occured: There's been user error building progression, " +
            "The Searched progression flag string used was misspelt, OR the save file never completed or had the objective " +
            "that contained the searched flag. \n" +
            "If you still encounter this error after making sure Debug Progression data contains the flag, and the searched flag " +
            "was not misspelt, you should contact Sweden in order to debug and find the problem.");

        // Repairing code below
        Debug.LogWarning("Repairing Objective");
        RepairCurrentObjective();

        ProgressionFlag flagTwo = SearchCurrentObjective(flagName);

        if (flagTwo != null)
            return flagTwo;

        Debug.LogError("After repairing the current save file objective, the current " +
            "flag couldn't be found. Best to contact Sweden for support.");
        
        return null;
    }
    void RepairCurrentObjective()
    {
        Objective currentCopy = (Objective)CurrentObjective.Clone();
        CurrentObjective = (Objective)GetBaseObjective(CurrentObjective.Description).Clone();

        for (int i = 0; i < currentCopy.Flags.Count; i++)
        {
            ProgressionFlag copy = currentCopy.Flags[i];
            foreach (ProgressionFlag f in CurrentObjective.Flags)
                if (f.name == copy.name && copy.triggered)
                    f.triggered = true;
        }
    }
    ProgressionFlag SearchCurrentObjective(string flagName)
    {
        foreach (ProgressionFlag flag in CurrentObjective.Flags)
        {
            //Debug.LogWarning(flag.name + " " + flagName + " " + (flag.name == flagName));
            if (flag.name == flagName)
            {
                return flag;
            }
        }
        
        return null;
    }
    public void TriggerFlag(string flagName)
    {
        ProgressionFlag flag = GetFlag(flagName);
        flag.triggered = true;
        FlagTriggered?.Invoke(flag.name);
    }
    public bool EvaluateVariable(int chapter, int objective, List<int> flags)
    {
        try
        {
            //Chapter c = Database.Chapters[chapter];
            Objective o = CurrentObjective;
            for (int i = 0; i < flags.Count; i++)
            {
                if (!o.Flags[flags[i]].triggered)
                {
                    return false;
                }
            }
            return true;
        }
        catch(Exception e)
        {
            Debug.LogWarning("A CRITICAL ERROR HAS OCCURED EVALUATING DIALOGUE VARIABLE EVALUATION " +
                "\n PLEASE ENSURE YOU FILL IT OUT CORRECTLY.");
            Debug.LogWarning(e.ToString());
            return false;
        }
    }

    void GeneratePast()
    {
        int chap = GetChapterNum();
        foreach (Objective o in Database.Chapters[chap].Objectives)
        {
            if (o.Description == CurrentObjective.Description)
                break;
            Objective ob = (Objective)o.Clone();
            /*for (int i = 0; i < ob.Flags.Count; i++)
            {
                ProgressionFlag flag = ob.Flags[i];
                ProgressionFlag dupe = (ProgressionFlag)flag.Clone();
                if (!dupe.optional)
                    dupe.triggered = true;
                ob.Flags[i] = dupe;
                
            }*/
           foreach (ProgressionFlag flag in ob.Flags)
                if (!flag.optional)
                    flag.triggered = true;
            Data.Add(chap, ob);
        }
    }

    public string GetChapterSaveTitle(int i)
    {
        if (Database.Chapters.Count == 0)
            return "";
        return Database.Chapters[i].saveTitle;
    }
}
/// <remarks>
/// The reason why this data is structured this way was for it to be flexible if something needed to be added
/// but also not a huge pain to sift through.
/// </remarks>
/// <summary>
/// Found in ProgressionManager: Save data for past progression.
/// </summary>
[System.Serializable]
public class ProgressionData
{
    public int chapter;
    public List<ProgressionInfo> Info = new List<ProgressionInfo>();
    public void Add(int i, Objective o) // i = chapter
    {
        if (i >= Info.Count)
            Info.Add(new ProgressionInfo());
        Info[i].Add(o);
    }
    public void Clear() => Info.Clear();
}
/// <summary>
/// Found in ProgressionManager: Past Objective Info of a chapter
/// </summary>
[System.Serializable]
public class ProgressionInfo
{
    public List<Objective> objectives = new List<Objective>();
    public void Add(Objective o) => objectives.Add(o);
}
