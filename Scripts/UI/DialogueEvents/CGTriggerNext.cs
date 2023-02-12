using System.Collections;
using UnityEngine;
using DREditor.Dialogues.Events;
public class CGTriggerNext : MonoBehaviour
{
    public float fadespeed = 0.5f;
    private CanvasGroup _canvasGroup;

    private void OnEnable()
    {
        DialogueEventSystem.StartListening("CG_TriggerNextState", TriggerNextState);
        DialogueEventSystem.StartListening("CG_FadeOut", CG_FadeOut);
    }

    private void OnDisable()
    {
        DialogueEventSystem.StopListening("CG_TriggerNextState", TriggerNextState);
        DialogueEventSystem.StopListening("CG_FadeOut", CG_FadeOut);
    }

    private void TriggerNextState(object value = null)
    {
        GetComponent<Animator>().SetTrigger("Next");
    }

    public void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        StartCoroutine(FadeAnimation(fadespeed, 0f, 1f));
    }

    private void CG_FadeOut(object value = null)
    {
        StartCoroutine(FadeAnimation(fadespeed, 1f, 0f));
    }

    private IEnumerator FadeAnimation(float speed, float startalpha, float endalpha)
    {
        float _startTime = 0f;

        while (_startTime < speed)
        {
            _canvasGroup.alpha = Mathf.Lerp(startalpha, endalpha, _startTime / speed);
            _startTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (startalpha == 1f)
        {
            Destroy(gameObject);
        }
        else
        {
            _canvasGroup.alpha = 1f;
        }
    }
}
