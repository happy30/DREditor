//All-in-one Audio Manager script by SeleniumSoul for DREditor

using System.Collections;
using UnityEngine;
using DREditor.Dialogues.Events;
using DREditor.Audio;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] private float BGMVolume;
    [SerializeField] private AudioSource BGMSource;
    [SerializeField] private AudioSource VoiceSource;
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private AudioSource SystemSource;
    [SerializeField] private DRAudioVisualizer Visualizer;
    [SerializeField] private Playlist playlist;
    [SerializeField] private int currentBGMNum;

    private void Start()
    {
        DialogueEventSystem.StartListening("ChangeMusic", PlayBGM);
        if (!Visualizer) Visualizer = GameObject.Find("[UI]MusicVisualizer").GetComponent<DRAudioVisualizer>();

        if (!BGMSource.isPlaying)
        {
            ToggleVisualizer(false);
        }
    }

    private void OnEnable()
    {
        DialogueEventSystem.StartListening("PlayBGM", PlayBGM);
        DialogueEventSystem.StartListening("PlayVoice", PlayVoice);
        DialogueEventSystem.StartListening("PlaySFX", PlaySFX);
        DialogueEventSystem.StartListening("PlaySystemSFX", PlaySystemSFX);
    }

    private void OnDisable()
    {
        DialogueEventSystem.StopListening("PlayBGM", PlayBGM);
        DialogueEventSystem.StopListening("PlayVoice", PlayVoice);
        DialogueEventSystem.StopListening("PlaySFX", PlaySFX);
        DialogueEventSystem.StopListening("PlaySystemSFX", PlaySystemSFX);
    }

    /// <summary>
    /// Will be removed.
    /// </summary>
    private void Update()
    {
        //Debug.Log("BGM Time: " + BGMSource.time + " | TimeSamples: " + BGMSource.timeSamples);

        if (Input.GetKeyDown("p"))
        {
            BGMSource.time += 5f;
        }

        if (Input.GetKeyDown("o"))
        {
            BGMSource.time -= 5f;
        }
    }

    private void PlayBGM(object BGMnum)
    {
        int _num;
        float _fadeout = 0f;

        if (BGMnum is CMTuple _numpass)
        {
            _num = _numpass.MusicNum;
            _fadeout = _numpass.fadeOut;
        }
        else
        {
            _num = (int)BGMnum;
        }

        if ((_num + 1) == 0)
        {
            ToggleVisualizer(false);
            if (_fadeout > 0f)
            {
                StartCoroutine(FadeOutLerp(_fadeout));
            }
            else
            {
                BGMSource.Stop();
            }
        }
        else if ((_num + 1) != currentBGMNum)
        {
            BGMSource.volume = BGMVolume;
            if (!Visualizer.gameObject.activeSelf)
            {
                ToggleVisualizer(true);
            }

            BGMSource.clip = playlist.Musics[_num].BGM;
            BGMSource.Play();
            if (Visualizer) Visualizer.ChangeBGMName(playlist.Musics[_num].Title);
        }

        currentBGMNum = _num + 1;
    }

    private void PlaySFX(object sound)
    {
        SFXSource?.PlayOneShot((AudioClip)sound);
    }

    private void PlayVoice(object voice)
    {
        VoiceSource?.Stop();
        VoiceSource.clip = (AudioClip)voice;
        VoiceSource?.Play();
    }

    private void PlaySystemSFX(object sound)
    {
        SystemSource?.PlayOneShot((AudioClip)sound);
    }

    private void ToggleVisualizer(bool toggle)
    {
        if (toggle)
        {
            Visualizer.gameObject.SetActive(true);
        }
        else
        {
            //Play animation here
            Visualizer.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeOutLerp(float _fadeTime)
    {
        float _eT = 0f;

        while (_eT < _fadeTime)
        {
            BGMSource.volume = Mathf.Lerp(BGMVolume, 0f, (_eT / _fadeTime));
            _eT += Time.deltaTime;
            yield return null;
        }

        BGMSource.volume = 0f;
        BGMSource.Stop();
        yield return null;
    }
}
