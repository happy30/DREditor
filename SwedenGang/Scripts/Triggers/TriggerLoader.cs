//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Gardens
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class TriggerLoader
{
    // Keep in mind only one object should have an actuator otherwise there will be problems
    public static List<EventTrigger> Save()
    {
        List<EventTrigger> triggers = new List<EventTrigger>();

        var actList = UnityEngine.Object.FindObjectsOfType<ActuatorBase>();
        for(int i = 0; i < actList.Count(); i++)
        {
            EventTrigger trigger = new EventTrigger();
            var a = actList[i].GetComponents<MonoBehaviour>().OfType<IActuator>();
            trigger.Actuator = (Actuator)Convert.ChangeType(a.ElementAt(0).Save(), typeof(Actuator));
            ISubsequent[] subs = actList[i].GetSubsequents();
            for (int j = 0; j < subs.Count(); j++)
            {
                trigger.Subsequents.Add((Subsequent)Convert.ChangeType(subs[j].Save(), typeof(Subsequent)));
            }
            triggers.Add(trigger);
        }

        /*var aa = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().OfType<IActuator>();
        
        var ss = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().OfType<ISubsequent>();
        for(int i = 0; i < aa.Count(); i++)
        {
            EventTrigger trigger = new EventTrigger();
            IActuator a = aa.ElementAt(i);
            ISubsequent s = ss.ElementAt(i);
            trigger.Actuator = (Actuator)Convert.ChangeType(a.Save(), typeof(Actuator));
            trigger.Subsequent = (Subsequent)Convert.ChangeType(s.Save(), typeof(Subsequent));
            triggers.Add(trigger);
        }*/
        
        return triggers;
    }

    public static void Load(List<EventTrigger> Triggers, List<EventTrigger> InstanceTriggers = null)
    {
        if (InstanceTriggers != null) // This is to check whether the save file reversed the instance triggers when saving
        {
            //bool done = false;
            for (int z = 0; z < Triggers.Count; z++) 
            {
                try
                {
                    EventTrigger zBase = Triggers[z];
                    EventTrigger jInstance = InstanceTriggers[z];
                    if (zBase.Subsequents.Count != jInstance.Subsequents.Count)
                    {
                        Debug.Log("CALLED REVERSE");
                        InstanceTriggers.Reverse();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("MINOR ERROR: A BASE OR INSTANCE PIECE OF DATA COULDN'T BE FOUND on  " + z + "\n" + 
                        ex.ToString());
                }
                
            }
            Debug.Log("INSTANCE Trigger is: " + InstanceTriggers.Count);
        }
        
        for (int i = 0; i < Triggers.Count; i++)
        {
            EventTrigger currentTrigger = Triggers[i];
            GameObject g = new GameObject();
            if(InstanceTriggers != null)
            {
                try
                {
                    Debug.Log(InstanceTriggers.Count);
                    Debug.Log(InstanceTriggers[i].ToString());
                    Debug.Log(InstanceTriggers[i].Actuator.Triggered);
                    Debug.Log(InstanceTriggers[i].Subsequents.Count);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("MINOR ERROR: Logging Triggers had an error  on  " + i + "\n" +
                        ex.ToString());
                }
            }
            Type type = Type.GetType(currentTrigger.Actuator.Type);
            g.AddComponent(type);
            var x = g.GetComponents<MonoBehaviour>().OfType<IActuator>();
            foreach (IActuator a in x)
            {
                if(InstanceTriggers != null && InstanceTriggers.Count > 0 && i < InstanceTriggers.Count)
                    a.Load(InstanceTriggers[i].Actuator);
                else
                    a.Load(currentTrigger.Actuator.Clone());
            }

            for (int j = 0; j < currentTrigger.Subsequents.Count; j++)
            {
                Subsequent s = currentTrigger.Subsequents[j];
                Type stype = Type.GetType(s.Type);
                g.AddComponent(stype);
                var iSubs = g.GetComponents<MonoBehaviour>().OfType<ISubsequent>();

                if (InstanceTriggers != null && InstanceTriggers.Count > 0 && i < InstanceTriggers.Count)
                    iSubs.ElementAt(j).Load(Subsequent.MergeInstance((Subsequent)s.Clone(),InstanceTriggers[i].Subsequents[j]));
                else
                    iSubs.ElementAt(j).Load(s.Clone());
                
                
            }

            /*Type stype = Type.GetType(Triggers[i].Subsequent.Type);
            g.AddComponent(stype);
            var sx = g.GetComponents<MonoBehaviour>().OfType<ISubsequent>();
            foreach (ISubsequent s in sx)
            {
                if (InstanceTriggers != null)
                    s.Load(Subsequent.MergeInstance(Triggers[i].Subsequent, InstanceTriggers[i].Subsequent));
                else
                    s.Load(Triggers[i].Subsequent);
            }*/
        }
        
        
        
    }
}
[Serializable]
public class EventTrigger
{
    public Actuator Actuator;
    public List<Subsequent> Subsequents = new List<Subsequent>();
}
/// <summary>
/// How the EventTrigger is Triggered
/// </summary>
[Serializable]
public class Actuator // info here is where Actuator Info will be serialized by the Room Builder
{
    public string Type;
    public bool Triggered = false;
    public Vector3 position;
    public Vector3 HitBoxSize;
    public List<string> stringList = new List<string>();
    public bool boolValue;

    public static Actuator MergeInstance(Actuator a, Actuator i) // Use data of i to apply instance data
    {
        Actuator act = new Actuator();
        act.Type = a.Type;
        act.Triggered = i.Triggered;
        act.position = i.position;
        act.HitBoxSize = i.HitBoxSize;
        act.stringList = i.stringList;
        act.boolValue = i.boolValue;
        return null;
    }
    public object Clone()
    {
        return MemberwiseClone();
    }
    
}
/// <summary>
/// What the Event Trigger actually does
/// </summary>
[Serializable]
public class Subsequent // info here is where Subsequent Info will be serialized by the Room Builder
{
    public string Type;
    public ScriptableObject ScriptableObject;
    public bool Locked = false;
    public string String;
    public Vector3 position;
    public Quaternion rotation;
    public LocalDialogue localDialogue;

    public static Subsequent MergeInstance(Subsequent s, Subsequent i) // Use data of i to apply instance data
    {
        Subsequent sub = new Subsequent();
        sub.Type = s.Type;
        sub.ScriptableObject = s.ScriptableObject;
        sub.Locked = i.Locked;
        sub.String = i.String;
        sub.localDialogue = new LocalDialogue();
        sub.localDialogue.dialogue = s.localDialogue.dialogue;
        sub.localDialogue.dialogueIfFlagNotMet = s.localDialogue.dialogueIfFlagNotMet;
        sub.localDialogue.triggered = i.localDialogue.triggered;
        return sub;
    }
    public object Clone()
    {
        return MemberwiseClone();
    }
}
public class ActuatorBase : MonoBehaviour
{
    public Actuator Actuator;
    public virtual void Call()
    {
        var x = GetComponents<MonoBehaviour>().OfType<ISubsequent>();
        foreach(ISubsequent s in x)
        {
            s.Call();
        }
    }
    public ISubsequent[] GetSubsequents()
    {
        return GetComponents<MonoBehaviour>().OfType<ISubsequent>().ToArray();
    }
}
public class SubsequentBase : MonoBehaviour
{
    public Subsequent Subsequent;
    public ActuatorBase GetActuator()
    {
        return GetComponents<ActuatorBase>()[0];
    }
}
public interface IActuator : ITrack
{

}
public interface ISubsequent : ITrack
{
    public void Call();
}
