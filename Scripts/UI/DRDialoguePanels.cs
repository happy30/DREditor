//Dialogue Panel script by SeleniumSoul for DR:Distrust

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;

/// <summary>
/// UI Script that handles the comicstrip-like view when in a dialogue.
/// </summary>
public class DRDialoguePanels : MonoBehaviour
{
	public GameObject[] PanelCameras = new GameObject[3];
	public float TalkFOV;
	//public Vector3 CamPosition = new Vector3(0f,0.6f, 1.35f); 
	public RawImage BGCapture;
	[SerializeField] private RawImage BGCapture2; //Used for Panel1
	public Material Opacity;
	public bool Immediate;

	private Animator Panels;
	private GameObject[] Focused;
	private GameObject MainCam;
	private Camera MainCamComp;
	private Texture _Capture;

	private CanvasGroup BGCaptureCG, PanelCG;

	private Coroutine CameraLerp;

	private void Awake()
	{
		SceneManager.sceneLoaded += Reinitialize;
		Panels = GetComponent<Animator>();
		PanelCG = GetComponent<CanvasGroup>();
		BGCaptureCG = BGCapture.gameObject.GetComponent<CanvasGroup>();
	}

	private void Start()
	{
		DialoguePlayer.current.DiaPanels = gameObject;
		Focused = new GameObject[PanelCameras.Length];

		foreach (GameObject n in PanelCameras)
		{
			n.GetComponent<Camera>().enabled = false;
		}

		CamNullCheck();
		gameObject.SetActive(false);
	}

	private void Reinitialize(Scene load, LoadSceneMode mode)
    {
		CamNullCheck();
    }

	private bool CamNullCheck()
	{
		try
		{
			if (MainCam == null) MainCam = Camera.main.gameObject;
			if (MainCamComp == null) MainCamComp = MainCam.GetComponent<Camera>();

			return (MainCam && MainCamComp);
		}
		catch
		{
			return false;
		}
	}

	private void OnEnable()
	{
		//Subscribe to Events here
		DialogueEventSystem.StartListening("ChangeCharacterFocus", ChangeCamFocus);
		DialogueEventSystem.StartListening("ChangeWindowPattern", TriggerPanels);
		DialogueEventSystem.StartListening("DiaPanelTerminate", BGTerminate);

		DialogueEventSystem.StartListening("PanelActive", PanelActive);

		foreach (GameObject n in PanelCameras)
		{
			if (CamNullCheck())
			{
				n.transform.SetParent(MainCam.transform);
				n.transform.SetPositionAndRotation(MainCam.transform.position, MainCam.transform.rotation);
				n.GetComponent<Camera>().fieldOfView = MainCamComp.fieldOfView;
				n.GetComponent<Camera>().enabled = true;
			}
		}

		BGInitialize();
	}

	private void OnDisable()
	{
		DialogueEventSystem.StopListening("ChangeCharacterFocus", ChangeCamFocus);
		DialogueEventSystem.StopListening("ChangeWindowPattern", TriggerPanels);
		DialogueEventSystem.StopListening("DiaPanelTerminate", BGTerminate);

		DialogueEventSystem.StopListening("PanelActive", PanelActive);
	}

	private void BGInitialize()
	{
		Panels.SetBool("Immediate", Immediate);
		StartCoroutine(RecordFrame());
		BGFilter(true);
	}

	private void PanelActive(object value)
    {
		BGFilter((bool)value, true);
    }

	private void BGTerminate(object value)
	{
		Immediate = false;
		BGFilter(false, true, true);
	}

	private IEnumerator RecordFrame()
	{
		yield return new WaitForEndOfFrame();

		RenderTexture RT = RenderTexture.active;
		RT = MainCamComp.targetTexture;
		MainCamComp.Render();

		Texture2D image = new Texture2D(MainCamComp.pixelWidth, MainCamComp.pixelHeight);
		image.ReadPixels(new Rect(0, 0, MainCamComp.pixelWidth, MainCamComp.pixelHeight), 0, 0);
		image.Apply();

		_Capture = image; //RTImage();

		BGCapture.texture = _Capture;
		BGCapture2.texture = _Capture;
		if (CamNullCheck()) MainCamComp.enabled = false;

		yield break;
	}

	private void ReturnCamToOrigParent()
	{
		foreach (GameObject n in PanelCameras)
		{
			n.GetComponent<Camera>().fieldOfView = MainCamComp.fieldOfView;
			n.GetComponent<Camera>().enabled = false;
			n.transform.SetParent(transform);
		}
	}

	private void TriggerPanels(object state)
	{
		if (state is CWPTuple _CWPValue)
		{
			Debug.Log("DREditor (DialoguePanels): Changing pattern to " + _CWPValue.PatternName);
			Panels.SetTrigger(_CWPValue.PatternName);
		}

		else if (state is string _stringstate)
		{
			Panels.SetTrigger(_stringstate);
		}

		else if (state is int _intstate)
		{
			Debug.Log("DREditor (DialoguePanels): Triggering ChangePattern with int " + _intstate);
			Panels.SetTrigger(_intstate);
		}
	}

	private void ChangeCamFocus(object value)
	{
		var values = (ValueTuple<int, GameObject, Vector3>)value;

		int camNum = values.Item1;
		GameObject character = values.Item2;
		Vector3 position = values.Item3;
		
		//Change color of previous character
		if (Focused[camNum] && !DialoguePlayer.current.PropMode)
		{
			Focused[camNum].GetComponent<DRSpriteBillboard>().Focus(false);
		}

		Debug.Log("DREditor <color=purple>(DialoguePanels)</color>: Triggering ChangeCamFocus with camera " + camNum + " on " + character.name + " at " + position + " position.");

		Focused[camNum] = character;
		PanelCameras[camNum].transform.SetParent(character.transform);

		if (!DialogueHandler._skip)
		{
			CameraLerp = StartCoroutine(LerpPos(PanelCameras[camNum], position, 0.5f));
		} //If not skipping, lerp.
		else
		{
			PanelCameras[camNum].GetComponent<Camera>().fieldOfView = TalkFOV;
			PanelCameras[camNum].transform.localPosition = position;
			PanelCameras[camNum].transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
		} //If skipping, just teleport.

		//Change color of character
		if (!DialoguePlayer.current.PropMode)
		{
			Focused[camNum].GetComponent<DRSpriteBillboard>().Focus(true);
		}
	}
	private IEnumerator LerpPos(GameObject camera, Vector3 position, float speed)
	{
		Camera cam = camera.GetComponent<Camera>();
		float InitFOV = cam.fieldOfView;
		float _StartTime = 0;

		while (_StartTime < speed)
		{
			if (!DialoguePlayer.current.PropMode) cam.fieldOfView = Mathf.Lerp(InitFOV, TalkFOV, (_StartTime / speed * 2f));
			camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, position, (_StartTime / speed));
			camera.transform.localRotation = Quaternion.Lerp(camera.transform.localRotation, Quaternion.Euler(new Vector3(0f, 180f, 0f)), (_StartTime / speed));
			_StartTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		camera.transform.localPosition = position;
		camera.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
	}

	#region BGFilter - Used to enable/disable the filter of the background
	/// <summary>
	/// Do stuff on the BG capture.
	/// </summary>
	/// <param name="active">Set if the BG is active.</param>
	/// <param name="capdestroy">Destroy the capture.</param>
	/// <param name="dialogueend">Used only when the dialogue actually ends.</param>
	/// <param name="value">The speed of the lerps.</param>
	private void BGFilter(bool active, bool capdestroy = false, bool dialogueend = false, float value = 7f)
	{
		if (active)
		{
			StartCoroutine(FilterLerpOpacityIn(value));
		}
		else
		{
			StartCoroutine(FilterLerpOpacityOut(value, capdestroy, dialogueend));
		}
	}
	private IEnumerator FilterLerpOpacityIn(float value)
	{
		DialogueHandler.OnTransition = true;
		PanelCG.alpha = 1f;
		BGCaptureCG.alpha = 1f;

		Opacity.SetFloat("_MainAlpha", 0.744f);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		if (Panels.GetCurrentAnimatorClipInfo(0).Length > 0)
		{
			yield return new WaitForSeconds(Panels.GetCurrentAnimatorClipInfo(0)[0].clip.length);
		}

		DialogueHandler.Advance = true;
		DialogueEventSystem.TriggerEvent("LinePass", DialogueHandler._currentLineNum);
		Opacity.SetFloat("_MainAlpha", 0.744f);
		DialogueHandler.OnTransition = false;
		yield break;
	}
	private IEnumerator FilterLerpOpacityOut(float value, bool destroycap, bool enddialogue)
	{
		DialogueHandler.OnTransition = true;
		ResetAllTriggers();
		Panels.SetTrigger("DialogueEnd");
		yield return new WaitForSeconds(1f);

		ReturnCamToOrigParent();
		DialogueEventSystem.TriggerEvent("DisableCharacterHighlight");

		BGCaptureCG.alpha = 1f;
		do
		{
			BGCaptureCG.alpha -= value * Time.deltaTime;
			yield return new WaitForSeconds(0.01f);
		} while (BGCaptureCG.alpha > 0f);
		BGCaptureCG.alpha = 0f;

		yield return new WaitForSeconds(0.5f);

		float BGOpacity = 0.744f;
		do
		{
			BGOpacity -= value * Time.deltaTime;
			Opacity.SetFloat("_MainAlpha", BGOpacity);
			yield return new WaitForSeconds(0.01f);
		} while (Opacity.GetFloat("_MainAlpha") > 0f);
		Opacity.SetFloat("_MainAlpha", 0f);
		PanelCG.alpha = 0f;

		MainCam.SetActive(true);
		if (destroycap) Destroy(_Capture);
		if (enddialogue)
		{
			DialogueHandler.InDialog = false;
			DialoguePlayer.current.DRCursor.SetActive(true);
			Focused[0] = null;
			Focused[1] = null;
			Focused[2] = null;
			DialogueHandler.EndDialogue();
		}
		DialogueHandler.OnTransition = false;
		this.gameObject.SetActive(false);
		yield break;
	}
	#endregion

	#region Shake - Shake the panel camera (Event)
	/// <summary>
	/// Shake the panel camera
	/// </summary>
	/// <param name="camnum"></param>
	private void Shake(int camnum)
	{
		Camera cam = PanelCameras[camnum].GetComponent<Camera>();
		StartCoroutine(Shakey(cam));
	}
	private IEnumerator Shakey(Camera cam)
	{
		float magnitude = 0.015f;
		float duration = 0.1f;
		float elapsed = 0.0f;

		Vector3 originalCamPos = cam.transform.localPosition;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;

			float percentComplete = elapsed / duration;
			float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

			float x = UnityEngine.Random.value * 2.0f - 1.0f;
			float y = UnityEngine.Random.value * 2.0f - 1.0f;
			x *= magnitude * damper;
			y *= magnitude * damper;

			cam.transform.localPosition = cam.transform.localPosition + new Vector3(x, y);

			if (Input.GetButton("Cancel"))
				yield break;

			yield return null;
		}

		cam.transform.localPosition = originalCamPos;
	}
	#endregion

	private void ResetAllTriggers()
	{
		foreach (var param in Panels.parameters)
		{
			if (param.type == AnimatorControllerParameterType.Trigger)
			{
				Panels.ResetTrigger(param.name);
			}
		}
	}
}