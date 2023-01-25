//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
/// <summary>
/// Maybe split this into two separate classes?
/// </summary>
public class UIDisplayChanger : MonoBehaviour
{
    [SerializeField] bool enableMode = false;
    [SerializeField] Image image = null;
    private Sprite initialImage = null;
    [SerializeField] Sprite selectImage = null;
    [SerializeField] TextMeshProUGUI tmpText = null;
    [SerializeField] bool useInitial = false;
    [SerializeField] Color initialText;
    [SerializeField] Color selectText;
    // IF YOUR UI TOGGLES ARE SETTING TO ON WHEN YOU OPEN THE OPTIONS MENU
    // CHECK TO MAKE SURE THE FIRST OBJECT IS IT'S DEFAULT SETTING
    private void Awake()
    {
        if (!image)
            image = GetComponent<Image>();
        if (!enableMode && image)
        {
            initialImage = image.sprite;
        }
        if (tmpText && !useInitial)
        {
            initialText = tmpText.color;
        }
    }
    public void Select()
    {
        //Debug.LogWarning("Select Called on " + gameObject.name);
        if (enableMode && !image.enabled)
            image.enabled = true;
        if (!enableMode && image)
        {
            image.sprite = selectImage;
        }
        if (tmpText)
        {
            tmpText.color = selectText;
        }
        
    }

    public void Deselect()
    {
        if (enableMode && image.enabled)
            image.enabled = false;
        if (!enableMode && image)
        {
            image.sprite = initialImage;
        }
        if (tmpText)
        {
            tmpText.color = initialText;
        }
    }
}
