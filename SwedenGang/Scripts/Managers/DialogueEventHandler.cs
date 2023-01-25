//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DREditor.Dialogues;
using System;
using DREditor.Dialogues.Events;
using DREditor.Characters;

public class DialogueEventHandler : MonoBehaviour
{
    [SerializeField] CharacterDatabase database = null;
    private void Start()
    {
        if (!database)
            Debug.LogError("Character Database Reference Required");
        StartListening();
    }
    void StartListening()
    {
        DialogueEventSystem.StartListening("MoveCharacter", MoveCharacter);
        DialogueEventSystem.StartListening("ObjectToPosition", MoveObject);
    }

    void MoveCharacter(object value = null)
    {
        MCTuple tuple = (MCTuple)value;
        if (tuple.all)
        {
            var actors = FindObjectsOfType<Actor>();
            foreach(Actor a in actors)
            {
                a.transform.parent.localPosition = tuple.position;
                if(!GameSaver.LoadingFile && GameManager.instance.currentMode != GameManager.Mode.ThreeD)
                    a.transform.parent.localEulerAngles = tuple.rotation;
            }
        }
        else
        {
            GameObject actor = GameObject.Find(database.Characters[tuple.charNum].FirstName); // need a better way for this
            actor.transform.localPosition = tuple.position;
            if (!GameSaver.LoadingFile && GameManager.instance.currentMode != GameManager.Mode.ThreeD)
                actor.transform.localEulerAngles = tuple.rotation;
            if (!GameSaver.LoadingFile && GameManager.instance.currentMode == GameManager.Mode.ThreeD)
            {
                Transform target = GameObject.Find("Main Camera").GetComponent<Transform>(); //Find the player object
                Vector3 direction = target.transform.position - actor.transform.position;
                Quaternion rotation = Quaternion.LookRotation(direction);

                actor.transform.rotation = rotation;
                actor.transform.rotation = Quaternion.Euler(0, actor.transform.rotation.eulerAngles.y, 0);  //Limit Y and Z rotation
            }
        }
    }
    void MoveObject(object value = null)
    {
        OTuple tuple = (OTuple)value;
        GameObject g = GameObject.Find(tuple.objectName);
        if (tuple.localPos)
            g.transform.localPosition = tuple.position;
        else
            g.transform.position = tuple.position;
    }
}
