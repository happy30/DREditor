//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Subsequent that uses the Scriptable Object as a SceneEvent and the string for the
/// name of the character's Intro to use to call an character intro animation
/// </summary>
public class CharIntroSubsequent : SubsequentBase, ISubsequent
{
    [Header("Put SceneEvent here or in the ScriptableObject Section of the Subsequent" +
        "The same for the string Name of the character.")]
    [SerializeField] SceneEvent SceneEvent = null;
    [SerializeField] string CharacterName;
    public void Load(object ob)
    {
        Subsequent = (Subsequent)Convert.ChangeType(ob, typeof(Subsequent));
        SceneEvent = (SceneEvent)Subsequent.ScriptableObject;
        if (GameSaver.LoadingFile)
            Call();
    }

    object ITrack.Save()
    {
        Subsequent s = (Subsequent)Subsequent.Clone();
        s.Type = GetType().ToString();
        if (s.ScriptableObject == null)
            s.ScriptableObject = SceneEvent;
        
        return s;
    }

    public void Call()
    {
        SceneEventListener listen = gameObject.AddComponent<SceneEventListener>();
        listen.Event = (SceneEvent)Subsequent.ScriptableObject;
        listen.Event.RegisterListener(listen);
        //Debug.Log(listen.Response == null);
        listen.Response = new UnityEvent();
        //Debug.Log(listen.Response == null);
        listen.Response.AddListener(Intro);
        
    }
    public void Intro()
    {
        Debug.Log("Intro Was Called");
        CharIntroAnim.instance.CallIntro(Subsequent.String);
    }
}
