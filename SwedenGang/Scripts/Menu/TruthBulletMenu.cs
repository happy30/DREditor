//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.TrialEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.EventSystems;
using TMPro;
using DREditor.PlayerInfo;
public class TruthBulletMenu : MonoBehaviour
{
    /* TO-DO: For NSD, Have bullets in cylinder shown first and then
     * the rest of the bullets organized
     */
    [SerializeField] EvidenceDatabase edb = null;
    [SerializeField] bool trialVersion = false;
    [SerializeField] bool inTruthBulletSelectMinigame = false;
    public ScrollGroup scrollGroup = null;
    public Evidence evidence = null;
    [SerializeField] Image trialBG = null;
    //[SerializeField] Canvas canvas = null;
    [SerializeField] GameObject first = null;
    [SerializeField] Image bulletImage = null;
    [SerializeField] TextMeshProUGUI bulletText = null;
    [HideInInspector] public bool inMenu = false;
    //[SerializeField][EventRef] string openSound;
    //[SerializeField][EventRef] string closeSound;
    DRControls _controls;
    private void Awake()
    {
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
    public static bool OnMenu = false;
    private void Start()
    {
        SetChapterEvidence();
        //Load();
        
    }
    bool firstSet = false;
    [SerializeField] bool ignoreFirst = false;
    public void SetFirst() // Called from TBM Animator
    {
        if (!trialVersion && !inTruthBulletSelectMinigame)
            return;
        if (ignoreFirst)
            return;
        if (!firstSet)
        {
            Debug.Log("Called SetFirst");
            Time.timeScale = 0;
            EventSystem.current.SetSelectedGameObject(first);
            scrollGroup.SetCellSize();
            scrollGroup.SetContainerBounds();
            firstSet = true;
            if (!inTruthBulletSelectMinigame)
                AddBack();
            //UIHandler.instance.OnChange.AddListener(WriteInfo);
        }
        else
        {
            Debug.Log("Called True SetFirst");
            firstSet = false;
            //UIHandler.instance.OnChange.RemoveListener(WriteInfo);
            EventSystem.current.SetSelectedGameObject(null);
        }
        
    }
    public bool localTime = false;
    public void ReturnScale() // Called from TBM Animator
    {
        if (!trialVersion && !inTruthBulletSelectMinigame)
            return;
        if (localTime)
        {
            Time.timeScale = 1;
            ApplyFirstBullet();
            inMenu = false;
            OnMenu = false;
            localTime = false;
            return;
        }
        if (!localTime)
        {
            OnMenu = true;
            localTime = true;
            ApplyFirstBullet();
        }
    }
    void ApplyFirstBullet()
    {
        if (evidence.TruthBullets.Count > 0 && evidence.TruthBullets[0].Picture != null)
        {
            bulletImage.sprite = evidence.TruthBullets[0].Picture;
            Debug.LogWarning("Applied first bullet");
            bulletText.text = evidence.TruthBullets[0].Description;
        }
    }
    
    public void ShowTrial(CallbackContext context)
    {
        if (inMenu)
            return;
        else
            inMenu = true;
        Load();

        trialBG.DOFade(0.9f, 0.2f)
            .SetUpdate(true);
        scrollGroup.Reveal();
        
    }
    public void HideTrial(CallbackContext context)
    {
        scrollGroup.Hide();
        trialBG.DOFade(0, 0.2f)
            .SetUpdate(true);

        if (!inTruthBulletSelectMinigame)
            RemoveBack();
        
    }
    public void AddBack() => _controls.UI.Cancel.started += HideTrial;
    public void RemoveBack() => _controls.UI.Cancel.started -= HideTrial;
    public void Show()
    {
        /*if (inMenu)
            return;
        else
            inMenu = true;
        */
        if (scrollGroup.isActive)
            return;
        
        Load();
        trialBG.DOFade(0.9f, 0.2f)
            .SetUpdate(true);
        //scrollGroup.Reveal();
    }
    public void Hide()
    {
        //scrollGroup.Hide();
        trialBG.DOFade(0, 0.2f)
            .SetUpdate(true);
    }
    void SetChapterEvidence()
    {
        int n = ProgressionManager.instance.GetChapterNum();
        if (n >= edb.Evidences.Count)
            return;
        evidence = edb.Evidences[n];
    }
    public void Load()
    {
        if (trialVersion)
            LoadTrial();
        else
            LoadInv();
    }
    void LoadInv()
    {
        if (!evidence || evidence != edb.Evidences[ProgressionManager.instance.GetChapterNum()])
            SetChapterEvidence();

        List<TruthBullet> temp = new List<TruthBullet>();
        foreach (TruthBullet b in evidence.TruthBullets)
            temp.Add(b);

        for (int x = 0; x < scrollGroup.scrollOptions.Count; x++)
        {
            ScrollOption o = scrollGroup.scrollOptions[x];
            bool found = false;
            for (int i = 0; i < temp.Count; i++)
            {
                TruthBullet t = temp[i];

                if (found)
                    break;
                for (int j = 0; j < PlayerInfo.instance.Info.foundBullets.Count; j++)
                {
                    if (PlayerInfo.instance.Info.foundBullets[j] == t.Title)
                    {
                        o.text.text = t.Title;
                        o.selectable.interactable = true;
                        o.EnableOption();
                        found = true;
                        temp.Remove(t);
                        break;
                    }
                }
                if (found)
                    break;
                
            }
            if (!found)
            {
                //Debug.LogWarning("Disabling " + x);
                //Debug.LogWarning(x + " was " + found);
                o.DisableOption();
            }
            
        }
        

        DisableUnused();
    }
    void LoadTrial() // Set the scroll options and write their texts
    {
        if (PlayerInfo.instance.Info.foundBullets.Count == 0)
            foreach (TruthBullet t in evidence.TruthBullets)
                PlayerInfo.instance.Info.AddBullet(t.Title);
        LoadInv();
    }
    void DisableUnused()
    {
        for (int i = 0; i < scrollGroup.scrollOptions.Count; i++) // Disable Unused Options
        {
            if (i < evidence.TruthBullets.Count)
                continue;
            else
                scrollGroup.scrollOptions[i].DisableOption();
        }
    }

    public void WriteInfo() // Write the desc and image based on selected object
    {
        Debug.LogWarning("WriteInfo Called bad");
        ScrollOption current = EventSystem.current.currentSelectedGameObject.GetComponent<ScrollOption>();
        TruthBullet t = GetBullet(current);
        if (t.Picture != null)
            bulletImage.sprite = t.Picture;

        bulletText.text = t.Description;
    }

    TruthBullet GetBullet(ScrollOption o)
    {
        
        for (int i = 0; i < evidence.TruthBullets.Count; i++)
        {
            TruthBullet t = evidence.TruthBullets[i];
            if (o.text.text == t.Title)
                return t;

        }
        Debug.LogWarning("Couldn't find TruthBullet from scroll option!");
        return null;
    }

    public void WriteInfo(ScrollOption current) // Write the desc and image based on selected object
    {
        //Debug.LogWarning("WriteInfo Called Scroll");
        //ScrollOption current = option.GetComponent<ScrollOption>();
        TruthBullet t = GetBullet(current);
        //Debug.LogWarning(t.Picture.name);
        if (t.Picture != null)
            bulletImage.sprite = t.Picture;

        bulletText.text = t.Description;
    }
}
