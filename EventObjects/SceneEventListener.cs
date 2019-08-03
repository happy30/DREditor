using System.Collections;
using System.Collections.Generic;
using EventObjects;
using UnityEngine;
using UnityEngine.Events;

public class SceneEventListener : MonoBehaviour
{
    public SceneEvent Event;
    public UnityEvent Response;

    public BoolWithEvent Condition;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised()
    {
        if (Condition != null)
        {
            if (!Condition.Value) return;
        }
        Response.Invoke();
    }
    
}
