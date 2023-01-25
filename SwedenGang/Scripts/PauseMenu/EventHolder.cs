using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventHolder : MonoBehaviour
{
    [SerializeField] UnityEvent StartEvent;
    [SerializeField] UnityEvent EndEvent;
    [Tooltip("For enabling or disabling a canvas using public function")]
    [SerializeField] Canvas Canvas = null;
    [SerializeField] Canvas CanvasT = null;
    public void CallStartEvent()
    {
        StartEvent.Invoke();
    }
    public void CallEndEvent()
    {
        EndEvent.Invoke();
    }
    public enum To
    {
        Enabled, Disabled
    }
    public void EnableCanvas(To to)
    {
        Canvas.enabled = to == To.Enabled;
    }
    public void EnableCanvasT(To to)
    {
        CanvasT.enabled = to == To.Enabled;
    }
}
