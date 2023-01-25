//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.EventObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DREditor.PlayerInfo;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;
using TMPro;

public class ControlsUI : MonoBehaviour
{
    /* Dialogue = dialogue
     * 2.5D = tpfd
     * 3D = threed
     * Trial Dialogue = trialdialogue
     * NSD = nsd
     * NSD Pathos = nsdp
     * Closing Argument = ca
     */
    public static ControlsUI instance = null;
    // public static string ControlName = "";
    Canvas Canvas => GetComponent<Canvas>();
    [SerializeField] AudioClip PauseSFX = null;
    [SerializeField] AudioClip UnPauseSFX = null;
    [SerializeField] AudioClip SwitchSFX = null;
    [SerializeField] BoolWithEvent InMenu;
    [SerializeField] BoolWithEvent InDialogue;
    [SerializeField] ControlsDatabase database = null;
    [SerializeField] bool disableSystemTab = false;
    static string Key = "";
    void SetKey(string s)
    {
        //Debug.LogWarning("Setting Controls Key to: " + s);
        Key = s;
        //Debug.LogWarning("Controls Key is: " + Key);
    }
    public static bool Override = false;

    public ControlsUIPanel temp_targetPanel;

    public GameObject ControlsTab, SystemTab;


    ControlsUIPanel m_activePanel;
    bool m_activeTab = false; //0 - Controls / 1 - System
    bool m_panelBuilt = false;

    public TextMeshProUGUI Title;
    public Transform[] controlRows;

    #region Controls
#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
        DontDestroyOnLoad(this);
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
        if(_controls != null)
            _controls.Disable();
#endif
    }
    #endregion

    private void Start()
    {
        UIHandler.ToTitle += ResetBackLog;
        GameManager.OnSetMarker += SetKey;
        GameManager.OnCallControls += EvaluateGlobal;
        _controls.UI.Controls.started += EvaluateControls;
    }
    void ResetBackLog()
    {
        _controls.UI.Cancel.started -= UnPause;
        Canvas.enabled = false;
    }

    ControlsUIPanel FetchActivePanel() 
    {
        //Get teh active paanel here
        // return database.GetPanel(ControlsName);
        // if above == null then use the temp_TargetPanel
        if (database == null)
            return temp_targetPanel;
        ControlsUIPanel p = database.GetPanel(Key);
        if (p != null)
            return p;
        else
            return temp_targetPanel;
    }
    void EvaluateGlobal(bool b)
    {
        if (b)
        {
            Activate();
        }
    }
    void EvaluateControls(CallbackContext context)
    {
        //Debug.LogWarning("Evaluate Pause Called");
        if (PlayerInfo.instance.Info.pauseAccess && !TrialManager.InMinigame && !Canvas.enabled && !GameManager.instance.cantBeInMenu 
            && !Door.inLeaveProcess
            && !InMenu.Value && !ObserveManager.ChangingObserve && !Override)
        {
            Activate();
        }
    }
    void Activate()
    {
        m_activePanel = FetchActivePanel();
        if(m_activePanel == null)
        {
            Debug.LogWarning("ACTIVE PANEL FOR CONTROLS UI WAS NULL");
            return;
        }
        Debug.LogWarning("Controls Key is: " + Key);
        m_activeTab = false;
        m_panelBuilt = false;
        UpdateMenu();
        InMenu.Value = true;
        Canvas.enabled = true;
        Time.timeScale = 0;
        SoundManager.instance.PlaySFX(PauseSFX);

        _controls.UI.Cancel.started += UnPause;
        _controls.Minigame.Move.performed += SwitchMenus;
    }
    void UnPause(CallbackContext context)
    {
        StartCoroutine(Resume());
    }
    IEnumerator Resume()
    {
        
        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        _controls.UI.Cancel.started -= UnPause;
        _controls.Minigame.Move.performed -= SwitchMenus;
        EventSystem.current.SetSelectedGameObject(null);
        InMenu.Value = false;
        Canvas.enabled = false;
        Time.timeScale = 1;
        SoundManager.instance.PlaySFX(UnPauseSFX);
        yield return null;
    }
    // TO-DO: Set up for the game to know what mode your in
    // and also code to display such things
    private void OnDestroy()
    {
        UIHandler.ToTitle -= ResetBackLog;
        _controls.UI.Controls.started -= EvaluateControls;
        _controls.Minigame.Move.performed -= SwitchMenus;
    }
    void SwitchMenus(InputAction.CallbackContext context) 
    {
        if (disableSystemTab)
            return;
        bool s = false;
        if (context.ReadValue<Vector2>().x > 0)
            s = true;
        if (s == m_activeTab)
            return;
        m_activeTab = s;
        SoundManager.instance.PlaySFX(SwitchSFX);
        UpdateMenu();
    }
    void UpdateMenu() 
    {
        if (!m_panelBuilt) //No need to rebuild the panel when switching between Controls and System
        {
            for (int i = 0; i < controlRows.Length; i++)
            {
                Transform target = controlRows[i];
                if (i >= m_activePanel.ControlRows.Length)
                {
                    target.gameObject.SetActive(false);
                    continue;
                }
                ControlPanelRow row = m_activePanel.ControlRows[i];

                target.gameObject.SetActive(true);
                Transform[] tr = new Transform[3];
                tr[0] = target.Find("Icon_LEFT");
                tr[1] = target.Find("Icon_CENTER");
                tr[2] = target.Find("Icon_RIGHT");

                tr[0].gameObject.SetActive(row.Left != null);
                tr[1].gameObject.SetActive(row.Center != null);
                tr[2].gameObject.SetActive(row.Right != null);

                tr[0].GetComponent<Image>().sprite = row.Left;
                tr[1].GetComponent<Image>().sprite = row.Center;
                tr[2].GetComponent<Image>().sprite = row.Right;
                target.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = row.Description;
            }
            SystemTab.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = m_activePanel.SystemDescription;
            m_panelBuilt = true;
        }
        Title.text = m_activePanel.PanelTitle;
        if (!m_activeTab)
        {
            ControlsTab.SetActive(true);
            SystemTab.SetActive(false);
            return;
        }
        SystemTab.SetActive(true);
        ControlsTab.SetActive(false);
    }
}
