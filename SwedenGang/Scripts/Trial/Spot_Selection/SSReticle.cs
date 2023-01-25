//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Camera;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Spot Selection Reticle
/// </summary>
public class SSReticle : MonoBehaviour
{
    /// <remark>
    /// This needs Rigidbody2D and BoxCollider2D
    /// with Rigidbody's gravity scale set to 0
    /// </remark>
    [Header("Debugging")]
    [Tooltip("For just debugging the reticle make sure that the SSManager's debug mode is off")]
    [SerializeField] bool debugMode = false;
    [SerializeField] bool locked = true;
    [SerializeField] bool useIconBool = false;
    [SerializeField] string iconString = "ItemIcon";
    
    public static Vector3 Position = Vector3.zero;
    [SerializeField] string keyboardScheme = "KeyboardMouse";
    [SerializeField] float controllerSensitivity = 175;
    [SerializeField] string appearString = "Appear";
    [SerializeField] string hoverString = "IsSelected";

    
    [SerializeField] AudioClip hoverSound = null;


    Vector3 pos => SetCursor();
    Animator animator => GetComponent<Animator>();
    bool isShown = false;
    bool isHovering = false;
    Collider2D spot = null;

    //public delegate void voidDelegate();
    //public static event voidDelegate ShowHideReticle;
    //public static event voidDelegate SetReticleHover;
    PlayerInput PlayerInput => GameManager.instance.GetInput(); // For Controller use

#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif
    private void Awake()
    {
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
        //ShowHideReticle += ShowReticle;
        //SetReticleHover += HoverReticle;
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
        //ShowHideReticle -= ShowReticle;
        //SetReticleHover -= HoverReticle;
    }
    public void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        if (spot != null)
            return;
        HoverReticle(true);
        spot = collision;
        //Debug.LogWarning("Collision Set to: " + collision.gameObject.name);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (spot != null)
            return;
        if (!isHovering)
            HoverReticle(true);
        spot = collision;
        //Debug.LogWarning("Collision Set to: " + collision.gameObject.name);
    }
    public void OnTriggerExit2D(UnityEngine.Collider2D collision)
    {
        if (collision != spot)
            return;
        HoverReticle(false);
        spot = null;
        //Debug.LogWarning("Collision Null");
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
        
        if (debugMode)
            ShowReticle();
    }
    private void OnGUI()
    {
        Cursor.visible = false;
    }
    public Collider2D GetSpot() => spot;
    public void SetLock(bool to) => locked = to;
    private void Update()
    {
        if (!locked)
            SetPosition();
        
        
        
    }
    void SetPosition()
    {
        if (PlayerInput.currentControlScheme == keyboardScheme)
        {
            transform.position = pos;
        }
        else
        {
            
            Vector3 m = _controls.Player.Look.ReadValue<Vector2>() * controllerSensitivity; // read value
            Vector3 v = transform.position + m; // predicted
            
            if (v.x < Screen.width && v.x > 0)
                transform.position = new Vector3(v.x, transform.position.y, 0);

            if (v.y < Screen.height && v.y > 0)
                transform.position = new Vector3(transform.position.x, v.y, 0);
            
        }

        Position = transform.position;
    }
    //public void 
    public void ShowReticleOverride(bool to)
    {
        isShown = to;
        SetLock(!isShown);
        animator.SetBool(appearString, isShown);
        if (isHovering && useIconBool)
            animator.SetBool(iconString, isShown);
    }
    public void ShowReticle()
    {
        // If the hover is visable then turn it off, will this affect the state of reticle hover?
        // If so then change isHover bool
        isShown = !isShown;
        SetLock(!isShown);
        animator.SetBool(appearString, isShown);
        if (isHovering && useIconBool)
            animator.SetBool(iconString, isShown);
        //Debug.LogWarning("Setting reticle to: " + isShown);
    }
    public void HoverReticle(bool to)
    {
        isHovering = to;
        if(isHovering && isShown)
            SoundManager.instance.PlaySFX(hoverSound);
        animator.SetBool(hoverString, isHovering);
        if (useIconBool)
            animator.SetBool(iconString, isHovering);
    }
    public void SetOptionals()
    {
        
    }
    //public static void ShowOrHide() => ShowHideReticle?.Invoke();
    //public static void SetHover() => SetReticleHover?.Invoke();
}
