//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NSDReticle : MonoBehaviour
{
    Animator animator => GetComponent<Animator>();
    bool isShown = false;
    bool isHovering = false;
    public delegate void voidDelegate();
    public static event voidDelegate ShowHideReticle;
    public static event voidDelegate SetReticleHover;
    public static Vector3 Position = Vector3.zero;
    [SerializeField] string keyboardScheme = "KeyboardMouse";
    [SerializeField] float controllerSensitivity = 175;
    Camera cam;
    PlayerInput PlayerInput => GameManager.instance.GetInput();
    Vector3 pos => SetCursor();
    public static NSDReticle Instance = null;
#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif
    private void Awake()
    {
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        ShowHideReticle += ShowReticle;
        SetReticleHover += HoverReticle;
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
        ShowHideReticle -= ShowReticle;
        SetReticleHover -= HoverReticle;
    }
    Vector3 SetCursor()
    {
        if (_controls != null)
        {
            //return Reticle.transform.position;
            //Mouse.current.WarpCursorPosition(Mouse.current.position.ReadValue() + playerInput.actions["Look"].ReadValue<Vector2>());
            Vector2 p = _controls.Player.Cursor.ReadValue<Vector2>();
            return new Vector3(p.x, p.y, 0);
        }
        return Vector3.zero;
    }
    private void Start()
    {
        cam = NSDManager.instance.trialCamera.GetComponentInChildren<Camera>();
    }
    private void OnGUI()
    {
        Cursor.visible = false;
    }
    private void Update()
    {
        SetPosition();
        if (isShown)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.ScreenPointToRay(transform.position), out hit))
            {
                bool isPhrase = hit.transform.gameObject.GetComponent<PhraseText>() != null;
                bool isWhiteNoise = hit.transform.gameObject.GetComponent<WhiteNoiseText>() != null;
                if (!isHovering && (isPhrase || isWhiteNoise))
                    SetReticleHover();
            }
            else if (isHovering)
                SetReticleHover();
        }
    }
    void SetPosition()
    {
        if (PlayerInput.currentControlScheme == keyboardScheme)
        {
            transform.position = pos;
        }
        else
        {
            //transform.position += pos * 10;
            Vector3 m = _controls.Player.Look.ReadValue<Vector2>() * controllerSensitivity; // read value
            Vector3 v = transform.position + m; // predicted
            //transform.position += v;
            if (v.x < Screen.width && v.x > 0)
                transform.position = new Vector3(v.x, transform.position.y, 0);

            if (v.y < Screen.height && v.y > 0)
                transform.position = new Vector3(transform.position.x, v.y, 0);
            Debug.LogWarning(transform.position);
        }
        
        Position = transform.position;
    }
    void ShowReticle()
    {
        // If the hover is visable then turn it off, will this affect the state of reticle hover?
        // If so then change isHover bool
        isShown = !isShown;
        animator.SetBool("isShown", isShown);
    }
    public void ShowReticleOverride(bool to)
    {
        isShown = to;
        animator.SetBool("isShown", isShown);
    }
    void HoverReticle()
    {
        isHovering = !isHovering;
        animator.SetBool("isHovering", isHovering);
    }
    public static void ShowOrHide() => ShowHideReticle?.Invoke();
    public static void SetHover() => SetReticleHover?.Invoke();
    private void OnDestroy()
    {
        Instance = null;
    }
}
