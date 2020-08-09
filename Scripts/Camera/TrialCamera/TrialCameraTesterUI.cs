//Main UI Script for Trial Camera Script for debugging.
using UnityEngine;
using UnityEngine.UI;
using DREditor.Camera;

namespace DREditor.Debug
{
    [AddComponentMenu("DREditor/Debug/DR Trial Camera Tester")]
    public class TrialCameraTesterUI : MonoBehaviour
    {
        public GameObject CameraAnchor;
        public Animator CameraAnimator;
        public DRTrialCamera TrialCamera;

        public GameObject UI, OneShotButton;
        public float SeatNum, CharHeight, SeatRadius;
        [SerializeField] private string AnimName;
        [SerializeField] private Text SeatNumText = null, CharHeightText = null, SeatRadiusText = null;

        [SerializeField] private InputField strlistener = null;

        void Start()
        {
            if (CameraAnchor == null)
            {
                UnityEngine.Debug.LogError("DRTrialCamTesterUI: CameraAnchor not found! Be sure that the CameraAnchor is assigned in the inspector.");
            }
            if (!CameraAnimator)
            {
                CameraAnchor.gameObject.transform.GetComponentInChildren<Animator>();
            }

            TrialCamera = CameraAnchor.GetComponent<DRTrialCamera>();
            strlistener.onValueChanged.AddListener(SetAnimName);
        }

        void Update()
        {
            SeatNumText.text = (TrialCamera.SeatFocus + 1f).ToString();
            CharHeightText.text = TrialCamera.CharHeightOffset[(int)TrialCamera.SeatFocus].ToString();
            SeatRadiusText.text = TrialCamera.SeatRadius.ToString();
        }

        public void ChangeFocus(float charnum)
        {
            CameraAnchor.GetComponent<DRTrialCamera>().SeatFocus = charnum;
        }

        public void SetSeatNo(Dropdown value)
        {
            SeatNum = value.value;
        }

        public void SetAnimName(string anim)
        {
            AnimName = anim;
        }

        public void SetSmoothFocus(bool boolval)
        {
            CameraAnchor.GetComponent<DRTrialCamera>().SmoothFocus = boolval;
        }

        public void SetHeadmasterFocus()
        {
            CameraAnchor.GetComponent<DRTrialCamera>().SetHeadmasterFocus(!CameraAnchor.GetComponent<DRTrialCamera>().IsFocusedOnHeadmaster());
        }

        public void TriggerAnim()
        {
            CameraAnchor.GetComponent<DRTrialCamera>().TriggerAnim(SeatNum, AnimName);
        }

        public void TriggerAnim(int id)
        {
            CameraAnimator.SetTrigger(id);
        }

        public void TriggerAnim(string tag)
        {
            CameraAnimator.SetTrigger(tag);
        }

        public void SwitchActive(bool boolswitch)
        {
            UI.SetActive(boolswitch);
            OneShotButton.SetActive(!boolswitch);
        }
    }
}