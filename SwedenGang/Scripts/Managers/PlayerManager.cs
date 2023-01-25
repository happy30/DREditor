//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Camera;
using DREditor.FPC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerManager : MonoBehaviour
{
    //[SerializeField] Camera mainCam = null;
    //[SerializeField] Camera diaCam = null;
    [SerializeField] Camera protagCam = null;
    [SerializeField] ControlMonobehaviours controller = null;
    public Camera mainCamera = null;
    public Camera dialogueCamera = null;
    public Camera blurCamera = null;
    public Camera protagCamera = null;
    public Volume blur = null;
    Vector3 ogPos;
    Vector3 ogRot;

    public delegate void PlayerDel();
    public static PlayerDel ResetPlayer;
    public static PlayerManager instance = null;
    private PnCCamera pnc;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        pnc = (PnCCamera)controller.TPFDScript;
    }
    private void Start()
    {
        
        ogPos = protagCam.transform.localPosition;
        ogRot = protagCam.transform.localEulerAngles;
        ResetPlayer += ResetCameras;
        if (GameManager.instance.currentMode == GameManager.Mode.Trial)
        {
            DisablePlayer();
        }
        UIHandler.ToTitle += ResetMain;
    }
    private void OnDestroy()
    {
        ResetPlayer -= ResetCameras;
        UIHandler.ToTitle -= ResetMain;
    }
    void ResetMain()
    {
        controller.enabled = false;
        controller.disable = false;
        controller.setting = false;
    }
    public void EnableMainCamera(bool to)
    {
        mainCamera.enabled = to;
    }
    void ResetCameras()
    {
        protagCam.transform.localPosition = ogPos;
        protagCam.transform.localEulerAngles = ogRot;
        //diaCam.transform.localPosition = mainCam.transform.localPosition;
    }
    public void DisablePlayer()
    {
        mainCamera.enabled = false;
        dialogueCamera.enabled = false;
        blurCamera.enabled = false;
        protagCam.enabled = false;
        EnableMonoScripts(false);
        EnableControlMono(false);
        blur.enabled = false;
    }
    public void DisableMovement()
    {
        controller.MonoBehaviours[0].enabled = false; // Should always be MovePlayer.cs
    }
    public PnCCamera GetTPFD()
    {
        return (PnCCamera)controller.TPFDScript;
    }
    public void EnableControlMono(bool to)
    {
        //Debug.LogWarning("Setting Controller to: " + to);
        controller.enabled = to;
    }
    public void EnableScripts(bool to)
    {
        EnableMonoScripts(to);
        controller.EnableTPFD(to);
    }
    public void EnableMonoScripts(bool to)
    {
        controller.Enable(to);
    }
    public void TPFDControllable(bool to)
    {
        pnc.Controllable = to;
    }
    public bool GetControl() => controller.enabled;
    public void DialogueCamToMain()
    {
        dialogueCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);
    }
}
