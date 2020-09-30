//Camera Script for DREditor by SeleniumSoul
using UnityEngine;
using System.Collections;

namespace DREditor.Camera
{
    [AddComponentMenu("DREditor/Cameras/Point and Click Camera")]
    public class PnCCamera : MonoBehaviour
    {
        //Public parameters
        public bool Controllable = true;
        public Vector3 CharPosition = new Vector3(0f, 1.8f, 0f);
        public float MaxHAngle = 30f;
        public float MaxVAngle = 20f;
        public float CamSpeed = 3f;
        public float InitialHAngle = 0f;
        public float InitialVAngle = 0f;
        public float CamDistance = 8f;

        //Focus on Object
        public GameObject TargetObject;
        public Vector3 TargetAimRotation;
        public float TargetCamDistance = 5f;

        //Private variables for calculation
        private float CamHAngle, CamVAngle;
        private Vector3 TargetFocus;
        private Quaternion InitAngle;
        private Quaternion FinalAimRotation;
        private Vector3 FinalCamPosition;

        void Start()
        {
            InitAngle = Quaternion.Euler(InitialVAngle, InitialHAngle, 0f);
        }

        void Update()
        {
            //Please comment Line 35 if your desired inital angles is already applied.
            //InitAngle = Quaternion.Euler(InitialVAngle, InitialHAngle, 0f);

            if (Controllable)
            {
                //Debug.Log("3DVNCamera: Currently Controllable!");
                TargetObject = null;
                TargetFocus = CharPosition;

                CamHAngle += Mathf.Clamp(Input.GetAxis("Horizontal"), -1, 1) * (CamSpeed * 10) * Time.deltaTime;
                CamVAngle += Mathf.Clamp(Input.GetAxis("Vertical"), -1, 1) * (CamSpeed * 10) * Time.deltaTime;
                CamHAngle = Mathf.Clamp(CamHAngle, -MaxHAngle, MaxHAngle);
                CamVAngle = Mathf.Clamp(CamVAngle, -MaxVAngle, MaxVAngle);

                FinalAimRotation = Quaternion.Euler(CamVAngle + InitialVAngle, -CamHAngle + InitialHAngle, 0);
                FinalCamPosition = (TargetFocus - new Vector3(0, CamVAngle / 30f)) - (transform.forward * CamDistance);
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

        private void LateUpdate()
        {
            transform.rotation = FinalAimRotation;
            transform.position = FinalCamPosition;
        }
    }
}