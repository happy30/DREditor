using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour
{
    public RectTransform Target;
    public int ScrollSpeed;
    protected ScrollRect scrollRect;
    protected RectTransform contentPanel;

    protected int LineCount;
    protected Button LastLine;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        contentPanel = scrollRect.content;
    }

    void OnEnable()
    {
        LineCount = contentPanel.transform.childCount;
        if (LineCount != 0)
        {
            LastLine = contentPanel.GetChild(LineCount - 1).GetComponent<Button>();
            EventSystem.current.SetSelectedGameObject(LastLine.gameObject);
        }
    }

    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null) { 
            Target = EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>();
            SnapTo(Target);
        }
    }

    public void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        contentPanel.anchoredPosition = Vector2.Lerp(contentPanel.anchoredPosition, (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position) - (Vector2)scrollRect.transform.InverseTransformPoint(target.position), Time.unscaledDeltaTime * ScrollSpeed);
    }
}
