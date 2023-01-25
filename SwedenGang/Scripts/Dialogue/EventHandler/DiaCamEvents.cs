//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;

[RequireComponent(typeof(Camera))]
public class DiaCamEvents : MonoBehaviour
{
    Camera dialogueCamera;
    [SerializeField] Camera protagCamera = null;

    private void Start()
    {
        dialogueCamera = GetComponent<Camera>();
        StartListening();
    }
    void StartListening()
    {
        DialogueEventSystem.StartListening("CamToPosition", CamToPosition);
        DialogueEventSystem.StartListening("ShakeCamera", ShakeCamera);
        UIHandler.ToTitle += ResetCamEvents;
    }
    private void OnDisable()
    {
        UIHandler.ToTitle -= ResetCamEvents;
    }
    void CamToPosition(object values = null) 
    {
        CTPTuple data = (CTPTuple)values;
        GameObject obj = GameObject.Find(data.objectName);
        if (!data.keepFocus)
        {
            DialogueEventSystem.TriggerEvent("ToggleBlur", false);
            DialogueEventSystem.TriggerEvent("CheckFocus");
        }
        if (GameSaver.LoadingFile)
            return;
        /* To be changed to having these be prefabs and spawning when loading room
         * referenceing the prefab?
         * OR
         * Have an interface monobehavior that when spawning in is referenced to a list
         * as it's being spawned in.
         */
        if (obj)
        {
            if (DialogueAssetReader.instance.ffMode)
            {
                dialogueCamera.transform.position = obj.transform.position;
                dialogueCamera.transform.eulerAngles = obj.transform.eulerAngles;
                Debug.Log("Setting Dialogue Cam");
            }
            else
            {
                t = dialogueCamera.transform.DOMove(obj.transform.position, 1);
                dialogueCamera.transform.DORotate(obj.transform.eulerAngles, 1);
            }
        }
        else 
        {
            // Figure out a way to make sure we're not in the middle of loading a scene
            Debug.LogWarning("Couldn't find object \"" + data.objectName + "\" In the current Scene: " + SceneManager.GetActiveScene().name);
        }
    }
    public static Tween t;
    Vector3 lastPos;
    void ShakeCamera(object values = null)
    {
        SOTuple data = (SOTuple)values;
        /*
        EventHandler handler = null;
        handler = (s, e) =>
        {
            StopCam();
            DialogueAssetReader.OnLineEnd -= handler;
            DialogueAssetReader.delegates.Remove(handler);
        };
        DialogueAssetReader.delegates.Add(handler);
        DialogueAssetReader.OnLineEnd += handler;
        */
        if (t != null && t.IsPlaying())
        {
            //Debug.LogWarning("T is still playing");
            StartCoroutine(ShakeWait(data));
            return;
        }
        lastPos = dialogueCamera.transform.position;
        // Might need to add a bool to SOTuple to decide whether to use the below line or not
        DialogueAssetReader.OnLineEndEvent += StopCam;
        if (data.protag && protagCamera != null)
            protagCamera.DOShakePosition(data.duration, data.strength / 50, data.vibrato, data.randomness, data.fadeOut);
        else
            dialogueCamera.DOShakePosition(data.duration, data.strength / 50, data.vibrato, data.randomness, data.fadeOut);

    }
    IEnumerator ShakeWait(SOTuple data)
    {
        yield return new WaitUntil(() => !t.IsPlaying());
        lastPos = dialogueCamera.transform.position;
        // Might need to add a bool to SOTuple to decide whether to use the below line or not
        DialogueAssetReader.OnLineEndEvent += StopCam;
        if (data.protag && protagCamera != null)
            protagCamera.DOShakePosition(data.duration, data.strength / 50, data.vibrato, data.randomness, data.fadeOut);
        else
            dialogueCamera.DOShakePosition(data.duration, data.strength / 50, data.vibrato, data.randomness, data.fadeOut);
        yield break;
    }
    void StopCam()
    {
        Debug.LogWarning("StopCam Called");
        DialogueAssetReader.OnLineEndEvent -= StopCam;
        dialogueCamera.DOKill();
        protagCamera.DOKill();
        dialogueCamera.transform.DOMove(lastPos, 0.1f);
    }
    void ResetCamEvents()
    {
        Debug.LogWarning("Reset Cam Called");
        t = null;
        dialogueCamera.DOKill();
        dialogueCamera.transform.SetPositionAndRotation(PlayerManager.instance.mainCamera.transform.position,
            PlayerManager.instance.mainCamera.transform.rotation);
    }
}
