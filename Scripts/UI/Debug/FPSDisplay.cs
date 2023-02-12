using System.Collections;
using UnityEngine;

using TMPro;
using UnityEngine.Events;

public class FPSDisplay : MonoBehaviour
{
    UnityEvent m_MyEvent = new UnityEvent();
    public TextMeshProUGUI FPSText;
    public RectTransform ErrorPanel;
    public TextMeshProUGUI ErrorText;
    float deltaTime = 0.0f;

    void Start()
    {
        ErrorPanel.anchoredPosition = new Vector2(0, -45f);
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fps = Mathf.Round(fps);
        FPSText.text = "FPS: " + fps;
    }

    public void ShowError(string error)
    {
        StartCoroutine(ErrorWait(error));
    }

    IEnumerator ErrorWait(string error)
    {
        float _time = 5f;
        float _elaspedtime = 0f;
        Vector3 pos1 = new Vector3(0f, -45.5f);
        Vector3 pos2 = new Vector3(0f, 29.5f);

        ErrorText.text = error;
        do
        {
            ErrorPanel.anchoredPosition = Vector3.Lerp(pos1, pos2, _elaspedtime / _time);
            _elaspedtime += _time * 6f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        } while (_elaspedtime < _time);
        ErrorPanel.anchoredPosition = pos2;

        _elaspedtime = 0f;
        yield return new WaitForSeconds(5f);

        do
        {
            ErrorPanel.anchoredPosition = Vector3.Lerp(pos2, pos1, _elaspedtime / _time);
            _elaspedtime += _time * 6f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        } while (_elaspedtime < _time);
        yield break;
    }
}