//Camera Script for DREditor by SeleniumSoul
//Edited by Sweden<3 to work with New Unity Input System
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace DREditor.Camera
{
    using Debug = UnityEngine.Debug;
    [AddComponentMenu("DREditor/Cameras/Point and Click Camera")]
    public class PnCCamera : MonoBehaviour
    {
        //Public parameters
        public bool Controllable = true;
        public Vector3 CharPosition = new Vector3(0f, 1f, 0f);
        public float MaxLAngle = 30f;
        public float MaxRAngle = 30f;
        public float MaxTAngle = 20f;
        public float MaxBAngle = 20f;
        public float CamSpeed = 3f;
        public float InitialHAngle = 0f;
        public float InitialVAngle = 0f;
        public float CamDistance = 10f;

        //Focus on Object
        public GameObject TargetObject;
        public Vector3 TargetAimRotation;
        public float TargetCamDistance = 5f;

        //Private variables for calculation
        public float CamHAngle, CamVAngle;
        private Vector3 TargetFocus;
        private Quaternion InitAngle;
        private Quaternion FinalAimRotation;
        private Vector3 FinalCamPosition;
        DRControls _controls;
        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            _controls = new DRControls();
#endif
        }
        private void OnEnable()
        {
            //Debug.LogWarning("Enabled");
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
        void Start()
        {
            InitAngle = Quaternion.Euler(InitialVAngle, InitialHAngle, 0f);
        }
        int invertX;
        void Update()
        {
            //Please comment Line 35 if your desired inital angles is already applied.
            //InitAngle = Quaternion.Euler(InitialVAngle, InitialHAngle, 0f);

            if (Controllable)
            {
                //Debug.Log("3DVNCamera: Currently Controllable!");
                TargetObject = null;
                TargetFocus = CharPosition;

                invertX = PlayerInfo.PlayerInfo.instance.settings.InvertX ? -1 : 1;
                //Debug.LogWarning("X: " + _controls.Player.Move.ReadValue<Vector2>().x);
                //Debug.LogWarning("Y: " + _controls.Player.Move.ReadValue<Vector2>().y);
                CamHAngle += Mathf.Clamp(-_controls.Player.Move.ReadValue<Vector2>().x * invertX, -1, 1) * (CamSpeed * 10) * Time.deltaTime;
                CamVAngle += Mathf.Clamp(-_controls.Player.Move.ReadValue<Vector2>().y, -1, 1) * (CamSpeed * 10) * Time.deltaTime;
                CamHAngle = Mathf.Clamp(CamHAngle, -MaxLAngle, MaxRAngle);
                CamVAngle = Mathf.Clamp(CamVAngle, -MaxBAngle, MaxTAngle);

                FinalAimRotation = Quaternion.Euler(CamVAngle + InitialVAngle, -CamHAngle + InitialHAngle, 0);
                FinalCamPosition = (TargetFocus - new Vector3(0, CamVAngle / 30f)) - (transform.forward * CamDistance);
                //Debug.LogWarning(FinalCamPosition);
                //Debug.LogWarning(FinalAimRotation.eulerAngles);
            }
            else
            {
                if (TargetObject != null)
                {
                    //Debug.Log("3DVNCamera: Currently Focusing!");
                    TargetFocus = TargetObject.transform.position;
                    FinalAimRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(TargetAimRotation), Time.deltaTime * 30f);
                    FinalCamPosition = Vector3.Lerp(transform.position, TargetFocus - (transform.forward * TargetCamDistance), Time.deltaTime * 30f);
                }
            }
        }
        public Vector3 GetCalculatedPosition()
        {
            ClearPosition();
            TargetObject = null;
            TargetFocus = CharPosition;

            //invertX = PlayerInfo.PlayerInfo.instance.settings.InvertX ? -1 : 1;
            //Debug.LogWarning("X: " + _controls.Player.Move.ReadValue<Vector2>().x);
            //Debug.LogWarning("Y: " + _controls.Player.Move.ReadValue<Vector2>().y);
            CamHAngle += Mathf.Clamp(-_controls.Player.Move.ReadValue<Vector2>().x * invertX, -1, 1) * (CamSpeed * 10) * Time.deltaTime;
            CamVAngle += Mathf.Clamp(-_controls.Player.Move.ReadValue<Vector2>().y, -1, 1) * (CamSpeed * 10) * Time.deltaTime;
            CamHAngle = Mathf.Clamp(CamHAngle, -MaxLAngle, MaxRAngle);
            CamVAngle = Mathf.Clamp(CamVAngle, -MaxBAngle, MaxTAngle);

            FinalAimRotation = Quaternion.Euler(CamVAngle + InitialVAngle, -CamHAngle + InitialHAngle, 0);
            FinalCamPosition = (TargetFocus - new Vector3(0, CamVAngle / 30f)) - (transform.forward * CamDistance);
            Debug.LogWarning("Position: " + FinalCamPosition);
            return FinalCamPosition;
        }
        public Vector3 GetCalculatedPositionZ()
        {
            ClearPosition();
            //InitialHAngle = 0;
            //InitialVAngle = 0;
            TargetObject = null;
            TargetFocus = CharPosition;

            //invertX = PlayerInfo.PlayerInfo.instance.settings.InvertX ? -1 : 1;
            //Debug.LogWarning("X: " + _controls.Player.Move.ReadValue<Vector2>().x);
            //Debug.LogWarning("Y: " + _controls.Player.Move.ReadValue<Vector2>().y);
            CamHAngle += Mathf.Clamp(-_controls.Player.Move.ReadValue<Vector2>().x * invertX, -1, 1) * (CamSpeed * 10) * Time.deltaTime;
            CamVAngle += Mathf.Clamp(-_controls.Player.Move.ReadValue<Vector2>().y, -1, 1) * (CamSpeed * 10) * Time.deltaTime;
            CamHAngle = Mathf.Clamp(CamHAngle, -MaxLAngle, MaxRAngle);
            CamVAngle = Mathf.Clamp(CamVAngle, -MaxBAngle, MaxTAngle);

            FinalAimRotation = Quaternion.Euler(CamVAngle + InitialVAngle, -CamHAngle + InitialHAngle, 0);
            FinalCamPosition = (TargetFocus - new Vector3(0, CamVAngle / 30f)) - (new Vector3(0,0,1) * CamDistance);
            Debug.LogWarning("PositionZ: " + FinalCamPosition);
            //transform.rotation = FinalAimRotation;
            return FinalCamPosition;
        }
        public Quaternion GetCalculatedRotation()
        {
            ClearPosition();
            TargetObject = null;
            TargetFocus = CharPosition;
            
            CamHAngle += Mathf.Clamp(-_controls.Player.Move.ReadValue<Vector2>().x * invertX, -1, 1) * (CamSpeed * 10) * Time.deltaTime;
            CamVAngle += Mathf.Clamp(-_controls.Player.Move.ReadValue<Vector2>().y, -1, 1) * (CamSpeed * 10) * Time.deltaTime;
            CamHAngle = Mathf.Clamp(CamHAngle, -MaxLAngle, MaxRAngle);
            CamVAngle = Mathf.Clamp(CamVAngle, -MaxBAngle, MaxTAngle);

            FinalAimRotation = Quaternion.Euler(CamVAngle + InitialVAngle, -CamHAngle + InitialHAngle, 0);
            FinalCamPosition = (TargetFocus - new Vector3(0, CamVAngle / 30f)) - (new Vector3(0, 0, 1) * CamDistance);
            Debug.LogWarning(FinalAimRotation.eulerAngles);
            
            //transform.position = FinalCamPosition;
            return FinalAimRotation;
        }
        public void FocusCamera(string TarObj)
        {
            Controllable = false;
            TargetObject = GameObject.Find(TarObj);
            TargetAimRotation = Vector3.zero;
            TargetCamDistance = 8f;
            Debug.Log("3DVNCamera: Focusing on " + TargetObject.name + "using a string.");
        }

        public void FocusCamera(GameObject target, Vector3 rotation, float distance)
        {
            Controllable = false;
            TargetObject = target;
            TargetAimRotation = rotation;
            TargetCamDistance = distance;
            Debug.Log("3DVNCamera: Focusing on a spot manually.");
        }

        public void ControlSwitch(bool option)
        {
            Controllable = option;
        }
        public void ClearPosition()
        {
            CamHAngle = 0;
            CamVAngle = 0;
        }
        private void LateUpdate()
        {
            //.LogWarning(FinalCamPosition);
            //Debug.LogWarning(FinalAimRotation.eulerAngles);
            transform.rotation = FinalAimRotation;
            transform.position = FinalCamPosition;
        }
    }
}