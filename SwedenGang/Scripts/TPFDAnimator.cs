//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
/// <summary>
/// Placed on the animator object that animates a 2.5D Room 
/// If one isn't present in a tpfd scene nothing should happen
/// Make an animation with 3 animation events, 2 to start and end input
/// the last to call EndAnimation
/// </summary>
public class TPFDAnimator : MonoBehaviour
{
    public static bool RoomAnimating = false;
    public static TPFDAnimator instance = null;
    PlayerInput PlayerInput => GameManager.instance.GetInput();
    [Header("String name of the animation clip for animating the room.")]
    [SerializeField] string clipName;
    private Animator roomAnimator = null;
    private void Start()
    {
        instance = this;
        roomAnimator = GetComponent<Animator>();
    }
    public void StartAnimation()
    {
        roomAnimator.Play(clipName);
    }
    public void EndAnimation()
    {
        RoomAnimating = false;
    }
    public void AddSpeedInput()
    {
        PlayerInput.actions["FastForward"].performed += SpeedUp;
        PlayerInput.actions["FastForward"].canceled += SlowDown;
    }
    public void RemoveSpeedInput()
    {
        PlayerInput.actions["FastForward"].performed -= SpeedUp;
        PlayerInput.actions["FastForward"].canceled -= SlowDown;
        Time.timeScale = 1;
    }
    void SpeedUp(CallbackContext context) => Time.timeScale = 2;
    void SlowDown(CallbackContext context) => Time.timeScale = 1;
    private void OnDestroy()
    {
        instance = null;
    }
}
