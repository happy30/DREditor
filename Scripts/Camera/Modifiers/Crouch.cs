using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

namespace DREditor.Camera
{
    
    [RequireComponent(typeof(CharacterController))]
    public class Crouch : MonoBehaviour
    {
        private CharacterController _characterController;
        [SerializeField] bool disable = false;
        private float targetHeight;

        public float CrouchHeight;
        float NormalHeight;
        public float CrouchOmega;

        public bool ToggleToCrouch;
        DRControls _controls;
        void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            NormalHeight = _characterController.height;
            targetHeight = NormalHeight;
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
        private void Start()
        {
            if (disable)
                return;
#if ENABLE_INPUT_SYSTEM
            _controls.Player.Crouch.started += cont => CrouchCall();
            _controls.Player.Crouch.canceled += cont => UnCrouchCall();
#endif
        }
        void CrouchCall()
        {
            if (ToggleToCrouch)
            {
                targetHeight = targetHeight == NormalHeight ? CrouchHeight : NormalHeight;
                return;
            }
            targetHeight = CrouchHeight;
        }
        void UnCrouchCall()
        {
            if (ToggleToCrouch)
                return;
            targetHeight = NormalHeight;
        }

        void Update()
        {
            
            _characterController.height = Mathf.Lerp(_characterController.height, targetHeight, CrouchOmega * Time.deltaTime);

            //ETween.Step(_characterController.height, targetHeight, CrouchOmega);
        }
    }
}