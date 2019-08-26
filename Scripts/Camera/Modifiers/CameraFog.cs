using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFog : MonoBehaviour
{
    public float EnableDensity;
    public float DisableDensity;


    public void EnableFog()
    {
        StartCoroutine(EnableFogTransition());
    }

    public void DisableFog()
    {
        StartCoroutine(DisableFogTransition());
    }
    
    
    IEnumerator EnableFogTransition()
    {
        RenderSettings.fog = true;


        while (RenderSettings.fogDensity < EnableDensity)
        {
            RenderSettings.fogDensity += Time.deltaTime / 5f;
            yield return null;
        }
    }
    
    
    IEnumerator DisableFogTransition()
    {
        RenderSettings.fog = true;

        while (RenderSettings.fogDensity > DisableDensity)
        {
            RenderSettings.fogDensity -= Time.deltaTime / 5f;
            yield return null;
        }
    }
}



