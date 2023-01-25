using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CAReticle : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator anim;
    [SerializeField] List<Collider2D> currentColls = new List<Collider2D>();
    [SerializeField] Vector2 vectorMove;
    //[SerializeField] string keyboardScheme = "KeyboardMouse";
    public QuestionPanel selectedPanel;
    public bool allowInput = false;

    [SerializeField] AudioClip panelHover = null;


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
    private void Start()
    {
        StartCoroutine(WaitForStart());
    }

    IEnumerator WaitForStart()
    {
        yield return new WaitForSeconds(1);
        allowInput = true; 
    }

    public void GetMovement(InputAction.CallbackContext context)
    {
        if (!context.performed || !allowInput)
            return;
        vectorMove = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (allowInput)
        {
            Vector3 m = _controls.Player.Look.ReadValue<Vector2>(); // read value
            Vector2 x = (Vector2)m /2;
            rb.MovePosition(rb.position + x);
            /*
            if (GameManager.instance.GetInput().currentControlScheme == keyboardScheme)
            {
                
            }
            else
            {
                //Debug.LogWarning("This is a test");
                vectorMove = vectorMove * 9 * Time.deltaTime;
                rb.MovePosition(rb.position + vectorMove);
            }
            */
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (allowInput) //stops sfx from playing during the fade in at the start
        {
            selectedPanel = collision.gameObject.GetComponent<QuestionPanel>();
            currentColls.Add(collision);

            anim.SetBool("Hover", true);
            SoundManager.instance.PlaySFX(panelHover);
            
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        currentColls.Remove(collision);
        if (currentColls.Count == 0) 
        {
            anim.SetBool("Hover", false);
            selectedPanel = null;
        }
        else
            selectedPanel = currentColls[0].gameObject.GetComponent<QuestionPanel>(); //if it's colliding with 2 at the same time and leaving the collision of one of them. Otherwise the selection would be wrongly set to null.
    }

    public void EnableMech() //Enable the reticle mechanically (hitbox, input) Visuals are unaffected
    {
        allowInput = true;
        this.gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    public void DisableMech()
    {
        allowInput = false;
        this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }
}
