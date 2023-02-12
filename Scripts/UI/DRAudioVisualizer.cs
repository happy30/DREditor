//Audio Visualizer script by SeleniumSoul for DREditor
using UnityEngine;
using TMPro;

/// <summary>
/// UI Script used to display the music visualizer typically on the top right corner of the screen.
/// </summary>
public class DRAudioVisualizer : MonoBehaviour
{
    public AudioSource MusicPlayer;
    public GameObject MusicBar;

    public int BarCount;
    public float BarSensitivity, BarSmoothness, BarMaxHeight = 5f, GapWidth;

    public FFTWindow SampleMethod;
    public int SampleCount = 512;

    private float[] FreqBands, AudioSamples;
    private GameObject[] MusicBars;

    public RectTransform TitleMarquee;
    public TextMeshProUGUI TitleTMPro;
    public float ScrollSpeed = 0.5f;

    private RectTransform TitleMask;
    private float _maskboundleft, _maskboundright;

    void Start()
    {
        if (!MusicPlayer) MusicPlayer = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioSource>();

        FreqBands = new float[BarCount];
        MusicBars = new GameObject[BarCount];
        AudioSamples = new float[SampleCount];
        TitleMask = TitleMarquee.transform.parent.GetComponent<RectTransform>();
        TitleTMPro = TitleMarquee.GetComponent<TextMeshProUGUI>();

        MusicBars[0] = MusicBar;

        for (int x = 1; x < BarCount; x++)
        {
            RectTransform MusicBarsPos = MusicBars[x-1].GetComponent<RectTransform>();

            GameObject _duplicatebar = Instantiate(MusicBars[0]);
            RectTransform _dbrecttransform = _duplicatebar.GetComponent<RectTransform>();
            _duplicatebar.transform.SetParent(MusicBars[0].transform.parent);
            _dbrecttransform.localScale = Vector3.one;
            _dbrecttransform.anchoredPosition = MusicBarsPos.anchoredPosition + new Vector2(MusicBarsPos.sizeDelta.x + GapWidth, 0f);
            _duplicatebar.name = "MusicBar" + x;
            MusicBars[x] = _duplicatebar;
        }

        _maskboundright = TitleMask.sizeDelta.x / 2f;
        _maskboundleft = -_maskboundright;
    }

    void Update()
    {
        MusicPlayer.GetSpectrumData(AudioSamples, 0, SampleMethod);

        CalculateFreqBands();
    }

    void FixedUpdate()
    {
        for (int samples = 0; samples < BarCount; samples++)
        {
            float _freqband;
            RectTransform MusicBarsScale = MusicBars[samples].GetComponent<RectTransform>();
            _freqband = FreqBands[samples] * (10f * BarSensitivity);
            _freqband = Mathf.Clamp(_freqband, 0f, BarMaxHeight);
            MusicBarsScale.localScale = Vector3.Lerp(MusicBarsScale.localScale, new Vector3(1f, _freqband, 1f), Time.deltaTime * BarSmoothness);
        }

        TitleMarquee.anchoredPosition -= Vector2.right * ScrollSpeed;

        if (TitleMarquee.anchoredPosition.x < _maskboundleft - (TitleMarquee.sizeDelta.x / 2f))
        {
            TitleMarquee.anchoredPosition = new Vector2(_maskboundright + (TitleMarquee.sizeDelta.x / 2f), 0f);
        }
    }

    private void CalculateFreqBands()
    {
        float _bandrange = AudioSamples.Length / SampleCount;

        for (int x = 0; x < BarCount; x++)
        {
            float _summation = 0f;
            for (int y = 0; y < _bandrange; y++)
            {
                _summation += AudioSamples[(int)(y + (_bandrange * x))];
            }

            float _average = _summation / _bandrange;
            FreqBands[x] = _average;
        }
    }

    public void ChangeBGMName(string name)
    {
        TitleTMPro.text = name;
    }
}
