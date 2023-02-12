using System.Collections;
using UnityEngine;

using DREditor.Dialogues;
using DREditor.Dialogues.Events;

public class DRSpriteBillboard : MonoBehaviour
{
	public Color AmbientLight = Color.gray;
	public DRSpriteDepth CharShadow;

	private bool PnCMode;
	private GameObject MainCamera;
	private Renderer CharTex;
	public Material CharacterSprite;

	private Coroutine FlashC;

    private void Start()
	{
		PnCMode = AssistantDirector.current.SceneType == SceneType.PointAndClick;
		AmbientLight = RenderSettings.ambientLight;

		if (!CharTex) CharTex = GetComponent<Renderer>();
		if (!CharShadow && transform.GetChild(0)) CharShadow = transform.GetChild(0).gameObject.GetComponent<DRSpriteDepth>();
		if (!MainCamera) MainCamera = Camera.main.gameObject;

		CharTex.material.color = AmbientLight;
	}

	private void Update()
	{
		if (!DialogueHandler.InDialog && MainCamera && !PnCMode)
		{
			transform.localRotation = Quaternion.Euler(new Vector3(0f, MainCamera.transform.localRotation.eulerAngles.y, 0f));
		}
	}

	private void OnEnable()
	{
		DialogueEventSystem.StartListening("DisableCharacterHighlight", DisableFocusHighlight);
	}

	private void OnDisable()
	{
		DialogueEventSystem.StopListening("DisableCharacterHighlight", DisableFocusHighlight);
	}

	/// <summary>
	/// Used to change the main Texture of the Sprite Billboard.
	/// </summary>
	/// <param name="face"></param>
	public void ChangeExpression(Material face)
	{
		CharTex.material = face;
		if (CharShadow != null)
		{
			StartCoroutine(CharShadowCoroutine(face));
		}
	}
	private IEnumerator CharShadowCoroutine(Material mat)
    {
		yield return new WaitForSeconds(1.5f * Time.deltaTime);
		CharShadow.UpdateMat(mat);
	}

	public void Focus(bool check)
	{
		if (check)
		{
			CharTex.material.color = Color.white;
		}
		else
		{
			CharTex.material.color = AmbientLight;
		}
	}

	private void DisableFocusHighlight(object value)
	{
		Focus(false);
	}

	/// <summary>
	/// Flash the texture for a split second.
	/// </summary>
	/// <param name="intensity"></param>
	public void Flash(float intensity = 5f, AudioClip aud = null)
	{
		if (aud != null) DialogueEventSystem.TriggerEvent("PlaySystemSFX", aud);
		FlashC = StartCoroutine(FlashCoroutine(intensity));
	}
	
	//Planning to change this into a Sinewave here. But too lazy to do it yet.
	private IEnumerator FlashCoroutine(float intensity)
	{
		float _time = 0.01f;
		float _elaspedtime = 0f;
		Color flash;
		flash.r = intensity;
		flash.g = intensity;
		flash.b = intensity;
		flash.a = 1f;

		do
		{
			CharTex.material.color = Color.Lerp(Color.white, flash, _elaspedtime / _time);
			_elaspedtime += _time * 10f * Time.deltaTime;
			yield return new WaitForEndOfFrame();
			if (Input.GetButton("Cancel"))
				yield break;
		} while (_elaspedtime < _time);

		_elaspedtime = 0f;

		do
		{
			CharTex.material.color = Color.Lerp(flash, Color.white, _elaspedtime / _time);
			_elaspedtime += _time * 10f * Time.deltaTime;
			yield return new WaitForEndOfFrame();
			if (Input.GetButton("Cancel"))
				yield break;
		} while (_elaspedtime < _time);

		CharTex.material.color = Color.white;
		yield break;
	}
}
