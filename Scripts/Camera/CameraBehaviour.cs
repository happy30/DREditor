using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DREditor;
using DREditor.EventObjects;

namespace DREditor.Camera
{
    public class CameraBehaviour : MonoBehaviour
    {
        public bool DR_Style;
        public Crouch CrouchModifier;
        public Headbobbing HeadbobbingModifier;
        public FollowPlayer FollowPlayerModifier;
        public SmoothMouseLook MouseLookModifier;
        public CameraShake CameraShake;
        public Transform Player;
        public BoolWithEvent InCameraShakeMode;

        public bool IsMoving;
        private float _timer;
        private float TimeForNewShake;

        private Vector3 ShakeOffset;

        private Vector3 originalPos;


        public bool Fog;


        // Start is called before the first frame update
        void Start()
        {
            RenderSettings.fog = Fog;
            InCameraShakeMode.Value = false;
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
            {
                IsMoving = true;
            }
            else
            {
                IsMoving = false;
            }

            if (FollowPlayerModifier)
            {
                transform.position = FollowPlayerModifier.GetPlayerPosition();
            }

            if (MouseLookModifier)
            {
                if (DR_Style)
                {
                    if (!IsMoving) transform.localRotation = MouseLookModifier.GetRotation();
                    else transform.localRotation = MouseLookModifier.DRGetRotation();
                }
                else
                {
                    transform.localRotation = MouseLookModifier.GetRotation();
                }

                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Player.localEulerAngles.y, transform.localEulerAngles.z);
            }

            if (CameraShake && InCameraShakeMode.Value)
            {
                if (CameraShake.Enabled)
                {
                    TimeForNewShake = 2 - (CameraShake.Omega.Value * 2);
                    _timer += Time.deltaTime;
                    if (_timer > TimeForNewShake)
                    {
                        originalPos = transform.localPosition;
                        ShakeOffset = CameraShake.GetRandomOffset();
                        _timer = 0f;
                    }

                    transform.localPosition = Vector3.Lerp(transform.localPosition, originalPos + ShakeOffset, 0.2f * Time.deltaTime);


                    // ETween.Step(transform.localPosition, originalPos + ShakeOffset, 0.2f);
                }
            }
        }

        void FixedUpdate()
        {
            if (HeadbobbingModifier)
            {
                transform.position = transform.position + new Vector3(0, HeadbobbingModifier.GetYBobAmount(), 0);
            }
        }
    }
}