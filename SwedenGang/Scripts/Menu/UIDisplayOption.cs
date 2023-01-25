//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIDisplayOption : UIBehaviour, ISelectHandler, IDeselectHandler
{
    public UnityEvent Selection;
    public UnityEvent Deselection;
    public void InvokeSelect() => Selection?.Invoke();
    public void InvokeDeselect() => Deselection?.Invoke();
    public virtual void OnSelect(BaseEventData eventData)
    {
        Selection?.Invoke();
    }
    public virtual void OnDeselect(BaseEventData eventData)
    {
        Deselection?.Invoke();
    }
}
