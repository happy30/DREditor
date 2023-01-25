using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISwitcher : MonoBehaviour
{
    [SerializeField] private Sprite dailySprites;
    [SerializeField] private Sprite nightSprites;
    [SerializeField] private Sprite deadlySprites;

    private readonly Dictionary<GameManager.State, Sprite> _spritesDictionary = new Dictionary<GameManager.State, Sprite>();

    [HideInInspector] public Image currentSprites;
    [HideInInspector] public RawImage rawCurrentSprites;
    private void Start()
    {
        currentSprites = GetComponent<Image>();
        rawCurrentSprites = GetComponent<RawImage>();
        _spritesDictionary[GameManager.State.Daily] = dailySprites;
        _spritesDictionary[GameManager.State.Night] = nightSprites;
        _spritesDictionary[GameManager.State.Deadly] = deadlySprites;

        GameManager.StateChange += UIChange;
    }

    private void UIChange(GameManager.State state)
    {
        if (currentSprites != null)
            currentSprites.sprite = _spritesDictionary[state];
        if (rawCurrentSprites != null)
            rawCurrentSprites.texture = _spritesDictionary[state].texture;
    }
}
