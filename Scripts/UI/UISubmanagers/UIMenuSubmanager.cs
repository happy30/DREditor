using UnityEngine;
using DREditor.EventObjects;

public class UIMenuSubmanager : MonoBehaviour
{
    private DRInput _controls;

    public GameObject BacklogUI;
    public GameObject HelpUI;

    void Awake()
    {
        _controls = new DRInput();

        _controls.Global.Backlog.performed += ctx =>
        {
            BacklogUI.SetActive(!BacklogUI.activeSelf);
            //InMenu.Value = BacklogUI.activeSelf;
        };
        _controls.Global.Help.performed += ctx =>
        {
            HelpUI.SetActive(!HelpUI.activeSelf);
            //InMenu.Value = HelpUI.activeSelf;
        };
    }

    void OnEnable()
    {
        _controls.Enable();
    }

    void OnDisable()
    {
        _controls.Disable();
    }
}
