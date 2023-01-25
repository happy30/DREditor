using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMPMarquee : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI tmp = null;

    [SerializeField] float speed = 0.75f;

    private float speedFactor = 1e2f;

    /**
     * TODO: X + width isn't correct apparently. Hardcode for now.
     */

    //[SerializeField]
    //private RectTransform maskRect = null;
    [SerializeField] float resetOn = -10;
    [SerializeField] float resetX = 180.0f;

    void Update()
    {
        if (tmp.rectTransform.localPosition.x  < resetOn + (-tmp.rectTransform.sizeDelta.x * 2))
        {
            tmp.rectTransform.localPosition = new Vector3(resetX,
                tmp.rectTransform.localPosition.y, tmp.rectTransform.localPosition.z);
        }
        var translate = new Vector3(-speed * Time.deltaTime * speedFactor, 0, 0);
        tmp.rectTransform.Translate(translate);
    }
    public void MarqueeReset(string songName)
    {
        var translate = new Vector3(-speed * Time.deltaTime * speedFactor, 0, 0);
        tmp.rectTransform.Translate(translate);
        tmp.text = songName;
    }
}
