using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class MCChoice : MonoBehaviour
{
    [SerializeField] Image arrowLeft = null;
    [SerializeField] Image arrowRight = null;
    [SerializeField] RectTransform choiceMask = null;
    [SerializeField] Image choiceBar = null;
    [SerializeField] Image glowImage = null;
    float duration => MultipleChoiceManager.instance.appearDuration;
    float appearDuration => MultipleChoiceManager.instance.selectDuration;
    public TMP_Text choiceText = null;
    Selectable selectable => choiceText.GetComponent<Selectable>();
    public void Show()
    {
        selectable.enabled = true;
        arrowLeft.DOFade(1, duration);
        arrowRight.DOFade(1, duration);

        arrowLeft.transform.DOLocalMoveX(-654, duration)
            .SetDelay(duration);
        arrowRight.transform.DOLocalMoveX(654, duration)
            .SetDelay(duration);
        choiceMask.DOSizeDelta(new Vector2(1406.98f, 100), duration)
            .SetDelay(duration);
        choiceText.DOFade(1, duration)
            .SetDelay(duration*2);
    }
    public void TempHide()
    {
        arrowLeft.DOFade(0, 1);
        arrowRight.DOFade(0, 1);
        choiceBar.DOFade(0, 1);
        choiceText.DOFade(0, 1);
    }
    public void TempShow()
    {
        arrowLeft.DOFade(1, 1);
        arrowRight.DOFade(1, 1);
        choiceBar.DOFade(1, 1);
        choiceText.DOFade(1, 1);
    }
    public void Hide()
    {
        choiceText.DOFade(0, duration);
        choiceMask.DOSizeDelta(new Vector2(0, 100), duration)
            .SetDelay(duration);
        choiceBar.DOFade(0, 1)
            .SetDelay(duration);
        arrowLeft.transform.DOLocalMoveX(-54, duration)
            .SetDelay(duration);
        arrowRight.transform.DOLocalMoveX(54, duration)
            .SetDelay(duration);
        // Reverse this
        arrowLeft.DOFade(0, duration)
            .SetDelay(duration);
        arrowRight.DOFade(0, duration)
            .SetDelay(duration);
    }

    public void Select()
    {
        transform.DOScale(new Vector3(1, 1, 1), appearDuration);
        glowImage.DOFade(1, appearDuration);
    }
    public void DeSelect()
    {
        transform.DOScale(new Vector3(0.77f, 0.77f, 1), appearDuration);
        glowImage.DOFade(0, appearDuration);
    }
}
