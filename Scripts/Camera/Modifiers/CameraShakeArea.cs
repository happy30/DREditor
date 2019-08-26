using System.Collections;
using System.Collections.Generic;
using EventObjects;
using UnityEngine;

public class CameraShakeArea : MonoBehaviour
{
    private bool _active;
    private bool _enabled;
    public FloatWithEvent ShakeOmega;

    private Transform _player;

    public float MaxShake;
    private float shake;

    public BoolWithEvent CameraShakeMode;
    
    // Start is called before the first frame update
    void Start()
    {
        _enabled = true;
        shake = MaxShake;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_active || !_enabled) return;

        ShakeOmega.Value = MaxShake - (MaxShake - (shake / Vector3.Distance(_player.position, transform.position)));


    }


    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            CameraShakeMode.Value = true;
            _player = col.transform;
            _active = true;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            CameraShakeMode.Value = false;
            ShakeOmega.Value = 0;
            _active = false;
        }
    }

    public void Disable()
    {
        CameraShakeMode.Value = false;
        ShakeOmega.Value = 0;
        _enabled = false;

    }


    void OnDrawGizmos()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(0, 1, 1, 1);
        Gizmos.DrawWireSphere(transform.position, transform.localScale.x /2);
    }
}
