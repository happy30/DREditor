
using EventObjects;
using UnityEngine;


public class MouseLook : MonoBehaviour
{
    public BoolVariable InCutscene;
    public float Sensivity;
    public Transform Player;
    
    
    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        InCutscene.Register();
        
        if (!InCutscene.Value)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void OnDisable()
    {
        InCutscene.Unregister();
    }

    void Update()
    {
        if(InCutscene.Value) return;
        FirstPersonView();
    }
    
    void FirstPersonView()
    {
        Player.transform.Rotate(0, Input.GetAxis("Mouse X") * Sensivity, 0);
        
        var rotationX = -Input.GetAxis("Mouse Y") * Sensivity + transform.eulerAngles.x;
        rotationX = ClampAngle(rotationX, -80, 80);
        
        
        
        
        transform.rotation = Quaternion.Euler(new Vector3(rotationX, transform.eulerAngles.y, transform.eulerAngles.z));
    }
    
    //Make sure we can't make loops with the camera.
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < 90 || angle > 270)
        {       // if angle in the critic region...
            if (angle > 180)
            {
                angle -= 360;
            }// convert all angles to -180..+180
            if (max > 180)
            {
                max -= 360;
            }
            if (min > 180)
            {
                min -= 360;
            }
        }
        angle = Mathf.Clamp(angle, min, max);
        if (angle < 0)
        {
            angle += 360;
        }   // if angle negative, convert to 0..360
        return angle;
    }
    

}
