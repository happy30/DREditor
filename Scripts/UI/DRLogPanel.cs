using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DRLogPanel : MonoBehaviour, ISelectHandler
{
    public Image NamePanel;
    public TextMeshProUGUI NameField;
    public TextMeshProUGUI TextField;
    public RawImage Portait;
    public GameObject VoiceIcon;

    private CanvasGroup _nameCanGP, _voiceCanGP;

    public void OnEnable()
    {
        _nameCanGP = NamePanel.gameObject.GetComponent<CanvasGroup>();
        _voiceCanGP = VoiceIcon.GetComponent<CanvasGroup>();

        if (NameField.text == "" || NameField.text == " ") _nameCanGP.alpha = 0;
        else _nameCanGP.alpha = 1;

        if (GetComponent<AudioSource>().clip == null) _voiceCanGP.alpha = 0;
        else _voiceCanGP.alpha = 1;
    }

    public void OnSelect(BaseEventData eventData)
    {
        //Warning: Extremely hacky way of getting the Main Parent Component
        transform.parent.parent.parent.parent.GetComponent<DRBacklog>()?.PlaySFX();
    }
}
