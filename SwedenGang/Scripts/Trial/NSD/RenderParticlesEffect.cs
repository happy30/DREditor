//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class RenderParticlesEffect : MonoBehaviour
{
    // Here reference the camera component of the particles camera
    [SerializeField] private Camera particlesCamera;

    // Adjust the resolution in pixels
    [SerializeField] private Vector2Int imageResolution = new Vector2Int(256, 256);

    // Reference the RawImage in your UI
    [SerializeField] private RawImage targetImage;

    private RenderTexture renderTexture;

    private void Awake()
    {
        if (!particlesCamera) particlesCamera = GetComponent<Camera>();
        
        renderTexture = new RenderTexture(imageResolution.x, imageResolution.y, 32);
        particlesCamera.targetTexture = renderTexture;

        targetImage.texture = renderTexture;
    }
}