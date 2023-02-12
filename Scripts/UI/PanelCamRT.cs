using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelCamRT : MonoBehaviour
{
	[Range(0.2f, 2f)] public float RenderResolution = 1f;
	[SerializeField] private Camera _PanelCam;

	[SerializeField] private bool DynamicResolution = true;
	private RenderTexture RT;
	private RectTransform _rect;
	private RawImage _rawImage;

	private void Awake()
	{
		_rect = GetComponent<RectTransform>();
		_rawImage = GetComponent<RawImage>();

		RT = new RenderTexture((int)(_rect.sizeDelta.x * RenderResolution), (int)(_rect.sizeDelta.y * RenderResolution), 32);
		RT.useDynamicScale = DynamicResolution;
		RT.filterMode = FilterMode.Bilinear;
		_PanelCam.targetTexture = RT;
		_rawImage.texture = RT;
	}
}
