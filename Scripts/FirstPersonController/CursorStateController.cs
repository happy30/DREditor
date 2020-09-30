using System.Collections;
using System.Collections.Generic;
using EventObjects;
using UnityEngine;

namespace DREditor.FPC
{

    public class CursorStateController : MonoBehaviour
    {
        public BoolWithEvent InInspectMode;
        public BoolWithEvent InDialogue;
        public BoolWithEvent MouseOverRotatableObject;
        public BoolWithEvent MouseOverInspectPoint;
        public BoolWithEvent InSettings;
        public Texture2D CursorImage;
        public Texture2D GrabCursorImage;
        public Texture2D GrabbingCursorImage;
        public Texture2D PressCursorImage;

        private bool grabbing;


        void Awake()
        {
            if (CursorImage != null)
            {
                Cursor.SetCursor(CursorImage, Vector2.zero, CursorMode.Auto);
                Cursor.lockState = CursorLockMode.Locked;
            }

        }

        void Update()
        {
            if (InDialogue.Value && !InSettings.Value && !InInspectMode.Value)
            {
                Cursor.visible = false;
                return;
            }



            Cursor.lockState = InInspectMode.Value || InSettings.Value ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = InInspectMode.Value || InSettings.Value;

            if (MouseOverRotatableObject.Value && !grabbing)
            {
                grabbing = true;
            }
            else if (!MouseOverRotatableObject.Value && grabbing && !Input.GetMouseButton(0))
            {
                Cursor.SetCursor(CursorImage, Vector2.zero, CursorMode.Auto);
                Cursor.visible = true;
                grabbing = false;
            }

            if (!MouseOverInspectPoint.Value)
            {
                if (grabbing)
                {
                    if (Input.GetMouseButton(0))
                    {
                        Cursor.SetCursor(GrabbingCursorImage, new Vector2(8, 8), CursorMode.Auto);
                        Cursor.visible = true;
                    }
                    else
                    {
                        Cursor.SetCursor(GrabCursorImage, new Vector2(8, 8), CursorMode.Auto);
                        Cursor.visible = true;
                    }
                }
            }
            else
            {
                Cursor.SetCursor(PressCursorImage, new Vector2(4, 4), CursorMode.Auto);
                Cursor.visible = true;
            }

            if (InSettings.Value)
            {
                Cursor.SetCursor(CursorImage, new Vector2(4, 4), CursorMode.Auto);
                Cursor.visible = true;
            }




        }
    }
}