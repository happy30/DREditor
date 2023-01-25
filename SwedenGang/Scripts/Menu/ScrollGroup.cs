//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollGroup : MenuGroup
{
    /* The mask recttransform
     * The list of each object
     * The width or height of one cell slot
     * Keep in mind that for SetContainerBounds to work properly the canvas needs to be enabled
     */
    //[SerializeField] RectTransform scrollMask = null;
    [SerializeField] RectTransform container = null;
    [SerializeField] GridLayoutGroup layout = null;
    [SerializeField] float cellSize = 0f;
    public List<ScrollOption> scrollOptions = new List<ScrollOption>();
    float midPoint;
    float top;
    float bottom;
    float yScale;
    private bool boundsSet = false;
    public override void Start()
    {
        base.Start();
        yScale = GetComponentInParent<Canvas>().scaleFactor;
        //Debug.Log(yScale);
        //SetContainerBounds();
        SetCellSize();
        //Debug.LogWarning("Bounds Set");
        StartEvents.AddListener(SetContainerBounds);
        StartEvents.AddListener(AddScroll);
        
        EndEvents.AddListener(RemoveScroll);
    }
    private int resolutionX;
    private int resolutionY;

    private void Awake()
    {
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
        resolutionX = Screen.width;
        resolutionY = Screen.height;
    }
    bool notCentered;
    Vector3 origin;
    private void Update()
    {
        if (resolutionX == Screen.width && resolutionY == Screen.height) return;

        // do stuff
        if (isActive)
        {
            notCentered = container.localPosition != Vector3.zero;
            if (notCentered)
                origin = container.localPosition;
            Debug.LogWarning("SCREEN WIDTH CHANGED AND SET BOUNDS");
            container.localPosition = Vector2.zero;
            
            StartCoroutine(SetBounds());

            //midPoint = container.position.y;
            //top = midPoint + container.rect.height * yScale / 2;
            //bottom = midPoint - container.rect.height * yScale / 2;

            //Debug.Log("Midpoint: " + midPoint);
            //Debug.LogWarning("Top: " + top);
            //Debug.LogWarning("Bottom: " + bottom);
        }

        resolutionX = Screen.width;
        resolutionY = Screen.height;
    }
    public void SetContainerBounds()
    {
        if (boundsSet)
            return;
        StartCoroutine(SetBounds());

    }
    IEnumerator SetBounds() // TO-DO: learn how to disable input and disable input until canvas loads
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        yield return new WaitUntil(() => canvas.enabled == true);
        Canvas.ForceUpdateCanvases();
        //Debug.LogWarning("ContainerBounds Called on " + gameObject.name);
        yScale = GetComponentInParent<Canvas>().scaleFactor;
        SetCellSize();
        midPoint = container.position.y;
        //Debug.Log("Midpoint: " + midPoint);
        //Debug.Log(container.rect.yMax);
        //Debug.Log(container.rect.yMin);
        //Debug.Log(container.rect.height);
        //Debug.Log(yScale);
        top = midPoint + container.rect.height * yScale / 2;

        bottom = midPoint - container.rect.height * yScale / 2;
        //Debug.LogWarning("Top: " + top);
        //Debug.LogWarning("Bottom: " + bottom);
        boundsSet = true;
        //top *= yScale;
        //bottom *= yScale;
        if (notCentered)
            container.localPosition = origin;
        yield break;
    }
    public void SetCellSize()
    {
        if (!(layout != null))
            Debug.LogError("Scroll Option List doesn't have any content!");

        cellSize = layout.cellSize.y;
        cellSize *= yScale;
        //Debug.Log("Cell Size " + cellSize);
    }
    public void AddScroll() // called when Revealed
    {
        UIHandler.instance.OnChange.AddListener(Scroll);
    }
    public void RemoveScroll() // called when Hidden
    {
        UIHandler.instance.OnChange.RemoveListener(Scroll);
    }
    
    void Scroll() => Check();
    void Check()
    {
        float yPos = EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>().position.y;

        // check if outside the bounds of the mask

        
        //Debug.Log(yPos + " " + midPoint + " " + top + " " + bottom + " ");
        if (yPos <= bottom)
            ScrollAnim(false);
        else if (yPos >= top)
            ScrollAnim(true);
    }

    void ScrollAnim(bool up)
    {
        //Debug.Log("Called Scroll " + cellSize);
        Vector3 c = container.position;
        if (up)
            c.y -= cellSize;
        else
            c.y += cellSize;

        container.position = c;
    }
}
