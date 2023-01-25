//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DREditor.PlayerInfo;
using DREditor.Dialogues;
/// <summary>
/// The class that holds all data to be written to a save file
/// </summary>
[Serializable]
public class GameData
{
    public readonly static string CursiveWriter = "Thank you for being apart of the community that lifted me up when I needed it.";
    public BaseSave BaseData = null;
    public MainData MainData = null;
    public PlayerInfo.Data PlayerData = null;
    public string MusicData = "";
    public string EnvData = "";
    public RoomInstanceManager.InstanceData RoomData = null;
    public DialogueData DialogueData = null;
    public ProgressionData ProgressionData = null;
    public BacklogData BackData = null;
    public TrialData TrialData = null;
}
