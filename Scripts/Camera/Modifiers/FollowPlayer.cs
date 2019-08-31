
using UnityEngine;

namespace DREditor.Camera
{

    public class FollowPlayer : MonoBehaviour
    {
        public Transform Player;
        public float YOffset;
        public float FollowSpeedOmega;

        public Vector3 GetPlayerPosition()
        {
            return Vector3.Lerp(transform.position, Player.position + Vector3.up * YOffset, FollowSpeedOmega * Time.deltaTime);

            //ETween.Step(transform.position, Player.position + Vector3.up * YOffset, FollowSpeedOmega);
        }

        public Quaternion GetPlayerRotation()
        {
            return Player.rotation;
        }

    }
}