//DRHelpPanel UI Script by SeleniumSoul for DREditor
//Note: This is not meant to be released yet for DREditor yet due to its unmodularity.

using UnityEngine;
using UnityEngine.UI;
using DREditor.EventObjects;
using TMPro;

public class DRHelpPanel : MonoBehaviour
{
    public DRInput _controls;

    public enum GameplayState
    {
        Explore3D, Explore2_5D, Conversation, Trial
    }
    public float _speed = 10;

    public BoolWithEvent InDialog, InMenu;
    public ScrollRect System;
    public GameplayState GState;

    public TextMeshProUGUI MainText;
    public RectTransform[] ControlLists = new RectTransform[2]; //To be updated soon
    public RectTransform[] SystemTexts = new RectTransform[2]; //To be updated soon

    //PanelTab = 0 Controls
    //PanelTab = 1 System
    private bool PanelTab = false;
    private Animator Panel;

    private void Awake()
    {
        Panel = GetComponent<Animator>();
        _controls = new DRInput();

        gameObject.GetComponentInParent<UIMenuSubmanager>().HelpUI = gameObject;
        gameObject.SetActive(false);

        _controls.UI.ChangeHelpTab.performed += ctx =>
        {
            switch (_controls.UI.ChangeHelpTab.ReadValue<float>())
            {
                case -1:
                    OnChangeTabLeft();
                    break;
                case 1:
                    OnChangeTabRight();
                    break;
            }
        };
    }

    void OnEnable()
    {
        _controls.Enable();

        if (InDialog.Value)
        {
            GState = GameplayState.Conversation;
            MainText.text = "Explore (Conversation)";
            System.content = SystemTexts[1];
            TextEnable(1);
        }
        else
        {
            GState = GameplayState.Explore3D;
            MainText.text = "Explore (3D)";
            System.content = SystemTexts[0];
            TextEnable(0);
        }
    }

    void TextEnable(int GStateIndex)
    {
        for (int count = 0; count < SystemTexts.Length; count++)
        {
            if (count != GStateIndex)
            {
                SystemTexts[count].gameObject.SetActive(false);
                ControlLists[count].gameObject.SetActive(false);
            }
            else
            {
                SystemTexts[count].gameObject.SetActive(true);
                ControlLists[count].gameObject.SetActive(true);
            }
        }
    }

    void OnChangeTabLeft()
    {
        if (PanelTab)
        {
            PanelTab = false;
            SwitchToControlScreen();
        }
    }

    void OnChangeTabRight()
    {
        if (!PanelTab)
        {
            PanelTab = true;
            SwitchToSystemScreen();
        }
    }

    void SwitchToControlScreen()
    {
        Panel.SetTrigger("ControlPanel");
    }

    void SwitchToSystemScreen()
    {
        Panel.SetTrigger("SystemPanel");
    }

    void OnDisable()
    {
        _controls.Disable();

        Panel.ResetTrigger("ControlPanel");
        Panel.ResetTrigger("SystemPanel");
        PanelTab = false;
    }

    public void PlaySFX(AudioClip clip)
    {
        GetComponent<AudioSource>()?.PlayOneShot(clip);
    }
}
