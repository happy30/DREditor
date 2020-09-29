using System.Collections;
using System.Collections.Generic;
using EventObjects;
using UnityEngine;

namespace DREditor.FPC
{
    public class SetPlayerSpawnPoint : MonoBehaviour
    {
        public TransformWithEvent TransformWithEvent;
        public SceneEvent MovePlayer;

        void Awake()
        {
            TransformWithEvent.Reset();
            TransformWithEvent.Init();
            TransformWithEvent.SetValue(new PosRot(transform.position, transform.rotation));

        }

        void Start()
        {
            MovePlayer.Raise();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);
            Gizmos.DrawWireSphere(transform.position, 1f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + transform.forward, new Vector3(0.3f, 0.3f, 0.3f));
        }
    }
}