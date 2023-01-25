//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// A Subsequent for Locking down an entire area (Global Lock)
/// </summary>
public class LockSubsequent : SubsequentBase, ISubsequent
{
    ProgressionManager manager = ProgressionManager.instance;
    [Header("Keep in mind this is a GLOBAL Lock, so until this is done with the Clear Locks option through dialogue" +
        " the player won't be able to leave the area.")]
    [SerializeField] Dialogue lockDialogue = null;
    void ITrack.Load(object ob)
    {
        Subsequent = (Subsequent)Convert.ChangeType(ob, typeof(Subsequent));
        lockDialogue = (Dialogue)Subsequent.ScriptableObject;
        if (GetActuator().Actuator.Triggered && !Subsequent.Locked)
        {
            InnerCall();
        }
        //else if (manager.GlobalLock)
            //manager.GlobalLock = null;
        // Above Else If statement may need to be changed if there's a problem loading a file
        // Where the room has two global locks, one triggered, one not
    }
    object ITrack.Save()
    {
        Subsequent s = (Subsequent)Subsequent.Clone();
        s.Type = GetType().ToString();
        //Debug.Log(s.Type);
        s.ScriptableObject = lockDialogue;
        return s;
    }
    void ISubsequent.Call()
    {
        //manager.GlobalLock = lockDialogue;
        // Set the universal lock dialogue
        //Door.GlobalLockLifted += LockLifted;
    }
    void InnerCall()
    {
        //manager.GlobalLock = lockDialogue;
        // Set the universal lock dialogue
        Door.GlobalLockLifted += LockLifted;
    }
    // Note: Subsequent.Locked acts just like Actuator.Triggered here
    void LockLifted() => Subsequent.Locked = true;
    private void OnDestroy()
    {
        //manager.GlobalLock = null;
        //Door.GlobalLockLifted -= LockLifted;
    }

}
