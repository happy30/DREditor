using System.Collections;
using System.Collections.Generic;
using DREditor.EventObjects;
using UnityEngine;
using DREditor;
using UnityEngine.InputSystem;


namespace DREditor.Camera
{
    public class CameraBehaviour : MonoBehaviour
    {
        public bool DR_Style => PlayerInfo.PlayerInfo.instance != null?
            PlayerInfo.PlayerInfo.instance.settings.DRCameraPan : DR_Cam;
        [SerializeField] bool DR_Cam = false;
        public Crouch CrouchModifier;
        public Headbobbing HeadbobbingModifier;
        public bool HeadbobbingEnabled => PlayerInfo.PlayerInfo.instance != null?
            PlayerInfo.PlayerInfo.instance.settings.MovementBob : Bobbing;
        [SerializeField] bool Bobbing = true;
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

        DRControls _controls;
        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            _controls = new DRControls();
#endif
        }
        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            _controls.Enable();
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            _controls.Disable();
#endif
        }
        // Start is called before the first frame update
        void Start()
        {
            RenderSettings.fog = Fog;
            InCameraShakeMode.Value = false;
        }

        // Update is called once per frame
        void Update()
        {

            if (_controls.Player.Move.ReadValue<Vector2>() != Vector2.zero)
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

            if (MouseLookModifier.enabled)
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

            if (HeadbobbingEnabled && HeadbobbingModifier.enabled)
            {
                transform.position = transform.position + new Vector3(0, HeadbobbingModifier.GetYBobAmount(), 0);
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
        public void CallLook()
        {
            transform.position = FollowPlayerModifier.GetPlayerNoLerp();
            if (DR_Style)
            {
                if (!IsMoving) transform.localRotation = MouseLookModifier.GetRotation();
                else transform.localRotation = MouseLookModifier.DRGetRotation(false);
            }
            else
            {
                transform.localRotation = MouseLookModifier.GetRotation(false);
            }

            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Player.localEulerAngles.y, transform.localEulerAngles.z);
        }
    }
}