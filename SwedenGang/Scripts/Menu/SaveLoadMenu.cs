//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Requires: MenuGroup & BaseSave
/// Component Attached to the canvas of the Save Menu
/// </summary>
public class SaveLoadMenu : MonoBehaviour
{
    /*
     * For Save button: Change Group, SaveLoadMenu.SetSaveMode, LoadSlots
     * For Load button: Change Group, SaveLoadMenu.SetLoadMode, LoadSlots
     */
    [SerializeField] bool OnPauseMenu = true;
    //[SerializeField] bool OnSavePoint = false;
    //[SerializeField] GameObject SlotsHolder = null;
    [SerializeField] string TitleScreenSceneName = "TitleScreen";
    [SerializeField] List<SaveSlot> SlotList = new List<SaveSlot>();
    [SerializeField] PopUp SavePopUp = null;
    [SerializeField] PopUp OverwritePopUp = null;
    [SerializeField] PopUp LoadPopUp = null;
    [SerializeField] PopUp ReturnPopUp = null;
    SaveSlot currentSlot = null;
    bool SaveMode = false;
    public UnityEvent OnSave;
    public void SetSaveMode() => SaveMode = true;
    public void SetLoadMode() => SaveMode = false;
    public void LoadSlots()
    {
        for(int i = 0; i < SlotList.Count; i++) // Set Slot Data
        {
            
            SaveSlot s = SlotList[i];
            if (SaveSystem.CheckSave("save" + i))
                FoundSave(s, i);
            else
                NoSave(s, i);

        }
        for (int i = 0; i < SlotList.Count; i++) // Set Navigation
        {

            SaveSlot s = SlotList[i];
            if (s.Select.navigation.selectOnDown != null)
                break;

            if (i == 0)
                SelectOnDown(s.Select, SlotList[i + 1].Select);
            else if (i == SlotList.Count - 1)
                SelectOnUp(s.Select, SlotList[i - 1].Select);
            else
                SelectUpDown(s.Select, SlotList[i - 1].Select, SlotList[i + 1].Select);
        }
        
    }

    void FoundSave(SaveSlot slot, int i)
    {
        // Display the Title, Date, and Time of save
        slot.HasData = true;
        try
        {
            GameData s = GameSaver.GetData("save" + i);
            slot.Title.text = "Slot " + (i + 1) + " : " + EvaluateSaveTitle(s);
            slot.Date.text = s.BaseData.Date.Month + "/" + s.BaseData.Date.Day + "/" + s.BaseData.Date.Year;
            slot.TimeOfSave.text = s.BaseData.Date.Hour + ":" + s.BaseData.Date.Minute + ":" + s.BaseData.Date.Second;
        }
        catch
        {
            Debug.LogError("Slot BaseSave data could not be read and written to the GUI");
        }
        
    }
    string EvaluateSaveTitle(GameData s)
    {
        string title;
        string chap = "";
        string area = s.BaseData.SceneName.Replace("GYM_", "");
        string life = "";
        string added = "";

        if (s.MainData.location != null && s.MainData.location != "")
        {
            area = s.MainData.location;
        }
        if (ProgressionManager.instance != null)
        {
            chap = ProgressionManager.instance.GetChapterSaveTitle(s.ProgressionData.chapter);
        }
        if(s.MainData.addedState != AddState.None)
        {
            added = "(" + s.MainData.addedState.ToString() + ")";
        }
        // if or when you do life, make sure during it's evaulation the result adds a space " "
        title = chap + " " + life + area + " " + added;
        return title;
    }
    void NoSave(SaveSlot slot, int i)
    {
        // Add On click error noise
        slot.Title.text = "Free Slot " + (i+1);
        slot.Date.text = "";
        slot.TimeOfSave.text = "";
    }
    public void SetCurrentSave(SaveSlot slot) => currentSlot = slot;
    public void EvaluatePopUp(MenuGroup group)
    {
        if (SaveMode)
        {
            group.RemoveBackInput();
            if (currentSlot.HasData)
                OverwritePopUp.Reveal();
            else
                SavePopUp.Reveal();
        }
        else
        {
            if (currentSlot.HasData)
            {
                group.RemoveBackInput();
                LoadPopUp.Reveal();
            }
            else
                SoundManager.instance.PlayLockedUI();
        }
    }
    public void ReturnToPopUp(MenuGroup group)
    {
        group.RemoveBackInput();
        ReturnPopUp.Reveal();
    }
    /// <summary>
    /// Confirm To Title Screen / Main Menu
    /// </summary>
    public void ReturnToMenu()
    {
        MenuGroup.CanSelect = false;
        EventSystem.current.SetSelectedGameObject(null);
        DialogueAssetReader.ClearSaveData();
        
        ReturnPopUp.RemoveBackInput();
        
        StartCoroutine(FadeToMenu());
    }
    IEnumerator FadeToMenu()
    {
        GlobalFade.instance.FadeTo(0.3f);
        yield return new WaitForSecondsRealtime(0.3f);

        UIHandler.CallToTitle();
        EventSystem.current.SetSelectedGameObject(null);
        GameSaver.FirstTimeLoaded = true;
        RoomLoader.instance.RoomsCannotLoad();
        //ProgressionManager.instance.ClearLockedDialogue();

        DialogueAssetReader.instance.StopAllCoroutines();
        DialogueAnimConfig.instance.DisableVisuals();
        DialogueAnimConfig.instance.mainCanvas.enabled = false;
        DialogueAnimConfig.instance.ShowMainUI(false);
        DialogueTextConfig.instance.ClearText();
        MenuGroup.CanSelect = true;
        SceneManager.LoadSceneAsync(TitleScreenSceneName);
        yield break;
    }
    /// <summary>
    /// Confirm Load file, called from pop up
    /// </summary>
    public void LoadCurrentSlot()
    {
        
        int dataNum = GetSlotNum(currentSlot);
        DialogueAssetReader.ClearSaveData();
        GameSaver.LoadGameFile("save" + dataNum);
        if (OnPauseMenu) 
        {
            MenuGroup.CanSelect = false;
            // Visuals to transition to title screen
            // Load Title screen
            LoadPopUp.Hide();
            LoadPopUp.RemoveBackInput();
            EventSystem.current.SetSelectedGameObject(null);
            LoadPopUp.GetBackGroup().SetLastSelected(currentSlot.gameObject);
            StartCoroutine(FadeToMenu());
        }
        else // On the Main Menu Screen
        {
            GameSaver.ApplyCurrentData();
        }

        // At this point the static GameSaver.CurrentGameData is set
        
    }

    public void SaveCurrentSlot()
    {
        // Save Scenario Code
        int dataNum = GetSlotNum(currentSlot);
        GameSaver.SaveGameFile("save" + dataNum);
        LoadSlots();
        OnSave?.Invoke();
        // Below is just for testing beginning save data
        //BaseSave save = new BaseSave();
        //SaveSystem.SaveToJSON(save, "save" + dataNum);
        //LoadSlots();
    }

    public int GetSlotNum(SaveSlot slot)
    {
        for(int i = 0; i < SlotList.Count; i++)
        {
            SaveSlot s = SlotList[i];
            if (s == slot)
                return i;
        }
        return 0;
    }

    public void SelectSlot()
    {
        EventSystem.current.SetSelectedGameObject(currentSlot.gameObject);
        UIHandler.instance.current = currentSlot.gameObject;
    }

    public void ResetSlots(LayoutGroup layout) => StartCoroutine(ResetSlot(layout));
    IEnumerator ResetSlot(LayoutGroup layout)
    {
        yield return new WaitForSeconds(0.2f);
        //layout.transform.localPosition = Vector3.zero;
        //Debug.LogWarning("ResetSlots"); 
        yield break;
    }

    #region Helper Functions
    void SelectUpDown(Selectable on, Selectable u, Selectable d)
    {
        Navigation n = on.navigation;
        n.selectOnUp = u;
        n.selectOnDown = d;
        on.navigation = n;
    }
    void SelectOnUp(Selectable on, Selectable to)
    {
        Navigation n = on.navigation;
        n.selectOnUp = to;
        on.navigation = n;
    }
    void SelectOnDown(Selectable on, Selectable to)
    {
        Navigation n = on.navigation;
        n.selectOnDown = to;
        on.navigation = n;
    }
    #endregion

}
