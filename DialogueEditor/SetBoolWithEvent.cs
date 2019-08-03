using System.Collections;
using System.Collections.Generic;
using EventObjects;
using UnityEngine;

public class SetBoolWithEvent : MonoBehaviour
{
    public BoolWithEvent BoolWithEvent;

    public void SetBool()
    {
        if (BoolWithEvent != null) BoolWithEvent.Value = true;
    }
}
