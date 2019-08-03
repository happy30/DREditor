using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Scene Event", fileName = "New Scene Event")]
public class SceneEvent : ScriptableObject
{
    private List<SceneEventListener> _listeners = new List<SceneEventListener>();

    public void Raise()
    {
        for (var i = _listeners.Count - 1; i >= 0; i--)
        {
            _listeners[i].OnEventRaised();
        }
        
    }

    public void RegisterListener(SceneEventListener listener)
    {
        _listeners.Add(listener);
    }

    public void UnregisterListener(SceneEventListener listener)
    {
        _listeners.Remove(listener);
    }
}
