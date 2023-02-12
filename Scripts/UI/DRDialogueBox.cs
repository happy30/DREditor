//DialogueBox script by SeleniumSoul for DR:Distrust

using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DREditor.Trial;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;

/// <summary>
/// UI Script that handles the dialogue box for the story text.
/// </summary>
public class DRDialogueBox : MonoBehaviour
{
	//Note: This will be changed after the protagonist asset in DREditor is finished.
	public string Protagonist = "Rantaro Amami";

	[Tooltip("Check if this dialogue box is used for CG.")]
	public bool IsCGDialogueBox;
	[Tooltip("Amount of time the text will finish writing. (Not in seconds)")]
	[Range(1f, 15f)] public float WriteSpeed;

	public AudioClip _nextLineSound;
	public AudioClip _TruthBulletShowSound;
	public AudioClip _TruthBulletHideSound;

	[SerializeField] private CanvasGroup NBTextBoxCG;
	[SerializeField] private TextMeshProUGUI NBText, TBText;
	[SerializeField] private RectTransform NBTextTransform;
	[SerializeField] private Image TruthBulletImage;
	private Animator BoxAnimate;

	private DRInput _controls;
	private Coroutine TextCoroutine;

	void Awake()
	{
		if (!BoxAnimate) BoxAnimate = GetComponent<Animator>();

		_controls = new DRInput();
	}

	private void Start()
	{
		gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		DialogueEventSystem.StartListening("TruthBulletDisplay", TruthBulletDisplay);

		Auto(DialogueHandler._auto);
		_controls.Enable();

		foreach (var trgr in BoxAnimate.parameters)
		{
			if (trgr.type == AnimatorControllerParameterType.Trigger)
			{
				BoxAnimate.ResetTrigger(trgr.name);
			}
		}
	}

	private void OnDisable()
	{
		DialogueEventSystem.StopListening("TruthBulletDisplay", TruthBulletDisplay);

		DialogueHandler.IsWriting = false;
		_controls.Disable();
	}

	private void Update()
	{
		Auto(DialogueHandler._auto || DialogueHandler._skip);
	}
    public void Auto(bool _on)
    {
		BoxAnimate.SetBool("Auto", _on);
	}

	public void UseText(string name, string line, bool start = false)
	{
		if (TextCoroutine != null) StopCoroutine(TextCoroutine);
		if (IsCGDialogueBox) DialogueHandler.Advance = true;
        NameSetting(name);
		NBText.text = name;
		DialogueEventSystem.TriggerEvent("PlaySystemSFX", _nextLineSound);
		BoxAnimate.SetBool("Finish", false);
		if (!start) BoxAnimate.SetTrigger("Next");
		TextCoroutine = StartCoroutine(TypeText(line));
	}

	public void NameSetting(string name, float xpos = 678f)
	{
		if (name == Protagonist)
		{
			NBTextBoxCG.alpha = 1;
			NBTextTransform.anchoredPosition = new Vector2(xpos, NBTextTransform.anchoredPosition.y);
			NBTextTransform.localScale = new Vector3(-1, 1, 1);
			NBTextTransform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
		}
		else if (name == " " || name == "")
		{
			NBTextBoxCG.alpha = 0;
		}
		else
		{
			NBTextBoxCG.alpha = 1;
			NBTextTransform.anchoredPosition = new Vector2(-xpos, NBTextTransform.anchoredPosition.y);
			NBTextTransform.localScale = Vector3.one;
			NBTextTransform.GetChild(0).GetComponent<RectTransform>().localScale = Vector3.one;
		}
	}

	//Tags:
	//<emp> </emp> = <material="Nunito_Emphasis"> = Emphasis
	//<nar> </nar> = <material="Nunito_Narration"> = Narration
	//<tut> </tut> = <material="Nunito_Tutorial"> = Tutorial

	IEnumerator TypeText(string text) {
		text = Regex.Replace(text, "<emp>", "<material=\"Nunito_Emphasis\">");
		text = Regex.Replace(text, "</emp>", "</material>");
		text = Regex.Replace(text, "<nar>", "<material=\"Nunito_Narration\">");
		text = Regex.Replace(text, "</nar>", "</material>");
		text = Regex.Replace(text, "<tut>", "<material=\"Nunito_Tutorial\">");
		text = Regex.Replace(text, "</tut>", "</material>");

		DialogueHandler.IsWriting = true;

		TBText.text = text;
		string _notag = Regex.Replace(text, "<.*?>", string.Empty);

		for (int x = 0; x <= _notag.Length; x++)
		{
			TBText.maxVisibleCharacters = x;

			if (TBText.maxVisibleCharacters > 1 && (_controls.Global.Confirm.triggered || _controls.Global.Cancel.triggered) && DialogueHandler.IsWriting)
			{
				TypeWhole(_notag.Length);
				yield break;
			}

			yield return new WaitForSeconds(0.05f / WriteSpeed * Time.deltaTime);
		}
		BoxAnimate.SetBool("Finish", true);

		DialogueHandler.IsWriting = false;
		yield break;
	}

	private void TypeWhole(int length)
	{
		TBText.maxVisibleCharacters = length;
		DialogueHandler.IsWriting = false;
		BoxAnimate.SetBool("Finish", true);
	}

	private void TruthBulletDisplay(object bullet)
	{
		DTBTuple _bulletObject = (DTBTuple)bullet;
		switch (_bulletObject.TBChoice)
		{
			case DTBChoice.Show:
				TruthBulletImage.sprite = _bulletObject.TB.Picture;
				DialogueEventSystem.TriggerEvent("PlaySystemSFX", _TruthBulletShowSound);
				BoxAnimate.SetTrigger("ShowTruthBullet");
				break;
			case DTBChoice.Hide:
				DialogueEventSystem.TriggerEvent("PlaySystemSFX", _TruthBulletHideSound);
				BoxAnimate.SetTrigger("HideTruthBullet");
				break;
			default:
				break;
		}

	}
}
