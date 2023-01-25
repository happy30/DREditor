
using DREditor.EventObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DREditor.FPC
{
    [RequireComponent(typeof(CharacterController))]
    public class MovePlayer : MonoBehaviour
    {
        float MoveSpeed;
        public float WalkSpeed;
        public float RunSpeed;
        public float JumpSpeed;
        private float _cameraHeight;

        private float timer;
        public float Gravity = 9.89f;
        private float _downForce;

        private Vector3 _initialParentPos;

        //public Transform camTransform;
        Vector3 _camPosition;
        Vector3 _crouchPos;
        Vector3 _bobPos;

        CharacterController _characterController;

        public BoolWithEvent InDialogue;
        public BoolWithEvent Running;

        bool toggleSprint = false;
        DRControls _controls;
        float hVelocity;
        float vVelocity;
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _initialParentPos = transform.parent.position;
            UnityEngine.Debug.Log("Awake Of Move Player Called");
            InDialogue.Value = false;
#if ENABLE_INPUT_SYSTEM
            _controls = new DRControls();
            _controls.Player.Sprint.started += cont => Sprint();
            _controls.Player.Sprint.canceled += cont => CancelSprint();
#endif
        }
        private void Start()
        {
            UnityEngine.Debug.Log("Start Of Move Player Called");
            MoveSpeed = WalkSpeed;
            Running.Value = false;
        }
        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            _controls.Enable();
            MoveSpeed = WalkSpeed;
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            _controls.Disable();
#endif
        }
        void Sprint()
        {
            if (toggleSprint)
            {
                MoveSpeed = MoveSpeed == WalkSpeed ? RunSpeed : WalkSpeed;
                return;
            }

            MoveSpeed = RunSpeed;
            Running.Value = true;
        }
        void CancelSprint()
        {
            if (toggleSprint)
                return;

            MoveSpeed = WalkSpeed;
            Running.Value = false;
        }

        private void Update()
        {
            if (InDialogue.Value) return;

            //MoveSpeed = (Input.GetKey(KeyCode.LeftShift) ? RunSpeed : WalkSpeed);
            //Running.Value = Input.GetKey(KeyCode.LeftShift);




#if ENABLE_INPUT_SYSTEM
            hVelocity = _controls.Player.Move.ReadValue<Vector2>().x;
            vVelocity = _controls.Player.Move.ReadValue<Vector2>().y;
#elif ENABLE_LEGACY_INPUT_MANAGER
			MoveSpeed = (Input.GetKey(KeyCode.LeftShift) ? RunSpeed : WalkSpeed);
			Running.Value = Input.GetKey(KeyCode.LeftShift);
			hVelocity = Input.GetAxisRaw("Horizontal");
			vVelocity = Input.GetAxisRaw("Vertical");
			if (_characterController.isGrounded && Input.GetButton("Jump")) {
				_downForce = JumpSpeed;
			}
#endif

            var moveTowardsPos = new Vector3(hVelocity, 0, vVelocity);
            moveTowardsPos = transform.TransformDirection(moveTowardsPos);
            
            
            /*if (_characterController.isGrounded && Input.GetButton("Jump")) {
                _downForce = JumpSpeed;
            }*/

            if (!_characterController.isGrounded)
            {
                _downForce -= Gravity * Time.deltaTime;
            }
            else
            {
                _downForce = 0;  //FIX THIS STINKY CODE
            }


            moveTowardsPos = moveTowardsPos.normalized;
            moveTowardsPos = moveTowardsPos * MoveSpeed * Time.deltaTime;

            if (transform.parent.position != _initialParentPos)
            {
                moveTowardsPos += transform.parent.position;
                transform.parent.position = _initialParentPos;
            }
                   

             _characterController.Move(new Vector3(moveTowardsPos.x, _downForce * Time.deltaTime, moveTowardsPos.z));
        }
        
    }
    
    

}


