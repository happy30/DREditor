//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MenuGroup : MonoBehaviour
{
    public static bool CanSelect = true;
    [SerializeField] Canvas canvas = null;
    
    public List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
    public List<Selectable> butts = new List<Selectable>();
    public VerticalLayoutGroup layoutGroup = null;
    public Animator animator = null;
    [SerializeField] int animatorLayer = 0;
    public float Spacing;
    public GameObject first = null;
    public MenuGroup backGroup = null;
    public bool isActive = false;

    public UnityEvent BeforeStart = null;
    public UnityEvent StartEvents = null;
    public UnityEvent EndEvents = null;
    [HideInInspector] public UnityEvent OnBack = null;
    [Header("Optional")]
    [SerializeField] bool dontUseLastSelected = false;
    [SerializeField] bool individualAnimate = false;
    [SerializeField] float asyncTime = 0;
    [SerializeField] GameObject LastSelected = null;
    [SerializeField] bool keepLastSelectedOnReset = false;
    [SerializeField] bool waitForEnd = false;
    [Tooltip("This is for if you want the canvas to still be enabled when Hidegroup happens")]
    [SerializeField] bool keepCanvas = false;
    [SerializeField] bool skipSelection = false;
    [SerializeField] bool useTrueWaitForEnd = false;
    List<Animator> animators = new List<Animator>();
    public delegate void MenuDel(MenuGroup group);
    public static event MenuDel GroupFinished;
    protected DRControls _controls;

    private GameObject tempFirst = null;

    private void Awake()
    {
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
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
    public virtual void Start()
    {
        UIHandler.ToTitle += ResetGroup;
        if(butts.Count == 0)
        {
            foreach (TextMeshProUGUI t in texts)
            {
                butts.Add(t.GetComponent<Button>());
            }
            foreach (Selectable b in butts)
            {
                b.enabled = false;
            }
            
        }
        else
        {
            if (!isActive)
            {
                foreach (Selectable b in butts)
                {
                    b.enabled = false;
                }
            }
            if (individualAnimate)
                foreach (Selectable b in butts)
                    animators.Add(b.GetComponent<Animator>());
        }
        
    }
    public void EnableButtons(bool t)
    {
        foreach (Selectable b in butts)
        {
            b.enabled = t;
        }
    }
    public void DisableMenu()
    {
        this.enabled = false;
    }
    protected void ResetGroup()
    {
        if (!keepLastSelectedOnReset)
            LastSelected = null;
        if (isActive)
        {
            HideProcess(0);
        }
        RemoveBackInput();
    }
    public static bool Changing = false;
    public void ChangeGroup(MenuGroup group)
    {
        if (Changing)
            return;
        Changing = true;
        //Debug.LogWarning("Group: " + gameObject.name + " is changing to Group: " + group.gameObject.name);
        StartCoroutine(Change(group));
    }
    IEnumerator Change(MenuGroup group)
    {
        LastSelected = EventSystem.current.currentSelectedGameObject;
        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        EventSystem.current.SetSelectedGameObject(null);
        Hide();
        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        group.Reveal();
        SoundManager.instance.PlaySubmit();
        yield break;
    }
    Tweener tweener;
    IEnumerator IndividualAnim(string to)
    {
        foreach(Animator a in animators)
        {
            if (a.gameObject.activeSelf)
            {
                a.ResetTrigger("Hide");
                a.SetTrigger(to);
                yield return new WaitForSecondsRealtime(asyncTime);
            }
        }
        yield break;
    }
    public virtual void Reveal()
    {
        //Debug.Log("Revealing Group: " + gameObject.name);
        if (isActive)
        {
            //Debug.LogWarning("Was Active");
            return;
        }
        
        //MenuGroup.GroupFinished += Check;
        if (animator != null)
        {
            //Debug.Log("Setting Triggers");
            animator.SetTrigger("Reveal");
            animator.ResetTrigger("Hide");
        }
        if (canvas)
            canvas.enabled = true;
        foreach (TextMeshProUGUI text in texts)
        {
            text.DOKill();
            text.DOFade(1, 1).SetUpdate(true);
        }
        if (layoutGroup)
        {
            tweener = DOTween.To(() => layoutGroup.spacing, x => layoutGroup.spacing = x, Spacing, 1).SetUpdate(true);
        }

        

        foreach(Selectable b in butts)
        {
            b.enabled = true;
        }
        if (individualAnimate)
            StartCoroutine(IndividualAnim("Show"));
        a = StartCoroutine(Active());
        
    }
    Coroutine a;
    IEnumerator Active()
    {
        //Debug.LogWarning("Active Started");
        BeforeStart?.Invoke();
        if (animator)
        {
            //Debug.LogWarning("Animator Found");
            //Debug.LogWarning("Animator Found " + animator.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime);
            float normalizedTime = animator.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime;
            float aLength = animator.GetCurrentAnimatorStateInfo(animatorLayer).length;
            //Debug.LogWarning("Animator " + aLength);
            //animator.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime = 0;
            if (waitForEnd && !float.IsPositiveInfinity(aLength))
            {
                yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime >
                1.0f);
                //yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(animatorLayer).length >
                //animator.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime);
            }
            else if (!float.IsPositiveInfinity(aLength))
            {
                yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(animatorLayer).length >
                animator.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime);
            }
            if (useTrueWaitForEnd)
            {
                if (float.IsPositiveInfinity(aLength))
                {
                    yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(animatorLayer).length !=
                    float.PositiveInfinity);
                }
                yield return new WaitForSeconds(Time.deltaTime);
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            }
            //Debug.LogWarning("Animator Finished");
        }

        //_controls.Disable();

        
        //Debug.Log("Starting Evaluate Selection");
        if(!skipSelection)
            yield return StartCoroutine(EvaluateSelection());
        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime * 2);
        //Debug.Log("Evaluate Selection Ended");
        AddBackInput();

        //Debug.LogWarning("Start Event Call for: " + gameObject.name);
        StartEvents?.Invoke();
        //Debug.Log("Revealed: " + gameObject.name);
        isActive = true;
        //Debug.LogWarning("Ended " + gameObject.name + "'s Active");
        Changing = false;

        //StartCoroutine(ControlTimer());
        yield break;
    }
    /*
    IEnumerator ControlTimer()
    {
        yield return new WaitForSeconds(0.2f);
        _controls.Enable();
        yield break;
    }*/
    public virtual void Hide()
    {
        HideProcess(0.5f);
    }
    void HideProcess(float duration)
    {
        if (individualAnimate)
            StartCoroutine(IndividualAnim("Hide"));

        if (animator != null)
        {
            animator.SetTrigger("Hide");
            animator.ResetTrigger("Reveal");
        }
        foreach (TextMeshProUGUI text in texts)
        {
            text.DOKill();
            text.DOFade(0, duration).SetUpdate(true);
        }
        if (layoutGroup)
        {
            tweener.Kill();
            tweener = DOTween.To(() => layoutGroup.spacing, x => layoutGroup.spacing = x, 0, duration).SetUpdate(true);
        }

        RemoveBackInput();

        foreach (Selectable b in butts)
        {
            b.enabled = false;
        }

        EndEvents?.Invoke();
        //Debug.LogWarning("Called End Events for: " + gameObject.name);

        if (canvas && !keepCanvas)
            canvas.enabled = false;
        if(a != null)
            StopCoroutine(a);
        isActive = false;
        //Debug.LogWarning("No longer active");
    }

    #region Inputs
    public bool back = false;
    public virtual void BackGroup(InputAction.CallbackContext context)
    {
        //Debug.LogWarning("Calling BackGroup for : " + gameObject.name);
        //LastSelected = null;
        if (backGroup)
        {
            ChangeGroup(backGroup);
            SoundManager.instance.PlayCancel();
        }

        OnBack?.Invoke();
    }
    public virtual void RemoveBackInput()
    {
        StartCoroutine(RemoveBack());
    }
    IEnumerator RemoveBack()
    {
        yield return new WaitForEndOfFrame();
        if (back) // was backGroup
        {
            back = false;
            _controls.UI.Cancel.started -= BackGroup;
            //Debug.LogWarning("Removed Back Input for: " + gameObject.name);
        }
        yield break;
    }
    public virtual void AddBackInput()
    {
        StartCoroutine(AddBack());
    }
    IEnumerator AddBack()
    {
        yield return new WaitForEndOfFrame();
        if (!back) // was backGroup
        {
            back = true;
            _controls.UI.Cancel.started += BackGroup;
            //Debug.LogWarning("Added Back Input for: " + gameObject.name);
        }
        yield break;
    }
    #endregion

    public void SetLastSelected(GameObject gameObject) => LastSelected = gameObject;
    public void EvaluateSelect()
    {
        
        StartCoroutine(EvaluateSelection());
    }
    IEnumerator EvaluateSelection()
    {
        if (!CanSelect)
        {
            Debug.Log("Can Select is false for MenuGroup: " + gameObject.name);
            yield break;
        }
        GameObject o = GetSelection();
        //Debug.LogWarning("Evaluating Selection");
        //Debug.LogWarning("Selection is " + o.name);
        if (!o)
            Debug.LogWarning("Get Selection spat out null");
        Animator a = null;
        if (individualAnimate)
            a = o.GetComponent<Animator>();

        if (a != null)
        {
            //Debug.LogWarning("Animator Found " + a.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime);
            
            if (a.GetCurrentAnimatorStateInfo(animatorLayer).length != float.PositiveInfinity &&
                !(a.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime > 1))
                yield return new WaitUntil(() => a.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime < 1.0f);
            //Debug.LogWarning("Animator Finished");
        }
        
        SetSelection(o);
        GroupFinished?.Invoke(this);
        
        yield break;
    }
    public void QuickSelection()
    {
        GameObject o = GetSelection();
        SetSelection(o);
    }
    
    GameObject GetSelection()
    {
        if (Check(LastSelected) && InButts(LastSelected) && !dontUseLastSelected)
        {
            //Debug.Log("GetSelection Returned LastSelected!");
            return LastSelected;
        }
        else
        {
            if (Check(first))
                return first;
            else if (Check(tempFirst))
                return tempFirst;
        }
        foreach(Selectable s in butts)
        {
            if (Check(s.gameObject))
                return s.gameObject;
        }
        Debug.LogWarning("GetSelection Returned Null!");
        return null;
    }
    bool Check(GameObject o) => o != null && o.activeSelf && o.GetComponent<Selectable>().interactable;
    bool InButts(GameObject o)
    {
        foreach(Selectable s in butts)
        {
            if (s.gameObject == o)
                return true;
        }
        return false;
    }
    void SetSelection(GameObject o)
    {
        //Debug.LogWarning("Set Selection to: " + o.name);
        EventSystem.current.SetSelectedGameObject(o);
        if (UIHandler.instance)
        {
            UIHandler.instance.current = o;
        }
        if (o == tempFirst)
        {
            Debug.Log("Chose Temp First");
            tempFirst = null;
        }
    }
    public void SetBackGroup(MenuGroup group)
    {
        backGroup = group;
    }
    public void SetNullBackGroup()
    {
        backGroup = null;
    }
    public void SetTempFirst(GameObject to)
    {
        tempFirst = to;
    }
    public void EnableCanvas(bool to)
    {
        if (canvas)
            canvas.enabled = to;
    }
    private void OnDestroy()
    {
        _controls.UI.Cancel.started -= BackGroup;
        UIHandler.ToTitle -= ResetGroup;
    }
}
