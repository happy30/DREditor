using System.Collections;
using System.Collections.Generic;
using Klak.Math;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Crouch : MonoBehaviour
{
    private CharacterController _characterController;

    private float targetHeight;

    public float CrouchHeight;
    float NormalHeight;
    public float CrouchOmega;

    public bool ToggleToCrouch;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        NormalHeight = _characterController.height;
    }


    void Update()
    {
        if (!ToggleToCrouch)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                targetHeight = CrouchHeight;
            }
            else
            {
                if (!Physics.Raycast(transform.position, Vector3.up, 2f))
                {
                    targetHeight = NormalHeight;
                }
                
            }
        }


        _characterController.height = ETween.Step(_characterController.height, targetHeight, CrouchOmega);
    }
}
