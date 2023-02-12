/**
 * Main Trial Camera Script for DREditor
 * Original Author: SeleniumSoul
 * Contributing Author: KHeartz
 */

using System.Collections.Generic;
using UnityEngine;

namespace DREditor.Camera
{
    [AddComponentMenu("DREditor/Cameras/DR Trial Camera v2")]
    public class DRTrialCamera2 : MonoBehaviour
    {
        [SerializeField] private Transform CameraPivotTransform = null;
        [SerializeField] private Animator CameraAnimator = null;

        [SerializeField] private float SeatFocus = 0.0f;
        [SerializeField] private float SeatRadius = 8.0f;

        [SerializeField] private RadiusLock RadiusLock = null;
        [SerializeField] private bool SmoothFocus = true;
        [SerializeField] private float SmoothTransitionTime = 10f;
        [SerializeField] private int HeadmasterPosition = 0;
        [SerializeField] private float HeamasterSeatHeightOffset = 12f;
        [SerializeField] private float HeadmasterSeatDistance = 12f;
        [SerializeField] private bool useCharacters = false;
        [SerializeField] private List<Characters.Student> students = null;
        public float[] CharHeightOffset = new float[16];
        private float AnchorAngle;
        private bool FocusOnHeadmaster = false;

        private void Start()
        {
            AnchorAngle = 360f / (useCharacters ? students.Count : CharHeightOffset.Length);
        }

        private void Update()
        {
            if (FocusOnHeadmaster)
            {
                UpdatePositionAndRotation(new Vector3(0, HeamasterSeatHeightOffset, 0), Quaternion.AngleAxis(HeadmasterPosition * AnchorAngle, Vector3.up));
            }
            else
            {
                UpdatePositionAndRotation(new Vector3(0, useCharacters ? students[(int)SeatFocus].TrialHeight : CharHeightOffset[(int)SeatFocus], 0),
                    Quaternion.AngleAxis(SeatFocus * AnchorAngle, Vector3.up));
            }
        }
        private void LateUpdate()
        {
            if (RadiusLock.Locked)
            {
                CameraPivotTransform.localPosition = new Vector3(0, 0, FocusOnHeadmaster ? HeadmasterSeatDistance : SeatRadius);
            }
        }

        private void UpdatePositionAndRotation(Vector3 targetPosition, Quaternion targetRotation)
        {
            if (SmoothFocus)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, SmoothTransitionTime * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, SmoothTransitionTime * Time.deltaTime);
            }
            else
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
            }
        }

        public void SetHeadmasterFocus(bool value) => FocusOnHeadmaster = value;

        public bool IsFocusedOnHeadmaster() => FocusOnHeadmaster;

        public void ChangeFocus(float charnum) => SeatFocus = charnum;

        public void TriggerAnim(int id) => CameraAnimator.SetTrigger(id);

        public void TriggerAnim(string tag) => CameraAnimator.SetTrigger(tag);

        public void TriggerAnim(float seat, string animname)
        {
            SeatFocus = seat;
            CameraAnimator.SetTrigger(animname);
        }
    }
}
