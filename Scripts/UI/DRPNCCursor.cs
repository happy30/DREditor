using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;

public class DRPNCCursor : MonoBehaviour
{
	public AudioClip[] SFX = new AudioClip[2];
	public RectTransform Reticle;
	public TextMeshProUGUI NameText;
	public CanvasGroup NamePanel;
	private Animator _cursoranimator;
	private Camera _maincamera;

	public float _raycastDistance = 5f;
	private RaycastHit[] _curshit;
	private Ray _cursray;
	private DRBillboardProperties _properties;

	private DRInput _controls;
	[SerializeField] private Vector2 ReticlePosition;
	private Vector2 look;
	public float ReticleSensitivity;

	private void Awake()
	{
		_controls = new DRInput();

		ReticlePosition = new Vector2(Screen.currentResolution.width / 2f, Screen.currentResolution.height / 2f);

		_controls.PNCExplore.Look.performed += ctx =>
		{
			if (!DialogueHandler.InDialog && !DialogueHandler.InMenu) {
				look = ReticleSensitivity * ctx.ReadValue<Vector2>();
			}
		};
		_controls.PNCExplore.Look.canceled += ctx => { look = Vector2.zero; };
	}

	void Start()
	{
		if (DialoguePlayer.current.DRCursor == null) DialoguePlayer.current.DRCursor = gameObject;
		_maincamera = Camera.main;
		_cursoranimator = GetComponent<Animator>();
		NamePanel.alpha = 0f;

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void OnEnable()
	{
		_controls.Enable();
	}

	private void OnDisable()
	{
		_controls.Disable();
	}

	void PassThreadToHandler()
	{
		if (_properties) DialogueHandler.DialogueAsset = _properties.DialogThreadPass();
		else Debug.LogError("DREditor (PassThreadToHandler): Your code works, but I can't seem to find _properties... :(");
	}

	void Update()
	{
		ReticlePosition += look;
		ReticlePosition = new Vector2(Mathf.Clamp(ReticlePosition.x, 0f, Screen.currentResolution.width), Mathf.Clamp(ReticlePosition.y, 0f, Screen.currentResolution.height));
		Reticle.anchoredPosition = ReticlePosition;
		if (_maincamera == null) _maincamera = Camera.main;
		_curshit = Physics.RaycastAll(_maincamera.ViewportPointToRay(new Vector3(ReticlePosition.x / Screen.currentResolution.width, ReticlePosition.y / Screen.currentResolution.height)), _raycastDistance, 1 << 10 | 1 << 12);
		_cursray = _maincamera.ViewportPointToRay(new Vector3(ReticlePosition.x / Screen.currentResolution.width, ReticlePosition.y / Screen.currentResolution.height));

		for (int i = 0; i < _curshit.Length; ++i)
		{
			Renderer rend = _curshit[i].transform.GetComponent<Renderer>();
			MeshCollider meshCollider = _curshit[i].collider as MeshCollider;
			Texture2D tex = rend.material.mainTexture as Texture2D;
			Vector2 pixelUV = _curshit[i].textureCoord;
			pixelUV.x *= tex.width;
			pixelUV.y *= tex.height;

			if (tex.GetPixel((int)pixelUV.x, (int)pixelUV.y).a != 0f)
			{
				NameText.text = _curshit[i].transform.name;
				_properties = _curshit[i].transform.gameObject.GetComponent<DRBillboardProperties>();
				NamePanel.alpha = 0.8f;
				_cursoranimator.SetBool("Highlight", true);

				if (_controls.Global.Confirm.triggered && !DialogueHandler.InDialog &&
					!DialogueHandler.InMenu && !DialogueHandler.IsWriting &&
					!DialogueHandler.Skippable && !UIFade.OnScreenFade)
				{
					DialogueHandler.InDialog = true;
					if (_curshit[i].transform.tag == "Props")
					{
						DialoguePlayer.current.PropMode = true;
						DialoguePlayer.current.CurrentCharacter = _curshit[i].transform.gameObject;
					}
                    else
                    {
						DialoguePlayer.current.PropMode = false;
					}
					TriggerSound(1);
					_cursoranimator.SetTrigger("Click");

					//Pass: Use ClickAnim as Animation Event
				}
				return;
			}
			else
			{
				NamePanel.alpha = 0f;
				_cursoranimator.SetBool("Highlight", false);
			}
		}
		NamePanel.alpha = 0f;
		_cursoranimator.SetBool("Highlight", false);
	}

	public void ClickAnim() //Used as Animation Event
	{
		if (_properties != null && _properties.DialogThread[0] != null && _properties.DialogThread.Count != 0)
		{
			PassThreadToHandler();
			DialogueEventSystem.TriggerEvent("StartDialogue");
		}
		else
		{
			DialogueHandler.InDialog = false;
			Debug.LogError("DREditor (DRFPSCursor): Dialogue File Missing! Please set it up on DR Sprite Properties.");
			GameObject.Find("[UI]FPS&Console")?.GetComponent<FPSDisplay>()?.ShowError("DREditor Null Exception: Dialogue File Missing! Please set it up on DR Sprite Properties.");
			return;
		}
	}

	void TriggerSound(int mode)
	{
		GetComponent<AudioSource>().PlayOneShot(SFX[mode]);
	}
}
