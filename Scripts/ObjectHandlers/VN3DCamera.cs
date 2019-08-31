//Camera Script for DREditor by SeleniumSoul
using UnityEngine;
using System.Collections;

namespace DREditor.Handlers
{
    [AddComponentMenu("DREditor/Handlers/VN3DCamera")]
    public class VN3DCamera : MonoBehaviour
    {

        public bool controlEnabled;
        int charPosition;
        float camAngle;
        float camVAngle;
        public float maxAngle = 30f;
        public float maxVAngle = 20f;
        public float camSpeed = 1.5f;

        public Vector3 targetFocus = new Vector3(0f, 1.8f, 0f);
        public GameObject targetChar;

        public Vector3 finalCamPosition;
        public float camDistance;

        Quaternion finalAimRotation;

        void Update()
        {
            if (controlEnabled)
            {
                camAngle += Mathf.Clamp(Input.GetAxis("Horizontal"), -1, 1) * 30 * Time.deltaTime;
                camVAngle += Mathf.Clamp(Input.GetAxis("Vertical"), -1, 1) * 30 * Time.deltaTime;

                camAngle = Mathf.Clamp(camAngle, -maxAngle, maxAngle);
                camVAngle = Mathf.Clamp(camVAngle, -maxVAngle, maxVAngle);

                //Debug.Log("3DVNCamera: Currently Controllable!");
                finalAimRotation = Quaternion.Euler(camVAngle, -camAngle, 0);

                if (targetChar != null)
                {
                    targetFocus = targetChar.transform.position;
                }
                finalCamPosition = (targetFocus - new Vector3(0, camVAngle / 30f)) - (transform.forward * camDistance);
            }
            else
            {
                if (targetChar != null)
                {
                    //Debug.Log("3DVNCamera: Currently Focusing!");
                    targetFocus = targetChar.transform.position;
                    finalAimRotation = Quaternion.Lerp(transform.rotation, targetChar.transform.rotation, Time.deltaTime * 10f);
                    finalCamPosition = Vector3.Lerp(transform.position, targetFocus, Time.deltaTime * 10f);
                }
            }
        }

        public void ChangeCamPosition(string TargetCharacter)
        {
            targetChar = GameObject.Find(TargetCharacter);
            Debug.Log("3DVNCamera: Focusing on " + targetChar.name + "using a string.");
        }

        public void ChangeCamPosition(Vector3 targetPosition, Vector3 targetRotation)
        {
            targetFocus = targetPosition;
            finalAimRotation = Quaternion.Euler(targetRotation);
            Debug.Log("3DVNCamera: Focusing on a spot manually.");
        }

        public void ControlSwitch(bool option)
        {
            controlEnabled = option;
        }

        private void LateUpdate()
        {
            transform.rotation = finalAimRotation;
            transform.position = finalCamPosition;
        }
    }
}