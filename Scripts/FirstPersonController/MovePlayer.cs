
using EventObjects;
using UnityEngine;

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


        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _initialParentPos = transform.parent.position;
            InDialogue.Value = false;
            
        }



        private void Update()
        {
            if (InDialogue.Value) return;

            MoveSpeed = (Input.GetKey(KeyCode.LeftShift) ? RunSpeed : WalkSpeed);
            Running.Value = Input.GetKey(KeyCode.LeftShift);
            
            
            
            var hVelocity = Input.GetAxisRaw("Horizontal");
            var vVelocity = Input.GetAxisRaw("Vertical");

            var moveTowardsPos = new Vector3(hVelocity, 0, vVelocity);
            moveTowardsPos = transform.TransformDirection(moveTowardsPos);
            
            
            if (_characterController.isGrounded && Input.GetButton("Jump")) {
                _downForce = JumpSpeed;
            }

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


