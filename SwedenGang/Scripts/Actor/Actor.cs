//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
/// <summary>
/// Requires Dialogue, DialogueAnimConfig, ProgressionManager
/// Actor script for DR Character
/// </summary>
public class Actor : MonoBehaviour, IDialogueHolder
{
    public Transform head = null;
    public Transform body = null;
    public MeshRenderer character = null;
    public MeshRenderer characterb = null;
    public MeshRenderer characterbMask = null; // NSD Only: For character cutout to render on top of the Text
    [HideInInspector] public Dialogue[] Conversation = null;
    public List<LocalDialogue> RoomConvo = new List<LocalDialogue>();
    public SpriteRenderer sprite = null;
    public SpriteRenderer spriteb = null;
    public bool identified = true;
    public string displayName = "";
    [SerializeField] Dialogue DebugNoDialogue = null;
    void HitByRay() // Called by RaycastReticle.cs
    {
        ItemDisplayer.instance.HideName();
        Conversation = new Dialogue[1];
        Conversation[0] = LocalDialogue.EvaluateDialogue(RoomConvo);
        if (!identified)
            identified = true;
        if (Conversation[0] != null)
            DialogueAnimConfig.instance.StartDialogue(this, Conversation);
        else
        {
            Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
            dialogue.Lines.Add(new Line());
            dialogue.Lines[0].Speaker = DialogueAssetReader.instance.database.GetCharacterByContaining(displayName);
            dialogue.Lines[0].Text = "THIS CHARACTER HAS NO DIALOGUE PLUGGED IN DO NOT SAVE ON THIS LINE";
            Conversation[0] = dialogue;
            DebugNoDialogue = dialogue;
            DialogueAnimConfig.instance.StartDialogue(this, Conversation);
        }
    }
    void DisplayName() // Called by RaycastReticle.cs
    {
        if (identified)
            ItemDisplayer.instance.DisplayName(displayName);
        else
            ItemDisplayer.instance.DisplayName("???");
    }
    
    public void TriggerBool(int convoNum) => RoomConvo[convoNum].triggered = true;

    

    public void CheckIfSaveDialogue()
    {
        foreach(LocalDialogue d in RoomConvo)
        {
            if (DialogueData.CheckDialogue(d.dialogue) || DialogueData.CheckDialogue(d.dialogueIfFlagNotMet))
            {
                GameSaver.LoadActor = this;
                //Debug.LogWarning("LOAD ACTOR WAS USED:t " + gameObject.transform.parent.name);
            }
            //DialogueData.CheckDialogue(d.dialogue);
            //DialogueData.CheckDialogue(d.dialogueIfFlagNotMet);
        }
    }

    //ActorData Test() => new ActorData(gameObject.transform.parent.name, gameObject, this);
    //Type ICreatable.GetObjType() { return GetType(); }
}
[System.Serializable]
public class LocalDialogue
{
    public Dialogue dialogue = null;
    public bool triggered = false;
    public string requiredProgressionFlag = "";
    public Dialogue dialogueIfFlagNotMet = null;
    public List<BoolTrigger> dialogueBoolTrigger = new List<BoolTrigger>();
    [Serializable]
    public class BoolTrigger
    {
        public string actorName = "";
        public int convoNum = 0;
    }
    public object Clone()
    {
        LocalDialogue d = new LocalDialogue();
        d.triggered = triggered;
        d.requiredProgressionFlag = requiredProgressionFlag;
        d.dialogue = dialogue;
        d.dialogueIfFlagNotMet = dialogueIfFlagNotMet;
        d.dialogueBoolTrigger = dialogueBoolTrigger;
        return d;
    }
    public static Dialogue EvaluateDialogue(List<LocalDialogue> RoomConvo)
    {
        if (RoomConvo.Count == 0)
            return null;
        LocalDialogue previous;
        foreach (LocalDialogue l in RoomConvo)
        {
            if (!l.triggered)
            {
                if (l.dialogueBoolTrigger.Count > 0)
                    CheckBoolTrigger(l.dialogueBoolTrigger);

                if (l.requiredProgressionFlag != "")
                {
                    if (ProgressionManager.instance.CheckFlag(l.requiredProgressionFlag))
                    {
                        l.triggered = true;
                        return l.dialogue;
                    }
                    else
                        return l.dialogueIfFlagNotMet;
                }
                l.triggered = true;
                return l.dialogue;
            }
            previous = l;
        }
        return RoomConvo[RoomConvo.Count - 1].dialogue; // Returns last Dialogue (After Chat)
    }
    static void CheckBoolTrigger(List<LocalDialogue.BoolTrigger> boolTriggers)
    {
        foreach (LocalDialogue.BoolTrigger trigger in boolTriggers)
        {
            if (trigger.actorName != "")
            {
                Actor a = GameObject.Find(trigger.actorName).GetComponentInChildren<Actor>();
                if (a != null)
                {
                    a.TriggerBool(trigger.convoNum);
                    if (!a.identified)
                        a.identified = true;
                }
                ItemActor i = GameObject.Find(trigger.actorName).GetComponentInChildren<ItemActor>();
                if (i != null)
                {
                    i.TriggerBool(trigger.convoNum);
                }
            }
        }
    }
}

[Serializable]
public class ActorData
{
    public Vector3 position;
    public Vector3 rotation;
    public string aReference;
    public List<LocalDialogue> aData; // Dialogue Array
    public bool identified = true;
    public Material mat; 
    public string matName = ""; // for searching resources and getting the mat from Char Database
    public object Clone()
    {
        ActorData n = new ActorData();
        ActorData copy = (ActorData)MemberwiseClone();
        n.aReference = copy.aReference;
        n.position = copy.position;
        n.rotation = copy.rotation;
        n.mat = copy.mat;
        n.matName = copy.matName;
        n.identified = copy.identified;
        n.aData = new List<LocalDialogue>();
        foreach (LocalDialogue l in copy.aData)
        {
            n.aData.Add((LocalDialogue)l.Clone());
        }
        return n;
    }
    public ActorData(string refName, GameObject actor, Actor a)
    {
        position = actor.transform.position;
        rotation = actor.transform.eulerAngles;
        aReference = refName;
        aData = a.RoomConvo;
        mat = a.character.sharedMaterial;
        identified = a.identified;
        matName = mat.name;
    }
    public ActorData() { }
}
[Serializable]
public class TActorData : ActorData
{
    public TActorData(string refName, GameObject actor, Actor a, string spriteName)
    {
        position = actor.transform.position;
        rotation = actor.transform.eulerAngles;
        aReference = refName;
        MeshRenderer mesh = a.GetComponent<MeshRenderer>();

        mat = a.character.sharedMaterial;
        identified = a.identified;
        matName = spriteName;
    }
    public new object Clone()
    {
        TActorData n = new TActorData();
        TActorData copy = (TActorData)MemberwiseClone();
        n.aReference = copy.aReference;
        n.position = copy.position;
        n.rotation = copy.rotation;
        n.mat = copy.mat;
        n.matName = copy.matName;
        n.identified = copy.identified;
        
        return n;
    }
    public TActorData() { }
}