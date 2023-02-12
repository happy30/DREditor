using System.Collections.Generic;
using UnityEngine;
using DREditor.Dialogues;

public class DRBillboardProperties : MonoBehaviour
{
    [SerializeField] public Vector3 CameraFocusPosition;
    public int DialogCounter = 0;
    [SerializeField] public List<Dialogue> DialogThread = new List<Dialogue>();
    public Dialogue DialogThreadPass()
    {
        if (DialogCounter < DialogThread.Count - 1) {
            DialogCounter++;
            return DialogThread[DialogCounter - 1];
        }
        else if(DialogCounter >= DialogThread.Count)
        {
            DialogCounter = DialogThread.Count;
            return DialogThread[DialogCounter];
        }
        else
        {
            return DialogThread[DialogCounter];
        }
    }
}
