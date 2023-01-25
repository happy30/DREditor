//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemActor : MonoBehaviour, IDialogueHolder
{
    public bool selectable = true;
    public List<LocalDialogue> RoomConvo = new List<LocalDialogue>();
    public Transform head = null;
    public Transform body = null;
    [HideInInspector] public Dialogue[] Conversation = null;
    public bool identified = true;
    public string displayName = "";
    [HideInInspector] public Collider box = null;
    private void Awake()
    {
        box = GetComponent<Collider>();
    }
    void HitByRay() // Called by RaycastReticle.cs
    {
        Conversation = new Dialogue[1];
        Conversation[0] = EvaluateDialogue();
        DialogueAnimConfig.instance.StartDialogue(null, Conversation, this);
    }
    void DisplayName() // Called by RaycastReticle.cs
    {
        if (identified)
            ItemDisplayer.instance.DisplayName(displayName);
        else
            ItemDisplayer.instance.DisplayName("???");
    }
    Dialogue EvaluateDialogue()
    {
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
    public void TriggerBool(int convoNum) => RoomConvo[convoNum].triggered = true;
    void CheckBoolTrigger(List<LocalDialogue.BoolTrigger> boolTriggers)
    {
        foreach (LocalDialogue.BoolTrigger trigger in boolTriggers)
        {
            if (trigger.actorName != "")
            {
                Actor a = GameObject.Find(trigger.actorName).GetComponentInChildren<Actor>();
                if(a != null)
                {
                    a.TriggerBool(trigger.convoNum);
                }
                ItemActor i = GameObject.Find(trigger.actorName).GetComponentInChildren<ItemActor>();
                if (i != null)
                {
                    i.TriggerBool(trigger.convoNum);
                }
            }
        }
    }
    public void CheckIfSaveDialogue()
    {
        foreach (LocalDialogue d in RoomConvo)
        {
            DialogueData.CheckDialogue(d.dialogue);
            DialogueData.CheckDialogue(d.dialogueIfFlagNotMet);
        }
    }
    /// <summary>
    /// Remember: Dialogue Object should be layer 6
    /// Item Object is 11
    /// </summary>
    /// <param name="to"></param>
    public void SetSelectable(bool to)
    {
        if (to == selectable)
            return;
        gameObject.layer = to ? 11 : 0;
        selectable = to;
    }
    public void ClearItemData()
    {
        RoomConvo.Clear();
        selectable = true;
    }
}
[Serializable]
public class ItemData
{
    //public Vector3 position;
    //public Vector3 rotation;
    public string iReference;
    public List<LocalDialogue> iData; // Dialogue Array
    public bool iSelectable;
    //public Material mat;
    //public string matName = ""; // for searching resources and getting the mat from Char Database
    public object Clone()
    {
        ItemData n = new ItemData();
        ItemData copy = (ItemData)MemberwiseClone();
        n.iReference = copy.iReference;
        n.iSelectable = copy.iSelectable;
        n.iData = new List<LocalDialogue>();
        foreach(LocalDialogue l in copy.iData)
        {
            n.iData.Add((LocalDialogue)l.Clone());
        }
        return n;
    }
    public ItemData(GameObject item, ItemActor i)
    {
        //position = actor.transform.position;
        //rotation = actor.transform.eulerAngles;
        iReference = item.name;
        iSelectable = i.selectable;
        iData = i.RoomConvo;
        //mat = a.character.sharedMaterial;
        //matName = mat.name;
    }
    public ItemData()
    {

    }
}