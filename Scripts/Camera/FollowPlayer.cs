
using Klak.Math;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform Player;
    public float YOffset;
    public float FollowSpeedOmega;
    
    public Vector3 GetPlayerPosition()
    {
        return ETween.Step(transform.position, Player.position + Vector3.up * YOffset, FollowSpeedOmega);
    }

    public Quaternion GetPlayerRotation()
    {
        return Player.rotation;
    }
    
}
