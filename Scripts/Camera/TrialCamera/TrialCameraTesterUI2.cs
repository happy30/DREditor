/**
 * Main UI Script for Trial Camera Script for debugging/previewing
 * Original Author: SeleniumSoul
 * Contributing Author: KHeartz
 */

using UnityEngine;
using UnityEngine.UI;
using DREditor.Camera;
using DREditor.Dialogues;

namespace DREditor.Debug
{
    [AddComponentMenu("DREditor/Debug/DR Trial Camera Tester v2")]
    public class TrialCameraTesterUI2 : MonoBehaviour
    {
        [SerializeField] private DRTrialCamera CameraAnchor = null;
        [SerializeField] private Animator CameraAnimator = null;
        [SerializeField] private DRTrialCamera TrialCamera = null;

        [SerializeField] private GameObject UI = null;
        [SerializeField] private GameObject OneShotButton = null;

        [SerializeField] private string AnimName = null;
        [SerializeField] private Text SeatNumText = null, CharHeightText = null, SeatRadiusText = null;

        [SerializeField] private InputField strlistener = null;

        [SerializeField] private Transform AnimButtonContentTransform = null;
        [SerializeField] private GameObject AnimButtonPF = null;

        private float SeatNum;
        void Start()
        {
            if (CameraAnchor == null)
            {
                UnityEngine.Debug.LogError("DRTrialCamTesterUI: CameraAnchor not found! Be sure that the CameraAnchor is assigned in the inspector.");
            }
            strlistener.onValueChanged.AddListener(SetAnimName);
            AddAnimButtons();
        }

        void Update()
        {
            SeatNumText.text = (TrialCamera.SeatFocus + 1f).ToString();
            CharHeightText.text = TrialCamera.CharHeightOffset[(int)TrialCamera.SeatFocus].ToString();
            SeatRadiusText.text = TrialCamera.SeatRadius.ToString();
        }

        public void ChangeFocus(float charnum) => TrialCamera.SeatFocus = charnum;

        public void SetSeatNo(Dropdown value) => SeatNum = value.value;

        public void SetAnimName(string anim) => AnimName = anim;

        public void SetSmoothFocus(bool boolval) => TrialCamera.SmoothFocus = boolval;

        public void SetHeadmasterFocus() => TrialCamera.SetHeadmasterFocus(!TrialCamera.IsFocusedOnHeadmaster());

        public void TriggerAnim() => TrialCamera.TriggerAnim(SeatNum, AnimName);

        public void TriggerAnim(int id) => CameraAnimator.SetTrigger(id);

        public void TriggerAnim(string tag) => CameraAnimator.SetTrigger(tag);

        public void SwitchActive(bool boolswitch)
        {
            UI.SetActive(boolswitch);
            OneShotButton.SetActive(!boolswitch);
        }
        private void AddAnimButtons()
        {
            System.Array.ForEach(CameraAnimator.runtimeAnimatorController.animationClips,
                animState => AddAnimButton(animState.name));
        }
        private void AddAnimButton(string animName)
        {
            var buttonGO = Instantiate(AnimButtonPF, AnimButtonContentTransform);
            var button = buttonGO.GetComponent<Button>();
            buttonGO.name = animName;
            buttonGO.GetComponentInChildren<Text>().text = animName;
            button.onClick.AddListener(() => strlistener.text = animName);
            button.onClick.AddListener(TriggerAnim);
        }
    }
}
