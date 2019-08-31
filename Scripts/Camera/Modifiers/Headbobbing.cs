using System;
using EventObjects;
using UnityEngine;

namespace DREditor.Camera
{

    public class Headbobbing : MonoBehaviour
    {
        private float timer;

        [Range(5f, 20f)]
        public float BobSpeed;

        [Range(0f, 3f)]
        public float BobAmount;

        public Transform Camera;

        public CharacterController Controller;
        public BoolWithEvent InDialogue;

        public SceneEvent PlayerStep;
        public BoolWithEvent Running;


        private float speed;
        private float amount;

        void Update()
        {

            speed = Running.Value ? BobSpeed * 2 : BobSpeed;
            amount = Running.Value ? BobAmount * 2 : BobAmount;

        }

        public float GetYBobAmount()
        {
            if (InDialogue.Value) return 0f;


            var waveslice = 0.0f;


            if (Math.Abs(Controller.velocity.magnitude) < 0.01f)
            {
                if (Math.Abs(timer) > 0.01f)
                {
                    PlayerStep.Raise();
                }
                timer = 0.0f;
                //Standing still step sound
            }
            else
            {
                waveslice = Mathf.Sin(timer);
                timer = timer + speed * Time.deltaTime;
                if (timer > Mathf.PI * 2)
                {
                    timer = timer - (Mathf.PI * 2);
                    PlayerStep.Raise();
                    //playerSFX.pitch = Random.Range(0.8f, 1.2f);
                    //playerSFX.PlayOneShot(footstepConcrete, 0.50f);

                }
            }

            if (waveslice != 0)
            {
                float translateChange = waveslice * (amount / 100);
                float totalAxes = Controller.velocity.magnitude;
                totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
                translateChange = totalAxes * translateChange;

                return translateChange;
            }

            return 0;
        }



    }
}
    
    
    
    

