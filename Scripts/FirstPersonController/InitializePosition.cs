using System.Collections;
using System.Collections.Generic;
using EventObjects;
using UnityEngine;

namespace DREditor.FPC
{
    public class InitializePosition : MonoBehaviour
    {
        public TransformWithEvent TransformWithEvent;
        public Transform Camera;

        void Start()
        {
            StartCoroutine(WaitAFrameBeforeMoving());
        }

        IEnumerator WaitAFrameBeforeMoving()
        {
            yield return new WaitForSeconds(0.2f);
            MovePlayer();
        }

        public void MovePlayer()
        {
            transform.position = TransformWithEvent.Value.Pos;

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, TransformWithEvent.Value.Rot.eulerAngles.y, transform.eulerAngles.z);

            Camera.rotation = TransformWithEvent.Value.Rot;
        }
    }
}