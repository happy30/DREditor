// Author LeotheDev
using DREditor.EventObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DREditor.PlayerInfo;
using static UnityEngine.InputSystem.InputAction;

public class BackLogUI : MonoBehaviour
{
    public static BackLogUI instance = null;

    Canvas Canvas => GetComponent<Canvas>();
    [SerializeField] AudioClip PauseSFX = null;
    [SerializeField] AudioClip UnPauseSFX = null;
    [SerializeField] BoolWithEvent InMenu;
    [SerializeField] BoolWithEvent InDialogue;
    //[SerializeField] GameObject FirstItem = null;
    [SerializeField] MenuGroup backGroup = null;

    struct BacklogEntry 
    {
        public DREditor.Characters.Character character;
        public string text;
        public string voiceline;
        public int aliasIndex;
        public Color color;
    };
    [System.Serializable]
    public class BacklogMugBind 
    {
        public string charaName;
        public Sprite Mug_ON;
        public Sprite Mug_OFF;
    }
    public readonly uint MaxLineCount = 50;
    public Transform[] Slots;
    public BacklogMugBind[] Mugshots;
    public Sprite OverlayOn;
    public Sprite OverlayOff;
    public DREditor.Characters.CharacterDatabase charaDatabase;

    public Transform cursor;
    public Vector2 cursorBounds;

    public float voicelineTime;

    uint m_lineCount = 0;

    BacklogEntry[] m_entries;

    int m_min, m_max, m_idx;
    bool m_canPlayVoiceline = true;

    public InputAction ScrollAction;
    public InputAction SelectAction;

    Transform[][] m_cachedTransforms;

    #region Controls
#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif
    private void Awake()
    {
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

    }
    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Enable();
        ScrollAction.Enable();
        SelectAction.Enable();
#endif
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Disable();
        ScrollAction.Disable();
        SelectAction.Disable();
#endif
    }
    #endregion

    private void Start()
    {
        UIHandler.ToTitle += ResetBackLog;
        _controls.UI.BackLog.started += EvaluateBackLog;

        //cache
        m_cachedTransforms = new Transform[Slots.Length][];
        for (int i = 0; i < Slots.Length; i++)
        {
            m_cachedTransforms[i] = new Transform[6];
            for (int j = 0; j < 6; j++) 
            {
                m_cachedTransforms[i][j] = Slots[i].GetChild(j);
            }
        }
    }
    void ResetBackLog()
    {
        _controls.UI.Cancel.started -= UnPause;
        Canvas.enabled = false;
        _controls.UI.Navigate.performed -= OnBacklogScroll;
    }
    void EvaluateBackLog(CallbackContext context)
    {
        
        if (InMenu.Value || Canvas.enabled || GameManager.instance.cantBeInMenu || Door.inLeaveProcess || ObserveManager.ChangingObserve)
            return;
        StartBackLog();
    }
    public void StartBackLog()
    {
        if (MenuGroup.Changing)
            return;
        //Debug.LogWarning("Start of Backlog");
        EventSystem.current.SetSelectedGameObject(null);
        _controls.UI.Navigate.performed += OnBacklogScroll;
        _controls.UI.Submit.performed += OnSelect;
        InMenu.Value = true;
        Canvas.enabled = true;
        Time.timeScale = 0;
        SoundManager.instance.PlaySFX(PauseSFX);
        _controls.UI.Cancel.started += UnPause;

        //Fetch data
        DREditor.Dialogues.BacklogLines[] l = DialogueAssetReader.Backlog.Lines.ToArray();
        
        
        var lines = l.Reverse().ToArray();
        if (lines.Length == 0)
        {
            m_lineCount = 0;
            cursor.gameObject.SetActive(false);
            RegenerateUI();
            return;
        }
        cursor.gameObject.SetActive(true);
        m_lineCount = lines.Length < MaxLineCount ? (uint)lines.Length : MaxLineCount;

        m_entries = new BacklogEntry[m_lineCount];

        for (int i = (int)m_lineCount - 1; i >= 0; i--)
        {
            var line = lines[i];
            m_entries[i].character = charaDatabase.GetCharacter(line.CharFirstName);
            m_entries[i].text = line.Text;
            //m_entries[i].voiceline = line.Voice;
            m_entries[i].aliasIndex = line.AliasNum;
            m_entries[i].color = line.color;
        }
        m_min = 0;
        m_max = m_min + Slots.Length - 1;
        m_idx = 0;
        RegenerateUI();
        MenuGroup.Changing = false;
    }
    void UnPause(CallbackContext context)
    {
        
        StartCoroutine(Resume());
    }
    IEnumerator Resume()
    {
        
        
        

        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        _controls.UI.Cancel.started -= UnPause;
        EventSystem.current.SetSelectedGameObject(null);
        _controls.UI.Navigate.performed -= OnBacklogScroll;
        _controls.UI.Submit.performed -= OnSelect;
        if (!PauseMenu.inPause)
            InMenu.Value = false;
        else
            Debug.Log("In Pause was true so in menu wasn't set to false");
        Canvas.enabled = false;
        Time.timeScale = 1;
        SoundManager.instance.StopVoiceLine();
        SoundManager.instance.PlaySFX(UnPauseSFX);
        if (backGroup != null && PauseMenu.inPause)
        {
            backGroup.EvaluateSelect();
            backGroup.AddBackInput();
        }

        yield return null;
    }

    // TO-DO: Set up when accessing from main Menu + Set Up in general
    /*
     * Working in GYM_Backlog
     * Use the first name to get the character asset (reference the character database)
     * The character asset will have a portrait for you to use to build your pieces
     * For the portrait, use the default headshot in the character asset
     * 
     * Create a function that gets the backlog class full of your lines. 
     * Take those lines to fill out the UI, your going to need to get the character portrait,
     * the character nameplate.
     * 
     * Suggested Idea: To set up I suggest using the scroll group class
     * making a prefab of a slot to use 
     * 
     * Your slots will be buttons 
     * 
     * When making UI Animations for your menu/scroll group have 2 parameters as triggers
     * name them Reveal and Hide
     * 
     * FEATURES OF A SLOT
     * - when a selected slot is pressed, it will play the voiceline
     *   use SoundManager.instance.PlayVoice to play that string. 
     *   If the voice string is empty do not show the Sound Icon.
     *   You do not have a Sound Icon Asset yet but please code as if you did
     */

    void RegenerateUI() 
    {
        uint localSelectedIDX = (uint)(m_idx - m_min);

        float pr = m_idx / Mathf.Max((float)m_lineCount - 1, 1);
        cursor.transform.localPosition = new Vector3(cursor.transform.localPosition.x, Mathf.Lerp(cursorBounds.x, cursorBounds.y, pr), 0);

        for (uint i = 0; i < Slots.Length; i++) 
        {
            BacklogEntry entry;
            uint globalIDX = (uint)(m_min + i);
            Transform[] cache = m_cachedTransforms[i];
            if (i >= m_lineCount)
            {
                cache[0].gameObject.SetActive(false);
                cache[4].gameObject.SetActive(false);
                cache[2].GetComponent<Image>().sprite = OverlayOff;
                cache[5].gameObject.SetActive(false);
                cache[1].gameObject.SetActive(false);
                cache[3].GetComponent<TMPro.TextMeshProUGUI>().text = string.Empty;
                continue;
            }
            if (localSelectedIDX == i)
            {
                entry = m_entries[globalIDX];
                BacklogMugBind mug1 = Mugshots.First(f => f.charaName == entry.character.FirstName);
                if (mug1 != null)
                {
                    cache[4].gameObject.SetActive(true);
                    cache[0].gameObject.SetActive(false);
                    cache[4].GetComponent<Image>().sprite = mug1.Mug_ON;
                }
                if (mug1 == null || (entry.character.FirstName == "Tozu" && entry.aliasIndex == 0))
                {
                    cache[4].gameObject.SetActive(false);
                    cache[0].gameObject.SetActive(false);
                }
                cache[5].gameObject.SetActive(true);
                cache[5].GetComponent<RawImage>().texture = entry.aliasIndex != -1 ? entry.character.Aliases[entry.aliasIndex].Nameplate : entry.character.Nameplate;
                cache[2].GetComponent<Image>().sprite = OverlayOn;
                cache[3].GetComponent<TMPro.TextMeshProUGUI>().text = entry.text;
                cache[3].GetComponent<TMPro.TextMeshProUGUI>().color = entry.color;
                cache[1].gameObject.SetActive(entry.voiceline == string.Empty ? false : true);

                continue;
            }
            entry = m_entries[globalIDX];
            BacklogMugBind mug = Mugshots.First(f => f.charaName == entry.character.FirstName);
            if (mug != null)
            {
                cache[4].gameObject.SetActive(false);
                cache[0].gameObject.SetActive(true);
                cache[0].GetComponent<Image>().sprite = mug.Mug_OFF;
            }
            if(mug == null || (entry.character.FirstName == "Tozu" && entry.aliasIndex == 0))
            {
                cache[4].gameObject.SetActive(false);
                cache[0].gameObject.SetActive(false);
            }
            cache[5].gameObject.SetActive(true);
            cache[5].GetComponent<RawImage>().texture = entry.aliasIndex != -1 ? entry.character.Aliases[entry.aliasIndex].Nameplate : entry.character.Nameplate;
            cache[2].GetComponent<Image>().sprite = OverlayOff;
            cache[3].GetComponent<TMPro.TextMeshProUGUI>().text = entry.text;
            cache[3].GetComponent<TMPro.TextMeshProUGUI>().color = entry.color;
            cache[1].gameObject.SetActive(entry.voiceline == string.Empty ? false : true);
        }
    }

    void OnBacklogScroll(CallbackContext ctx) 
    {
        Vector2 val = ctx.ReadValue<Vector2>();
        int off = (int)val.y;
        off = Mathf.Clamp(off, -1, 1);
        int tempIDX = m_idx + off;
        tempIDX = Mathf.Clamp(tempIDX, 0, (int)m_lineCount - 1);
        if(tempIDX == m_idx) 
            return;
        m_idx = tempIDX;
        if(m_idx > m_max) 
        {
            m_max++;
            m_min++;
        }
        if(m_idx < m_min) 
        {
            m_max--;
            m_min--;
        }

        //Play sound

        RegenerateUI();
    }

    IEnumerator VoiceLineTimer() 
    {
        yield return new WaitForSecondsRealtime(voicelineTime);
        m_canPlayVoiceline = true;
    }

    void OnSelect(CallbackContext ctx) 
    {
        if (m_lineCount == 0)
            return;
        BacklogEntry entry = m_entries[m_idx];
        if (entry.voiceline == string.Empty || !m_canPlayVoiceline)
            return;
        m_canPlayVoiceline = false;
        StartCoroutine(VoiceLineTimer());
        //SoundManager.instance.PlayVoiceLine(entry.voiceline);
    }
    private void OnDestroy()
    {
        UIHandler.ToTitle -= ResetBackLog;
        _controls.UI.Controls.started -= EvaluateBackLog;
    }
}
