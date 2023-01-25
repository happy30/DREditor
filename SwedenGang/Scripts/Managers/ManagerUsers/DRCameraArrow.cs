//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DREditor.EventObjects;

public class DRCameraArrow : MonoBehaviour
{
    public enum Direction
    {
        Up, Down, Left, Right
    }
    [SerializeField] Image arrow = null;
    [SerializeField] Image cancel = null;
    [SerializeField] BoolWithEvent inDialogue = null;
    [SerializeField] BoolWithEvent inTPFD = null;
    [SerializeField] Direction direction;
    Image current;
    [SerializeField] bool inCancel = false;
    private void Start()
    {
        //SetBoolWithEvent set = null;
        //set.add
        inTPFD.GetValueAndAddListener(Evaluate);
        current = arrow;
        arrow.DOFade(0, 0);
        cancel.DOFade(0, 0);
    }
    void Evaluate(bool setting)
    {
        
        //Debug.Log("Evaluate called when intpfd is " + setting + " and " + inDialogue.Value);
        if (setting)
        {
            RoomLoader.EndLoad += Show;
            Door.OnLeaveAsk += Hide;
            Door.OnDidntLeave += Show;
            DialogueAssetReader.OnDialogueStart += Hide;
            DialogueAssetReader.OnDialogueEnd += Show;
            if (!inDialogue.Value)
                Show();
        }
        else
        {
            RoomLoader.EndLoad -= Show;
            Door.OnLeaveAsk -= Hide;
            Door.OnDidntLeave -= Show;
            DialogueAssetReader.OnDialogueStart -= Hide;
            DialogueAssetReader.OnDialogueEnd -= Show;
            Hide();
        }
    }
    private void OnDestroy()
    {
        RoomLoader.EndLoad -= Show;
        Door.OnLeaveAsk -= Hide;
        Door.OnDidntLeave -= Show;
        DialogueAssetReader.OnDialogueStart -= Hide;
        DialogueAssetReader.OnDialogueEnd -= Show;
    }

    void Show() => StartCoroutine(Appear());
    IEnumerator Appear()
    {
        if (GameSaver.LoadingFile)
            yield break;
        if (Door.inLeaveProcess)
        {
            TPFDManager.Cancel -= CheckCancel;
            TPFDManager.UnCancel -= CheckUnCancel;
            yield break;
        }
        //Debug.LogWarning("Appear Called");
        current.DOFade(1, 1);
        canChange = true;
        yield return new WaitForSeconds(1);
        TPFDManager.Cancel += CheckCancel;
        TPFDManager.UnCancel += CheckUnCancel;
        
        yield break;
    }
    void Hide()
    {
        StartCoroutine(Disappear());
    }
    IEnumerator Disappear()
    {
        StopCoroutine(Appear());
        canChange = false;
        //Debug.LogWarning("Disappear Called");
        //Debug.LogWarning("Disappear Called: " + arrow.name + " from: " + cancel.name + " Can change is: " + canChange);
        current.DOFade(0, 1);
        arrow.DOFade(0, 1);
        cancel.DOFade(0, 1);
        yield return new WaitForSeconds(1);
        TPFDManager.Cancel -= CheckCancel;
        TPFDManager.UnCancel -= CheckUnCancel;
        
        yield break;
    }
    void CheckCancel(int d)
    {
        if (!inCancel && d == (int)direction)
        {
            ChangeIcon(cancel, arrow);
            inCancel = true;
        }
    }
    void CheckUnCancel(int d)
    {
        if (inCancel && d == (int)direction)
        {
            ChangeIcon(arrow, cancel);
            inCancel = false;
        }
    }
    bool canChange = false;
    void ChangeIcon(Image to, Image from)
    {
        if (!inTPFD.Value)
        {
            TPFDManager.Cancel -= CheckCancel;
            TPFDManager.UnCancel -= CheckUnCancel;
            return;
        }
        if (!canChange)
            return;
        to.DOKill(); 
        from.DOKill();
        // In side of the can change return you could set current = to and that could fix the latency
        //Debug.LogWarning("Icon Changing to: " + to.name + " from: " + from.name + " Can change is: " + canChange);
        
        to.DOFade(1, 0.3f);
        from.DOFade(0, 0.3f);
        current = to;
    }
}
