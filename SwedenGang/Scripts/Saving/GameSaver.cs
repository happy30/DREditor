//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DREditor.PlayerInfo;
using DREditor.Dialogues;
/// <summary>
/// Houses Central Mechanisims for loading the game.
/// Use SaveLoadMenu.cs should call this for saving and loading slots
/// </summary>
public class GameSaver : MonoBehaviour
{
    public static GameData CurrentGameData = null;
    public static bool LoadingFile = false;
    public static Dialogue LoadDialogue = null;
    public static Actor LoadActor = null;
    public static TrialDialogue LoadTrialDialogue = null;
    public static bool FirstTimeLoaded = false; // So the Main Menu knows how to load
    public static bool StartedNewWithLoaded = false;
    public static void SaveGameFile(string slotNum) // Builds the Gamedata class to then be written to a file/Encrypted
    {
        // Make a new GameData
        GameData data = new GameData();
        // Call from managers to assign the data for the games data
        data.BaseData = new BaseSave();
        data.MainData = GameManager.Save();
        data.PlayerData = PlayerInfo.instance.Save();
        data.RoomData = RoomInstanceManager.instance.SaveRoomProgress();
        // Keep in mind that the CurrentObjective is actually stored in the room data

        data.ProgressionData = ProgressionManager.instance.Save();

        data.DialogueData = new DialogueData();
        if (GameManager.instance.currentMode == GameManager.Mode.Trial && GameManager.instance.InDialogue.Value)
        {
            data.TrialData = TrialLoader.Save();
            data.DialogueData.DialogueName = TrialDialogueManager.SaveDialogueName;
            data.DialogueData.DirectToNum = TrialDialogueManager.SaveDirectToNum;
            data.DialogueData.Conditional = TrialDialogueManager.SaveCondition;
            data.DialogueData.LineNum = TrialDialogueManager.SaveLineNum;
            data.DialogueData.TransitionNum = TrialDialogueManager.SaveTransitionNum;
            //data.BackData = TrialDialogueManager.Backlog.Save();
            data.BackData = DialogueAssetReader.Backlog.Save();
        }
        else
        {
            data.DialogueData.DialogueName = DialogueAssetReader.SaveDialogueName;
            data.DialogueData.DirectToNum = DialogueAssetReader.SaveDirectToNum;
            data.DialogueData.Conditional = DialogueAssetReader.SaveCondition;
            data.DialogueData.LineNum = DialogueAssetReader.SaveLineNum;
            data.DialogueData.TransitionNum = DialogueAssetReader.SaveTransitionNum;
            data.BackData = DialogueAssetReader.Backlog.Save();
        }

        data.MusicData = SoundManager.instance.SaveMusic();
        data.EnvData = SoundManager.instance.SaveEnv();

        CurrentGameData = data;

        // Write to the file
        if (GameManager.instance.DebugSaveFiles)
        {
            SaveSystem.SaveToJSON(data, slotNum);
        }
        else
        {
            SaveSystem.SaveEncrypt(data, slotNum);
        }
        // Game is saved
    }
    public static void LoadGameFile(string slotNum)
    {
        Debug.Log("File Data is on static variable");
        CurrentGameData = GetData(slotNum);
        //Debug.Log(CurrentGameData.DialogueData.DialogueName);
    }
    public static GameData GetData(string slotNum)
    {
        try
        {
            return SaveSystem.LoadEncrypt<GameData>(slotNum);
        }
        catch
        {
            return SaveSystem.ReadFromJSON<GameData>(slotNum);
        }
    }
    
    public static void ApplyCurrentData()
    {
        ApplyMainData();
        PlayerInfo.instance.Load(CurrentGameData.PlayerData);
        OptionsMenu.CallUpdateSettings();
        RoomInstanceManager.instance.data = CurrentGameData.RoomData;
    }
    /// <summary>
    /// Applies the GameManagers MainData from the currently loaded save file data
    /// </summary>
    public static void ApplyMainData()
    {
        GameManager.Load(CurrentGameData.MainData);
    }
}

