// Dialogue Player Script by SeleniumSoul for DREditor
// Updated at 08/21/2021
// Handles all events for dialogues. Class Trial is not supported at this time.

// If desired, this class' Debug Logs should be <color=blue> for color-coding.

using System;
using System.Collections;
using UnityEngine;
using DREditor.Characters;
using DREditor.Dialogues.Events;

/// <summary>
/// Component used for dialogue rendering. Use this as a component on the main SYSTEM gameObject.
/// </summary>
namespace DREditor.Dialogues
{
	public class DialoguePlayer : MonoBehaviour
	{
		static public DialoguePlayer current;

		public bool PropMode = false;

		public float AutoWaitTime = 1f;
		private float SkipWaitTime = 0.4f;

		[HideInInspector] public UIDialogueSubmanager SayBoxManager;
		[HideInInspector] public GameObject DiaPanels, DRCursor;
		[HideInInspector] public GameObject CurrentCharacter;

		[SerializeField] private AudioClip FlashSFX;

		private UnityEngine.Camera CurrentMainCamera;
		private DRDialogueBox SayBox;
		private DRInput _controls;

		private DRSpriteBillboard DRSB = null;
		private DRBillboardProperties DRBP = null;

		private Coroutine AutoC;

		private void Awake()
		{
			if (current == null)
			{
				current = this;
			}
			else
			{
				Destroy(this);
			}

			if (!CurrentMainCamera) CurrentMainCamera = UnityEngine.Camera.main;

			_controls = new DRInput();
			_controls.Global.Confirm.started += ctx =>
			{
				if (DialogueHandler._skip || DialogueHandler._auto)
				{
					DialogueHandler._skip = false;
					DialogueHandler._auto = false;
					if (AutoC != null)
					{
						StopCoroutine(AutoC);
					}
					SayBox.Auto(DialogueHandler._auto);
					return;
				}

				OnConfirm();
			};
			_controls.Global.Skip.performed += ctx =>
			{ 
				if (DialogueHandler.InDialog && !DialogueHandler.InMenu && DialogueHandler.Advance)
				{
					if (DialogueHandler._auto) DialogueHandler._auto = false;
					DialogueHandler._skip = !DialogueHandler._skip;
					SayBox.Auto(DialogueHandler._skip);

					if (DialogueHandler._skip)
					{
						AutoC = StartCoroutine(AutoCoroutine(SkipWaitTime));
					}
					else
					{
						if (AutoC != null)
						{
							StopCoroutine(AutoC);
						}
					}
				}
			};
			_controls.Global.Auto.performed += ctx =>
			{
				if (DialogueHandler._skip)
				{
					DialogueHandler._skip = false;
					DialogueHandler._auto = false;
					return;
				}

				if (DialogueHandler.InDialog && !DialogueHandler.InMenu && DialogueHandler.Advance)
				{
					DialogueHandler._auto = !DialogueHandler._auto;
					SayBox.Auto(DialogueHandler._auto);

					if (DialogueHandler._auto)
					{
						AutoC = StartCoroutine(AutoCoroutine(AutoWaitTime));
					}
					else
					{
						if (AutoC != null)
						{
							StopCoroutine(AutoC);
						}
					}
				}
			};
		}

		private void Start()
		{
			if (!CurrentMainCamera) CurrentMainCamera = UnityEngine.Camera.main;
		}

		private void OnEnable()
		{
			_controls.Enable();

			DialogueEventSystem.StartListening("StartDialogue", StartDialogue);
			DialogueEventSystem.StartListening("LinePass", LinePass);

			//Special Events
			DialogueEventSystem.StartListening("ChangeFocus", ChangeFocus);
			DialogueEventSystem.StartListening("ShowCG", ShowCG);
			DialogueEventSystem.StartListening("CharacterLeave", LeaveSequence);
			DialogueEventSystem.StartListening("SpriteFlash", SpriteFlash);
		}

		private void OnDisable()
		{
			_controls.Disable();

			DialogueEventSystem.StopListening("StartDialogue", StartDialogue);
			DialogueEventSystem.StopListening("LinePass", LinePass);

			//Special Events
			DialogueEventSystem.StopListening("ChangeFocus", ChangeFocus);
			DialogueEventSystem.StopListening("ShowCG", ShowCG);
			DialogueEventSystem.StopListening("CharacterLeave", LeaveSequence);
			DialogueEventSystem.StopListening("SpriteFlash", SpriteFlash);
		}

		public void OnConfirm()
		{
			if (DialogueHandler.InDialog && !DialogueHandler.InMenu && DialogueHandler.Advance && !DialogueHandler.OnTransition)
			{
				if (!DialogueHandler.IsWriting)
				{
					if (!DialogueHandler._auto)
					{
						ContinueDialogue();
					}
					else
					{
						DialogueHandler._auto = false;
						SayBox.Auto(DialogueHandler._auto);

						if (AutoC != null)
						{
							StopCoroutine(AutoC);
						}
					}
				}
			}
		}

		private void CamNullCheck()
		{
			if (!CurrentMainCamera) CurrentMainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UnityEngine.Camera>();
		}

		private void StartDialogue(object value = null)
		{
			CamNullCheck();
			if (DRCursor) DRCursor.SetActive(false);

			//DialogueHandler.Skippable = true;
			DialogueHandler.InDialog = true;

			DialogueHandler._dialogueLength = DialogueHandler.DialogueAsset.Lines.Count;
			DialogueHandler._currentLineNum = 0;

			switch ((int)DialogueHandler.DialogueAsset.DialogueMode)
			{
				case 0: //Normal
					SayBox = SayBoxManager.DialogueBox.GetComponent<DRDialogueBox>();
					if (DialogueHandler.DialogueAsset.ShowDialoguePanel) // With Dialogue Panels
					{
						CurrentMainCamera.enabled = false;
						if (DiaPanels) DiaPanels.SetActive(true);
						//DialogueEventSystem.TriggerEvent("DiaPanelInitialize");
					}
					else // Without Dialogue Panels
					{
						SayBox.gameObject.SetActive(true);
						DialogueHandler.Advance = true;
						DialogueEventSystem.TriggerEvent("LinePass", DialogueHandler._currentLineNum);
					}
					break;
				case 1: //CG - without Dialogue Panels
					SayBox = SayBoxManager.DialogueBox_CG.GetComponent<DRDialogueBox>();
					DialogueHandler.OnTransition = false;
					SayBox.gameObject.SetActive(true);
					DialogueEventSystem.TriggerEvent("LinePass", DialogueHandler._currentLineNum);
					break;
				default:
					Debug.Log("DREditor (DialoguePlayer): Unable to identify dialogue mode. Defaulting to No Panel.");
					SayBox = SayBoxManager.DialogueBox.GetComponent<DRDialogueBox>();
					SayBox.gameObject.SetActive(true);
					DialogueEventSystem.TriggerEvent("LinePass", DialogueHandler._currentLineNum);
					break;
			}
		}

		private void ContinueDialogue()
		{
			DialogueHandler._currentLineNum++;

			if (DialogueHandler._currentLineNum <= DialogueHandler._dialogueLength - 1)
			{
				EnableDiaPanels();

				LinePass(DialogueHandler._currentLineNum);

				if (DialogueHandler._auto && !DialogueHandler._currentLine.Text.Contains("[WAIT]"))
				{
					AutoC = StartCoroutine(AutoCoroutine(AutoWaitTime));
				}
				else if (DialogueHandler._skip && !DialogueHandler._currentLine.Text.Contains("[WAIT]"))
				{
					AutoC = StartCoroutine(AutoCoroutine(SkipWaitTime));
				}
			}
			else
			{
				if (DialogueHandler.DialogueAsset.DirectTo.Enabled)
				{
					DirectTo();

					if (DialogueHandler._auto && !DialogueHandler._currentLine.Text.Contains("[WAIT]"))
					{
						AutoC = StartCoroutine(AutoCoroutine(AutoWaitTime));
					}
					else if (DialogueHandler._skip && !DialogueHandler._currentLine.Text.Contains("[WAIT]"))
					{
						AutoC = StartCoroutine(AutoCoroutine(SkipWaitTime));
					}
				}
				else if (DialogueHandler.DialogueAsset.Choices.Count != 0)
				{
					//Insert Choice code here.
				}
				else
				{
					EndDialogue();
				}
			}
		}

		private void LinePass(object _DPlinenum)
		{
			bool _firstLine = false;
			DialogueHandler._currentLine = DialogueHandler.DialogueAsset.Lines[(int)_DPlinenum];

			string charname = "";

			if (DialogueHandler._currentLine.Speaker != null)
			{
				if (DialogueHandler._currentLine.Speaker is Student) charname = DialogueHandler._currentLine.Speaker.FirstName + " " + DialogueHandler._currentLine.Speaker.LastName;
				else if (DialogueHandler._currentLine.Speaker is Headmaster) charname = DialogueHandler._currentLine.Speaker.FirstName;

				Debug.Log("DREditor (DialoguePlayer): Processing " + charname + "'s line.");
				if ((int)DialogueHandler.DialogueAsset.DialogueMode != 1 && !PropMode)
				{
					CurrentCharacter = GameObject.Find(charname);
					DRSB = CurrentCharacter.GetComponent<DRSpriteBillboard>();
					DRBP = CurrentCharacter.GetComponent<DRBillboardProperties>();
				}
			}

			//Enable DialogueBox
			if (SayBox.gameObject.activeSelf == false)
			{
				SayBox.gameObject.SetActive(true);
				_firstLine = true;
			}

			//Voices
			if (DialogueHandler._currentLine.VoiceSFX != null && !DialogueHandler._skip)
			{
				DialogueEventSystem.TriggerEvent("PlayVoice", DialogueHandler._currentLine.VoiceSFX);
			}

			//SFX
			if (DialogueHandler._currentLine.SFX.Count > 0 && !DialogueHandler._skip)
			{
				foreach (AudioClip aud in DialogueHandler._currentLine.SFX)
				{
					if (aud != null) DialogueEventSystem.TriggerEvent("PlaySFX", aud);
				}
			}

			//Dialogue Events
			if (DialogueHandler._currentLine.DiaEvents.Count > 0)
			{
				for (int d = 0; d < DialogueHandler._currentLine.DiaEvents.Count; d++)
				{
					DialogueHandler._currentLine.DiaEvents[d].TriggerDialogueEvent();
				}
			}

			//Sprites
			if (DialogueHandler._currentLine.Expression?.Sprite != null)
			{
				if (DRSB) DRSB.ChangeExpression(DialogueHandler._currentLine.Expression?.Sprite);
			}

			//If there's no text inside the line, just do the dialogue events and automatically go to the next line.
			if (DialogueHandler._currentLine.Text == "")
			{
				ContinueDialogue();

				return;
			}
			else if (DialogueHandler._currentLine.Text.Contains("[WAIT]"))
			{
				return;
			}

			//Dialogue Text
			if (DialogueHandler._currentLine.AliasNumber > 0 && DialogueHandler._currentLine.Speaker != null)
			{
				charname = DialogueHandler._currentLine.Speaker.Aliases[DialogueHandler._currentLine.AliasNumber - 1].Name;
			}
			SayBox.UseText(charname, DialogueHandler._currentLine.Text, _firstLine);

			//Pass Dialogue Lines to a Backlog File
			if (DialogueHandler.BLFile) DialogueHandler.BLFile.AddLine(DialogueHandler._currentLine.Speaker, DialogueHandler._currentLine.Text, DialogueHandler._currentLine.VoiceSFX);
		}

		private void EndDialogue()
		{
			Debug.Log("DREditor (DialoguePlayer): Ending Dialogue");

			CurrentMainCamera.enabled = true;
			DialogueHandler._currentLineNum = 0;
			DialogueHandler._dialogueLength = 0;

			DialogueHandler.Advance = false;
			DialogueHandler.Skippable = false;

			DialogueHandler._skip = false;
			DialogueHandler._auto = false;

			if (!DiaPanels || DiaPanels.activeSelf == false)
			{
				DialogueHandler.InDialog = false;
				SayBox.gameObject.SetActive(false);
				StartCoroutine(Waiting());
				DialogueEventSystem.TriggerEvent("DisableCharacterHighlight");
			}
			else
			{
				DialogueEventSystem.TriggerEvent("DiaPanelTerminate");
				SayBox.gameObject.SetActive(false);
			}

			CurrentCharacter = null;

			Resources.UnloadUnusedAssets();
		}

		private void DirectTo()
		{
			SayBox.gameObject.SetActive(false);

			DialogueHandler._currentLineNum = DialogueHandler.DialogueAsset.DirectTo.NewDialogueIndex;
			DialogueHandler.DialogueAsset = DialogueHandler.DialogueAsset.DirectTo.NewDialogue;

			DialogueHandler._dialogueLength = DialogueHandler.DialogueAsset.Lines.Count;

			switch ((int)DialogueHandler.DialogueAsset.DialogueMode)
			{
				case 0:
					SayBox = SayBoxManager.DialogueBox.GetComponent<DRDialogueBox>();
					break;
				case 1:
					SayBox = SayBoxManager.DialogueBox_CG.GetComponent<DRDialogueBox>();
					break;
				default:
					SayBox = SayBoxManager.DialogueBox.GetComponent<DRDialogueBox>();
					break;
			}
			
			SayBox.gameObject.SetActive(true);

			EnableDiaPanels();
			LinePass(DialogueHandler._currentLineNum);
		}

		private IEnumerator Waiting()
		{
			yield return new WaitForSeconds(0.1f);
			if (DRCursor) DRCursor.SetActive(true);
			yield break;
		}

		private IEnumerator AutoCoroutine(float WaitTime)
		{
			float _linelength = DialogueHandler._currentLine.Text.Length;
			float _waitTime;

            if (WaitTime >= 0.5f)
            {
                _waitTime = (_linelength * 0.05f * 2f) + WaitTime;
				_waitTime = Mathf.Clamp(_waitTime, 2f, 100f);
            }
            else
            {
                _waitTime = WaitTime * Time.deltaTime * 50f;
            }

            Debug.Log($"Dialogue will wait for {_waitTime} seconds.");
			yield return new WaitForSeconds(_waitTime);

			ContinueDialogue();

			yield break;
		}

		private void EnableDiaPanels()
		{
			if (DiaPanels.activeSelf == false && (int)DialogueHandler.DialogueAsset.DialogueMode == 0 && DialogueHandler.DialogueAsset.ShowDialoguePanel)
			{
				DiaPanels.GetComponent<DRDialoguePanels>().Immediate = true;
				CurrentMainCamera.enabled = false;
				DiaPanels.SetActive(true);
			}
		}

		//Dialogue Events
		/// <summary>
		/// Dialogue Event: Change the focus of the panel camera to the current character.
		/// </summary>
		/// <param name="values"></param>
		private void ChangeFocus(object values = null)
		{
			(int PNum, Vector3 CT) _convert = (ValueTuple<int, Vector3>)values;
			(int, GameObject, Vector3) newvalues;

			if (_convert.CT == Vector3.zero && CurrentCharacter != null)
			{
				if (!PropMode) newvalues = (_convert.PNum, CurrentCharacter, DRBP.CameraFocusPosition);
				else newvalues = (_convert.PNum, CurrentCharacter.transform.GetChild(0).gameObject, Vector3.zero);
			}
			else
			{
				newvalues = (_convert.PNum, CurrentCharacter, _convert.CT);
			}

			DialogueEventSystem.TriggerEvent("ChangeCharacterFocus", newvalues);
		}

		/// <summary>
		/// Dialogue Event: Show a CG on the screen.
		/// </summary>
		/// <param name="values"></param>
		private void ShowCG(object values = null)
		{
			GameObject _CG = (GameObject)values;
			Instantiate(_CG);
		}

		/// <summary>
		/// Dialogue Event: Show a sequence where a character leaves in the middle of the conversation.
		/// </summary>
		/// <param name="values"></param>
		private void LeaveSequence(object values = null)
		{
			CharacterSpot _cs = CurrentCharacter.transform.parent.parent.GetComponent<CharacterSpot>();
			DialogueHandler.Advance = false;

			_controls.Disable();
			DialogueHandler._skip = false;
			DialogueHandler._auto = false;

			if (!DiaPanels || DiaPanels.activeSelf == false)
			{
				SayBox.gameObject.SetActive(false);
				StartCoroutine(Waiting());
				DialogueEventSystem.TriggerEvent("DisableCharacterHighlight");
			}
			else
			{
				StartCoroutine(_LeaveSequence(_cs));
				SayBox.gameObject.SetActive(false);
			}
		}
		private IEnumerator _LeaveSequence(CharacterSpot _cs)
		{
			CurrentMainCamera.enabled = true;
			DialogueEventSystem.TriggerEvent("PanelActive", false);

			yield return new WaitForSeconds(3f);

			_cs.Disable();

			yield return new WaitForSeconds(1f);

			DialogueHandler.Advance = true;
			_controls.Enable();
			ContinueDialogue();
			yield return null;
		}

		/// <summary>
		/// Dialogue Event: Flash the sprite at this line.
		/// </summary>
		/// <param name="values"></param>
		private void SpriteFlash(object value = null)
		{
			if (DRSB != null)
			{
				DRSB.Flash(10, FlashSFX);
			}
		}
	}
}