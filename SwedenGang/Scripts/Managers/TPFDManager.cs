//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Camera;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manipulates the PnC Camera On the player, should be on separate game object in the scene
/// </summary>
public class TPFDManager : MonoBehaviour
{
    [SerializeField] bool DebugMode = false;
    [SerializeField] bool Reset = false;
    [SerializeField] bool setProtag = true;
    [SerializeField] float LAngle = 20;
    [SerializeField] float RAngle = 20;
    [SerializeField] float TAngle = 20;
    [SerializeField] float BAngle = 20;
    [SerializeField] float Distance = 10;
    [SerializeField] Vector3 CharPosition;
    [SerializeField] float InitialHAngle = 0f;
    [SerializeField] float InitialVAngle = 0f;
    private PnCCamera cam;
    private GameObject mainCamera = null;

    [SerializeField] Vector3 ProtagPosition;
    [SerializeField] Vector3 ProtagRotation;
    private Camera protag;
    private Vector3 basePosition;
    private Vector3 baseRotation;
    public float GetInitialHAngle() => InitialHAngle;
    // The Canvas that holds the arrows should be priority -2
    bool Up => cam.transform.rotation.eulerAngles.x >= TAngle - 1 && cam.transform.rotation.eulerAngles.x <= TAngle + 1;
    bool Down => cam.transform.rotation.eulerAngles.x >= -BAngle + 360 - 1 && cam.transform.rotation.eulerAngles.x <= -BAngle + 360 + 1;
    bool Left => cam.transform.rotation.eulerAngles.y >= LAngle - 1 && cam.transform.rotation.eulerAngles.y <= LAngle + 1;
    bool Right => cam.transform.rotation.eulerAngles.y >= -RAngle + 360 - 1 && cam.transform.rotation.eulerAngles.y <= -RAngle + 360 + 1;
    public Vector3 GetCharPosition() => CharPosition;
    public delegate void IntDel(int d);
    public static event IntDel Cancel;
    public static event IntDel UnCancel;
    public static bool SetCamAtStart = false;
    
    public void StartEarly() => Start();
    private void Start()
    {
        if (mainCamera != null)
            return;
        mainCamera = GameObject.Find("Main Camera");
        cam = mainCamera.GetComponent<PnCCamera>();
        protag = GameObject.Find("Protag Camera").GetComponent<Camera>();
        SetCamera();
        if(setProtag)
            SetProtag();
    }
    private void Update()
    {
        Check();
        if (DebugMode)
        {
            Set();
        }
        if (Reset)
            SetCamera();
    }
    void Check()
    {
        Do(Up, 0);
        Do(Down, 1);
        Do(Left, 2);
        Do(Right, 3);
        
    }
    private void Do(bool b, int d)
    {
        if (b)
            Cancel?.Invoke(d);
        else
            UnCancel?.Invoke(d);
    }
    void SetCamera()
    {
        cam.ClearPosition();
        Set();
    }
    public void Set()
    {
        cam.MaxLAngle = LAngle;
        cam.MaxRAngle = RAngle;
        cam.MaxTAngle = TAngle;
        cam.MaxBAngle = BAngle;
        cam.CamDistance = Distance;
        cam.CharPosition = CharPosition;
        cam.InitialHAngle = InitialHAngle;
        cam.InitialVAngle = InitialVAngle;
    }
    void SetProtag()
    {
        basePosition = protag.transform.position;
        baseRotation = protag.transform.eulerAngles;
        protag.transform.position = ProtagPosition;
        protag.transform.eulerAngles = ProtagRotation;
    }
    private void OnDestroy()
    {
        try
        {
            PlayerManager.ResetPlayer();
        }
        catch
        {

        }
    }
}
