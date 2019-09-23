// Main Trial Camera Script for DREditor by SeleniumSoul
using UnityEngine;

namespace DREditor.Camera
{
    [AddComponentMenu("DREditor/Cameras/DR Trial Camera")]
    public class DRTrialCamera : MonoBehaviour
    {
        public GameObject CameraPivot;
        public Animator CameraAnimator;

        public float SeatFocus;
        public float SeatRadius;

        public bool RadiusLock = true;
        public bool SmoothFocus = true;
        public float[] CharHeightOffset = new float[16];
        private float AnchorAngle;

        void Start()
        {
            if (CameraPivot == null)
            {
                CameraPivot = gameObject.transform.GetChild(0).gameObject;
            }
            if (CameraAnimator == null)
            {
                CameraAnimator = CameraPivot.GetComponent<Animator>();
            }

            AnchorAngle = 360f / CharHeightOffset.Length;
        }

        void Update()
        {
            RadiusLock = CameraPivot.GetComponent<RadiusLock>().Locked;
            if (SmoothFocus)
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(0, CharHeightOffset[(int)SeatFocus], 0), Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(SeatFocus * AnchorAngle, Vector3.up), Time.deltaTime * 10f);
            }
            else
            {
                transform.position = new Vector3(0, CharHeightOffset[(int)SeatFocus], 0);
                transform.rotation = Quaternion.AngleAxis(SeatFocus * AnchorAngle, Vector3.up);
            }
        }
        void LateUpdate()
        {
            if (RadiusLock) CameraPivot.transform.localPosition = new Vector3(0, 0, SeatRadius);
        }

        public void ChangeFocus(float charnum)
        {
            SeatFocus = charnum;
        }

        public void TriggerAnim(int id)
        {
            CameraAnimator.SetTrigger(id);
        }

        public void TriggerAnim(string tag)
        {
            CameraAnimator.SetTrigger(tag);
        }

        public void TriggerAnim(float seat, string animname)
        {
            SeatFocus = seat;
            CameraAnimator.SetTrigger(animname);
        }
    }
}