// Main Trial Camera Script for DREditor by SeleniumSoul
using DREditor.Dialogues;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Camera
{
    [AddComponentMenu("DREditor/Cameras/DR Trial Camera")]
    public class DRTrialCamera : MonoBehaviour
    {
        public GameObject CameraPivot;
        public Animator CameraAnimator;

        [SerializeField] float DefaultRadius = 8.11f;
        public float SeatFocus;
        //[SerializeField] float CurrentHeight = 0;
        public float SeatRadius;

        public bool RadiusLock = true;
        public bool SmoothFocus = true;
        public float SmoothTransitionTime = 10f;
        public int HeadmasterPosition = 0;
        public float HeamasterSeatHeightOffset = 12f;
        public float HeadmasterSeatDistance = 12f;
        public float[] CharHeightOffset = new float[16];
        private float AnchorAngle;
        private bool FocusOnHeadmaster = false;

        private bool Replace = false;
        private float HeightOverride;

        public delegate void LineDel(TrialLine line);
        public static event LineDel OnTestTrialLine;

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
            if (FocusOnHeadmaster)
            {
                UpdatePositionAndRotation(new Vector3(0, HeamasterSeatHeightOffset, 0), Quaternion.AngleAxis(HeadmasterPosition * AnchorAngle, Vector3.up));
                //CurrentHeight = HeamasterSeatHeightOffset;
                //UnityEngine.Debug.LogWarning("FOCUSING HEADMASTER");
            }
            else if (Replace)
            {
                //UnityEngine.Debug.LogWarning("USING REPLACE");
                UpdatePositionAndRotation(new Vector3(0, HeightOverride, 0), Quaternion.AngleAxis(SeatFocus * AnchorAngle, Vector3.up));
                //CurrentHeight = HeightOverride;
            }
            else
            {
                if ((int)SeatFocus >= CharHeightOffset.Length)
                    SeatFocus = 0;
                UpdatePositionAndRotation(new Vector3(0, CharHeightOffset[(int)SeatFocus], 0), Quaternion.AngleAxis(SeatFocus * AnchorAngle, Vector3.up));
                //CurrentHeight = CharHeightOffset[(int)SeatFocus];
            }
        }
        void LateUpdate()
        {
            if (RadiusLock)
            {
                if(FocusOnHeadmaster)
                {
                    CameraPivot.transform.localPosition = new Vector3(0, 0, HeadmasterSeatDistance);
                } else
                {
                    CameraPivot.transform.localPosition = new Vector3(0, 0, SeatRadius);
                }
            } 
        }

        void UpdatePositionAndRotation(Vector3 targetPosition, Quaternion targetRotation)
        {
            if(SmoothFocus)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, SmoothTransitionTime * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, SmoothTransitionTime * Time.deltaTime);
            } else
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
            }
        }

        public void SetHeadmasterFocus(bool value)
        {
            FocusOnHeadmaster = value;
        }

        public bool IsFocusedOnHeadmaster()
        {
            return FocusOnHeadmaster;
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
        public void ResetTrigger(string tag) => CameraAnimator.ResetTrigger(tag);
        public void TriggerAnim(float seat, string animname)
        {
            SeatFocus = seat;
            CameraAnimator.SetTrigger(animname);
        }

        public void SetOverride(bool to)
        {
            Replace = to;
        }
        public void SetDefaultValues()
        {
            SetOverride(false);
            SeatRadius = DefaultRadius;
        }
        public void ApplyOverrides(TCO o, bool dontPan)
        {
            HeightOverride = o.height;
            SeatRadius = o.distance;
            if (!dontPan)
                SeatFocus = o.seatFocus;
            SetOverride(true);
        }
        public void InvokeTestLine(TrialLine line)
        {
            OnTestTrialLine?.Invoke(line);
        }
    }
}