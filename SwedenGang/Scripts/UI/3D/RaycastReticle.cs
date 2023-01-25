//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.EventObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
/// <summary>
/// For selecting in 3D mode
/// Requires: GameManager, SoundManager, Door 
/// Won't work without: 
/// Animator in scene named Reticle with Animator component
/// Canvas named "3D UI" 
/// </summary>
public class RaycastReticle : MonoBehaviour
{
    [SerializeField] PlayerInput Input = null;
    [SerializeField] float controllerSensitivity = 45;
    [SerializeField] string keyboardScheme = "KeyboardMouse";
    [SerializeField] Animator Reticle = null;
    [SerializeField] Canvas reticleCanvas = null;
    [SerializeField] BoolWithEvent inDialogue = null;
    [SerializeField] BoolWithEvent inTPFD = null;
    [SerializeField] BoolWithEvent inMenu = null;
    [SerializeField] float tpfdDistance = 20;
    //[EventRef] [SerializeField] string hover = null;
    [SerializeField] AudioClip confirm = null;
    [SerializeField] AudioClip hoverSound = null;
    [Header("Animator Bool Strings")]
    [SerializeField] string appear = "Appear";
    [SerializeField] string speak = "SpeakIcon";
    [SerializeField] string item = "ItemIcon";
    [SerializeField] string confirmSelect = "ConfirmSelect";
    [SerializeField] string selected = "IsSelected";

    private bool inRange = false;
    private bool Unlocked => ToggleReticle();
#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif
    Vector3 Pos => SetCursor();
    Camera characterCamera;
    bool hovering = false;

    public static bool canSelect = true;
    public static bool canSelectOverride = true;
    private void OnTriggerEnter(Collider other) => inRange = true;
    private void OnTriggerExit(Collider other) => inRange = false;
    public static RaycastReticle Instance = null;
    private void Awake()
    {
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

    }
    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Enable();
#endif
        //Debug.LogWarning("Enabled");
        canSelect = true;
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Disable();// error called here in testing
#endif
        //Debug.Log("Disabled");
        canSelect = false;
    }
    void Start()
    {

        try
        {
            if (!Reticle)
                Reticle = GameObject.Find("Reticle").GetComponent<Animator>();
            if (!reticleCanvas)
                reticleCanvas = GameObject.Find("3D UI").GetComponent<Canvas>();

        }
        catch
        {
            Debug.LogWarning("Could not find reticle assets! Make sure you have: " +
                "A GameObject named \"Reticle\" that has an Animator Component AND " +
                "A GameObject named \"3D UI\" that is a Canvas! and if this is on a 2.5D camera then don't reference these two!");
        }
        if (!Input)
            Debug.LogError("You must have a Player Input Referenced So Raycast Reticle can work in 2.5D mode");
        characterCamera = GetComponent<Camera>();
        Cursor.visible = false;
        AddEnd();
    }
    private void OnDestroy()
    {
        RemoveEnd();
    }
    public void AddEnd()
    {
        DialogueAssetReader.OnDialogueEnd += FadeIn;
        RoomLoader.StartLoad += FadeOut;
        RoomLoader.EndLoad += FadeIn;
    }
    public void RemoveEnd()
    {
        DialogueAssetReader.OnDialogueEnd -= FadeIn;
        RoomLoader.StartLoad -= FadeOut;
        RoomLoader.EndLoad -= FadeIn;
    }
    void FadeIn()
    {
        if (!Door.inLeaveProcess && !RoomLoader.instance.inLoading.Value
            && !inDialogue.Value)
        {
            //Debug.LogWarning("Fade in was true!");
            SetBool(confirmSelect, false);
            SetBool(appear, true);
        }
    }
    public void FadeOut()
    {
        // Might add this function to UIHandler.ToTitle
        SetBool(appear, false);
    }
    public void ResetReticleVisual()
    {
        SetBool(speak, false);
        SetBool(item, false);
    }
    public void EnableCanvas(bool to)
    {
        reticleCanvas.enabled = to;
    }
    Vector3 SetCursor()
    {
        if (!canSelect)
        {
            //Debug.Log("Can select is false");
            return Reticle.transform.position;
        }
        if (_controls != null)
        {
            //Vector2 p = PlayerInput.actions["Cursor"].ReadValue<Vector2>();
            Vector2 p = _controls.Player.Cursor.ReadValue<Vector2>();
            //Debug.Log(p);
            return new Vector3(p.x, p.y, 0);
        }
        Debug.LogWarning("Controls is Null");
        return Vector3.zero;
    }
    bool ToggleReticle() => GameManager.instance != null && (GameManager.instance.currentMode == GameManager.Mode.TPFD || inTPFD.Value) &&
            !inDialogue.Value && !inMenu.Value;
    void Update()
    {
        // Bit shift the index of the layer (6) to get a bit mask
        // Add these layers in at their numbers
        int dialogueLayer = 1 << 6;
        int uiLayer = 1 << 7;
        int itemLayer = 1 << 11;
        //int gLayer = 1 << 15; // Door Layer
        // This would cast rays only against colliders in layer 6.
        // The ~ operator does this, it inverts a bitmask.
        //layerMask = ~layerMask;
        //Debug.LogWarning("" + canSelect + " " + !Door.inLeaveProcess + " " + inRange + " " + Unlocked);
        if (!inDialogue.Value && !GameSaver.LoadingFile && canSelectOverride && canSelect && !Door.inLeaveProcess && (inRange || Unlocked))
        {
            //Debug.Log("In Dialogue: " + inDialogue.Value);
            RaycastHit hit;
            Ray ray;
            float distance;
            if (Unlocked)
            {
                ray = characterCamera.ScreenPointToRay(Reticle.transform.position);
                distance = tpfdDistance;
                Cursor.visible = false;
                
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
                distance = 1.5f;
                Cursor.lockState = CursorLockMode.Locked;
            }
            
            //Debug.LogWarning(ray);
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(ray, out hit, distance, uiLayer))
            {
                //Debug.LogWarning("UI Hit");
                hovering = true;
                ReticleVisual(hit);
                if (_controls.Player.Fire.triggered)
                {
                    hit.transform.SendMessage("HitByRay");
                    inMenu.Value = true;
                    reticleCanvas.gameObject.SetActive(false);
                    SoundManager.instance.PlaySFX(confirm);
                }
            }
            else if (Physics.Raycast(ray, out hit, distance, dialogueLayer))
            {
                //Debug.LogWarning("Dialogue Hit");
                if(!hovering)
                    SoundManager.instance.PlaySFX(hoverSound);
                hovering = true;
                //DialogueAssetReader.instance.reticleIcon.texture = speakIcon;
                SetBool(item, false);
                ReticleVisual(hit, speak);
                if (_controls.Player.Fire.triggered)
                {
                    SetBool(confirmSelect, true);
                    hit.transform.SendMessage("HitByRay");
                    inDialogue.Value = true;
                    SoundManager.instance.PlaySFX(confirm);
                }
            }
            else if (Physics.Raycast(ray, out hit, distance, itemLayer))
            {
                //Debug.LogWarning("Item Hit");
                if (!hovering)
                    SoundManager.instance.PlaySFX(hoverSound);
                hovering = true;
                SetBool(speak, false);
                ReticleVisual(hit, item);
                
                if (_controls.Player.Fire.triggered)
                {
                    SetBool(confirmSelect, true);
                    hit.transform.SendMessage("HitByRay");
                    inDialogue.Value = true;
                    SoundManager.instance.PlaySFX(confirm);
                }
            }
            else if (Physics.Raycast(ray, out hit, distance) && hit.transform.gameObject.layer == 15)
            {
                //Debug.LogWarning("Door Hit");
                if (!hovering)
                    SoundManager.instance.PlaySFX(hoverSound);
                hovering = true;
                SetBool(speak, false);
                CallObject(hit, "CallLock", item);
            }
            else
            {
                //Debug.LogWarning("Not hitting Anything");
                //Physics.Raycast(ray, out hit, distance);
                //if (hit.transform != null)
                    //Debug.LogWarning(hit.transform.gameObject.name);
                if (hovering)
                {
                    hovering = false;
                    if(ItemDisplayer.instance)
                        ItemDisplayer.instance.HideName();

                }
                if (Reticle != null)
                {
                    foreach (AnimatorControllerParameter parameter in Reticle.parameters)
                    {
                        if (parameter.name == appear)
                            continue;
                        SetBool(parameter.name, false);
                    }
                }
            }
        }

        SetPosition();

        if (reticleCanvas != null && !inDialogue.Value && !inMenu.Value)
            reticleCanvas.gameObject.SetActive(true);
        else if (reticleCanvas != null && !inDialogue.Value)
        {
            Reticle.transform.localPosition = Vector3.zero;
        }
    }
    void ReticleVisual(RaycastHit hit, string icon = "")
    {
        if (!Reticle)
            return;
        SetBool(selected, true);
        if (HasParameter(icon, Reticle))
            SetBool(icon, true);
        try
        {
            hit.transform.SendMessage("DisplayName");
        }
        catch
        {

        }
    }
    void CallObject(RaycastHit hit, string message, string icon = "")
    {
        ReticleVisual(hit, icon);
        if (_controls.Player.Fire.triggered && Reticle != null)
        {
            SetBool(confirmSelect, true);
            hit.transform.SendMessage(message);
            if(SoundManager.instance != null)
                SoundManager.instance.PlaySFX(confirm);
        }
    }
    void SetPosition()
    {
        if (Unlocked)
        {
            if (Input.currentControlScheme == keyboardScheme)
                Reticle.transform.position = Pos;
            else
            {
                Vector3 m = _controls.Player.Look.ReadValue<Vector2>() * controllerSensitivity; // read value
                Vector3 v = Reticle.transform.position + m; // predicted

                if (v.x < Screen.width && v.x > 0)
                    Reticle.transform.position = new Vector3(v.x, Reticle.transform.position.y, 0);

                if (v.y < Screen.height && v.y > 0)
                    Reticle.transform.position = new Vector3(Reticle.transform.position.x, v.y, 0);
                //Debug.LogWarning(Reticle.transform.position);
            }
            
        }
        
    }
    public void UnSelectVisual()
    {
        SetBool(selected, false);
    }
    public void CenterReticle()
    {
        Reticle.transform.localPosition = Vector3.zero;
    }
    Dictionary<string, bool> cacheKnown = new Dictionary<string, bool>();
    void SetBool(string paramName, bool to)
    {
        bool known = cacheKnown.ContainsKey(paramName);
        if (!known)
        {
            cacheKnown.Add(paramName, HasParameter(paramName, Reticle));
        }
        if (cacheKnown[paramName])
        {
            Reticle.SetBool(paramName, to);
        }
    }
    public static bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
}
