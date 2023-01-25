//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static UnityEngine.InputSystem.InputAction;

public class TrialTutorialManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] bool debugMode = false;
    [SerializeField] TrialTutorialAsset debugAsset = null;

    [Header("Main Components")]
    [SerializeField] float totalFadeTime = 0.5f;
    [SerializeField] RawImage title = null;
    [SerializeField] RawImage page = null;
    //[SerializeField] RawImage subPage = null;
    [SerializeField] LayoutGroup layoutGroup = null;
    [SerializeField] GameObject circlePrefab = null;
    [SerializeField] Sprite activeCircle = null;
    [SerializeField] Sprite inactiveCircle = null;
    [SerializeField] Canvas canvas = null;
    [SerializeField] Animator animator = null;
    [SerializeField] int animatorLayer = 0;
    [SerializeField] string showString = "Show";
    [SerializeField] string hideString = "Hide";

    [Header("Optional")]
    [SerializeField] Transform rightArrow = null;


    int currentCircleIndex = 0;
    List<Image> circles = new List<Image>();
    List<Texture2D> pages = null;
    bool active = false;
    DRControls _controls;
    private void Awake()
    {
        _controls = new DRControls();
    }
    private void Start()
    {
        if (debugMode && debugAsset != null)
            EvaluateTutorial(debugAsset);
        DialogueEventSystem.StartListening("TrialTutorial", EvaluateTutorial);
    }
    private void OnEnable()
    {
        _controls.Enable();
    }
    private void OnDisable()
    {
        _controls.Player.Move.started -= ChangePage;
        _controls.Disable();
        DialogueEventSystem.StopListening("TrialTutorial", EvaluateTutorial);
    }
    void EvaluateTutorial(object o = null)
    {
        if (o == null && active)
        {
            TurnOff();
            return;
        }
        else if (o == null && !active)
            return;
        TurnOn((TrialTutorialAsset)o);
    }
    void TurnOn(TrialTutorialAsset asset)
    {
        active = true;
        Initialize(asset);
        if (animator)
        {
            animator.Play(showString);
            page.DOFade(1, 1);
        }
        else
        {
            page.DOFade(1, 0);
        }
        canvas.enabled = true;
        _controls.Player.Move.started += ChangePage;
    }
    void TurnOff()
    {
        // If animator use hiding 
        // When done then turn off the canvas and clear 
        StopAllCoroutines();
        _controls.Player.Move.started -= ChangePage;
        StartCoroutine(TurnOffRoutine());
    }
    IEnumerator TurnOffRoutine()
    {

        if (animator)
        {
            animator.Play(hideString);
            yield return new WaitForSeconds(Time.deltaTime);
            float normalizedTime = animator.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime;
            float aLength = animator.GetCurrentAnimatorStateInfo(animatorLayer).length;
            //Debug.LogWarning("Animator " + aLength);
            //animator.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime = 0;
            page.DOFade(0, 1);
            if (float.IsPositiveInfinity(aLength))
            {
                Debug.Log("Was Infin");
                yield return new WaitUntil(() => !float.IsPositiveInfinity(animator.GetCurrentAnimatorStateInfo(animatorLayer).length));
            }
            if (!float.IsPositiveInfinity(aLength))
            {
                Debug.Log("Wasnt Infin");
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            }
            Debug.Log("End if");
        }
        else
        {
            Debug.Log("FUCK");
            page.DOFade(0, 0);
        }
        

        Clear();
        currentCircleIndex = 0;
        active = false;
        //Debug.LogWarning("Turn Off Routine Finished");
        yield break;
    }
    void Initialize(TrialTutorialAsset asset)
    {
        title.texture = asset.title;
        pages = asset.pages;
        if (pages.Count > 0)
            page.texture = pages[0];
        InitializeCircles();
    }
    void InitializeCircles()
    {
        for(int i = 0; i < pages.Count; i++)
        {
            GameObject g = Instantiate(circlePrefab);
            g.transform.SetParent(layoutGroup.transform, false);
            circles.Add(g.GetComponent<Image>());
            if (i == 0)
                circles[i].sprite = activeCircle;
            circles[i].transform.SetAsLastSibling();
        }
        if (rightArrow != null)
            rightArrow.SetAsLastSibling();
    }
    void ChangePage(CallbackContext ctx)
    {
        if (Time.timeScale == 0 || pages.Count == 1)
            return;
        Vector2 read = _controls.Player.Move.ReadValue<Vector2>();
        _controls.Player.Move.started -= ChangePage;
        int newIndex = read.x == 1 ? currentCircleIndex + 1 : currentCircleIndex - 1;
        if(newIndex < 0)
            newIndex = pages.Count - 1;
        if (newIndex == pages.Count)
            newIndex = 0;
        //Debug.LogWarning("Change Page Called");
        // Swap the page animation
        StartCoroutine(SwapRoutine(newIndex));
    }
    IEnumerator SwapRoutine(int newIndex)
    {
        float time = totalFadeTime / 2;
        SwapCircle(newIndex);
        page.DOFade(0, time);
        yield return new WaitForSeconds(time);
        page.texture = pages[newIndex];
        page.DOFade(1, time);
        yield return new WaitForSeconds(time);
        currentCircleIndex = newIndex;
        _controls.Player.Move.started += ChangePage;
        yield break;
    }
    void SwapCircle(int newIndex)
    {
        circles[currentCircleIndex].sprite = inactiveCircle;
        circles[newIndex].sprite = activeCircle;
    }

    void Clear()
    {
        canvas.enabled = false;
        title.texture = null;
        pages = null;
        foreach (Image i in circles)
            Destroy(i.gameObject);
        circles.Clear();
    }
}
