// fix window resolution/aspect ratio for fullscreen, and revert back to previous window resolution after exit fullscreen
// because if user has resizable window, then going full screen with alt+tab keeps that window aspect ratio (causes letterboxing, black borders)
// this script takes highest available monitor resolution for full screen mode

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixFullScreenResolution : MonoBehaviour
{
    int maxWidth = 1280;
    int maxHeight = 720;

    int windowedWidth = 1280;
    int windowedHeight = 720;

    bool fullscreenFixed = false;
    bool windowedFixed = false;
    bool wasFullScreen = false;

    static FixFullScreenResolution Instance = null;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }
    void Start()
    {
        Resolution[] resolutions = Screen.resolutions;

        // pick max resolution for full size
        maxWidth = resolutions[resolutions.Length - 1].width;
        maxHeight = resolutions[resolutions.Length - 1].width;
    }

    private void LateUpdate()
    {
        // if we are fullscreen now
        if (Screen.fullScreen == true)
        {
            wasFullScreen = true;
            windowedFixed = false;

            // and havent fixed resolution
            if (fullscreenFixed == false)
            {
                fullscreenFixed = true;
                // then fix it
                Screen.SetResolution(maxWidth, maxHeight, true);
            }
            //            Debug.Log("---------------- full screen ---------------");
        }
        else // windowed
        {
            // we just came from fullscreen
            if (wasFullScreen == true)
            {
                //                Debug.Log("********* came from full screen *****************");
                wasFullScreen = false;

                // if windowed resolution is still fullscreen size
                if (windowedFixed == false)
                {
                    windowedFixed = true;
                    Screen.SetResolution(windowedWidth, windowedHeight, false);
                    //                    Debug.Log("FixWindowed res " + windowedWidth + "," + windowedHeight);
                }
            }
            else // we are still windowed
            {
                // take current windowed size (for example user resized it)
                windowedWidth = Screen.width;
                windowedHeight = Screen.height;
                //                Debug.Log("Take current window res = " + windowedWidth + "," + windowedHeight);
            }
            fullscreenFixed = false;
        }
    }
}
