using System.Collections;
using System.Collections.Generic;
using EventObjects;
using UnityEngine;

namespace DREditor.FPC
{
    public class ObjectInteractor : MonoBehaviour
    {
        public LayerMask ObjectLayer;
        private GameObject GrabbedObject;
        public BoolWithEvent GrabbingObject;
        public BoolWithEvent RaycastingObject;
        public float RaycastLength;



        public BoolWithEvent InDialogue;


        void Start()
        {
            GrabbingObject.Value = false;
            QualitySettings.vSyncCount = 0;
        }

        void Update()
        {
            if (!GrabbingObject.Value && !InDialogue.Value)
            {
                CheckForObject(RaycastLength);
            }
            else
            {
                RaycastingObject.Value = false;
            }
        }


        private void CheckForObject(float range)
        {

            RaycastHit hit;
            //Ray ray = Camera.ScreenPointToRay (Input.mousePosition);
            if (Physics.Raycast(transform.position, transform.forward, out hit, range, ObjectLayer))
            {

                if (!CheckLayer(range))
                {
                    return;
                }
            }

        }

        bool CheckLayer(float range)
        {
            RaycastHit hit;
            return Physics.Raycast(transform.position, transform.forward, out hit, range) && (hit.collider.isTrigger || hit.collider.gameObject.layer == LayerMask.NameToLayer("GrabObject"));
        }


    }
}