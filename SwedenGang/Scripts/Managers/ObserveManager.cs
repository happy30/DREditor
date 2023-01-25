//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
//Shader That made this possible + Idea by LeoTheDev
using DREditor.EventObjects;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;
using static UnityEngine.InputSystem.InputAction;

/// <summary>
/// Manager that handles the outlining of selectable DR objects
/// Intended to operate like DR V3's Observe mode. | Requires RaycastReticle, DialogueAssetReader
/// </summary>
public class ObserveManager : MonoBehaviour
{
    /* This requires a URP Forward renderer Render Object that will render every layer
     * with the material specified on that render object. 
     * 
     * On start up, disables your selection ability
     * On select while in Observe mode, turns off observe mode and the asset reader waits
     * until the transition is finished
     * On Leave Ask turns off observe mode same as above
     */
    [SerializeField] BoolWithEvent inDialogue = null;
    [SerializeField] BoolWithEvent inTPFD = null;
    [SerializeField] BoolWithEvent inMenu = null;
    [SerializeField] RenderObjects renderObject = null;
    [SerializeField] ForwardRendererData rendererData = null;
    [SerializeField] float fadeToTime = 0.5f;
    [SerializeField] float fadeOutTime = 0.5f;
    bool observing = false;
    public static bool ChangingObserve = false;
    public static bool CanChange = true;
#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
    private void Awake()
    {
        _controls = new DRControls();
    }
#endif
    private void Start()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Player.ObserveMode.started += CallObserve;
#endif

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += ModeChanged;
#endif

        DialogueAssetReader.OnDialogueStart += ObserveCheck;
        UIHandler.ToTitle += SetOff;
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
    void CallObserve(CallbackContext context)
    {
        if (GameManager.instance.currentMode == GameManager.Mode.Trial)
            return;
        //Debug.Log(!RoomLoader.instance.inLoading && !inDialogue.Value && inTPFD.Value);
        if (!Door.inLeaveProcess && !RoomLoader.instance.inLoading.Value && !inDialogue.Value && inTPFD.Value && !inMenu.Value
            && !ChangingObserve && CanChange)
        {
            RaycastReticle.canSelect = false;
            observing = !observing;
            
            StartCoroutine(ObserveOn(observing));
        }
    }
    void ObserveCheck()
    {
        if (observing)
        {
            observing = !observing;
            StartCoroutine(ObserveOn(observing));
            if(Door.inLeaveProcess)
                DialogueAssetReader.OnDialogueEnd += FixRetIssue;
        }
    }
    void FixRetIssue()
    {
        DialogueAssetReader.OnDialogueEnd -= FixRetIssue;
        RaycastReticle.canSelect = true;
    }
    
    void SetOff()
    {
        DialogueAssetReader.OnDialogueEnd -= FixRetIssue;
        SetObserve(false);
    }
    public void SetObserve(bool to)
    {
        renderObject.SetActive(to);
        if (to)
            rendererData.opaqueLayerMask &= ~1;
        else
            rendererData.opaqueLayerMask |= 1;
        if (to)
            SoundManager.instance.PlayObserve();
        else
            SoundManager.instance.StopObserveSFX();
        observing = to;
    }
    IEnumerator ObserveOn(bool to)
    {
        Debug.LogWarning("Observe Called " + to);
        ChangingObserve = true;
        if(!GameSaver.LoadingFile)
            GlobalFade.instance.FadeTo(fadeToTime);

        yield return new WaitForSeconds(fadeToTime);
        renderObject.SetActive(to);
        if(to)
            rendererData.opaqueLayerMask &= ~1;
        else
            rendererData.opaqueLayerMask |= 1;
        if (!GameSaver.LoadingFile)
            GlobalFade.instance.FadeOut(fadeOutTime);
        yield return new WaitForSeconds(fadeOutTime);
        if(!inDialogue.Value)
            RaycastReticle.canSelect = true;
        if (to)
            SoundManager.instance.PlayObserve();
        else
            SoundManager.instance.StopObserveSFX();
        ChangingObserve = false;
        yield break;
    }

#if UNITY_EDITOR
    void ModeChanged(PlayModeStateChange change)
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode &&
             EditorApplication.isPlaying)
        {
            if (renderObject.isActive)
                renderObject.SetActive(false);
            rendererData.opaqueLayerMask |= 1;
        }
    }
#endif
}
