using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioVisualizer : MonoBehaviour
{
    [SerializeField] Image[] blocks;
    [SerializeField] float heightScale;
    [SerializeField] int expScale;
    // Start is called before the first frame update
    void Start()
    {
        if (!SoundManager.instance.MusicSource)
        {
            Debug.LogError("You're SoundManager is missing a Music Source!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        float[] spectrum = new float[256];
        
        SoundManager.instance.MusicSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        float height;
        for (int i = 0; i < blocks.Length; ++i)
        {
            int section = expScale * i + 2;
            float level = lin2dB(spectrum[section]);
            height = (80 + level);

            height *= heightScale;

            Vector2 v = blocks[i].rectTransform.sizeDelta;
            v.y = Mathf.Lerp(v.y, height, 0.1f);
            blocks[i].rectTransform.sizeDelta = v;
        }
    }

    float lin2dB(float linear)
    {
        return Mathf.Clamp(Mathf.Log10(linear) * 20.0f, -80.0f, 0.0f);
    }
}
