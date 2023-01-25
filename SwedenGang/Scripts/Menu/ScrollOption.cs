//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Put this on the parent of made UI options
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ScrollOption : MonoBehaviour
{
    public RectTransform rectTransform = null;
    public Image cell = null;
    public TextMeshProUGUI text = null;
    public Selectable selectable = null;

    public float GetCellSize() => rectTransform.rect.height;
    public void EnableOption()
    {
        selectable.interactable = true;
        cell.enabled = true;
    }
    public void DisableOption()
    {
        selectable.interactable = false;
        cell.enabled = false;
        text.text = "";
    }
}
