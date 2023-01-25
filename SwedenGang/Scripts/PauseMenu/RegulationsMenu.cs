using DREditor.TrialEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static UnityEngine.InputSystem.InputAction;

public class RegulationsMenu : MonoBehaviour { // Code by Willy Bee

    [SerializeField] List<Regulation> regulations = null;
    [SerializeField] int regIndex;
    [SerializeField] Image leftArrow = null;
    [SerializeField] Image rightArrow = null;
    [SerializeField] Image ruleImg = null;
    [SerializeField] Image ruleDesc = null;
    [SerializeField] AudioClip NavSFX = null;
    DRControls _controls;

    private void Awake() => _controls = new DRControls();
    private void OnEnable() => _controls.Enable();
    private void OnDisable() => _controls.Disable();
    
    public void ActivateControls() => _controls.UI.Navigate.performed += changeRegulation;
    public void DeactivateControls() => _controls.UI.Navigate.performed -= changeRegulation;

    private void Start() {
        regulations = MasterDatabase.GetRegulations();
        resetIndex();
    }

    public void resetIndex() {
        regIndex = 0;
        setArrows();
        ruleImg.sprite = regulations[0].regulationImg;
        ruleDesc.sprite = regulations[0].regulationDesc;
    }

    private void changeRegulation(CallbackContext context) {
        Vector2 nav = _controls.UI.Navigate.ReadValue<Vector2>();
        
        if (nav.x > 0.5f && regIndex < regulations.Count - 1) {
            StartCoroutine(changeRegSequence(1));
        } else if (nav.x < -0.5f && regIndex > 0) {
            StartCoroutine(changeRegSequence(-1));
        }
    }

    public IEnumerator changeRegSequence(int direction) {
        _controls.Disable();
        
        yield return new WaitForEndOfFrame();

        ruleImg.transform.DOLocalMoveX(direction * -50f, 0.2f).SetEase(Ease.OutQuart).SetUpdate(true);
        ruleImg.DOFade(0f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);
        ruleDesc.DOFade(0f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);

        regIndex += direction;
        setArrows();
        SoundManager.instance.PlaySFX(NavSFX);

        yield return new WaitForSecondsRealtime(0.2f);

        ruleImg.sprite = regulations[regIndex].regulationImg;
        ruleDesc.sprite = regulations[regIndex].regulationDesc;
        ruleImg.transform.DOLocalMoveX(direction * 50f, 0f).SetUpdate(true);

        yield return new WaitForSecondsRealtime(0.1f);

        ruleImg.transform.DOLocalMoveX(0f, 0.2f).SetEase(Ease.OutQuart).SetUpdate(true);
        ruleImg.DOFade(1f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);
        ruleDesc.DOFade(1f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);

        _controls.UI.Navigate.performed += changeRegulation;

        _controls.Enable();
    }

    public void setArrows() {
        if (regulations.Count < 2) { 
            leftArrow.DOFade(0.3f, 0.3f).SetUpdate(true);
            rightArrow.DOFade(0.3f, 0.3f).SetUpdate(true);
        } else if (regIndex == 0) {
            leftArrow.DOFade(0.3f, 0.3f).SetUpdate(true);
            rightArrow.DOFade(1f, 0.3f).SetUpdate(true);
        } else if (regIndex == regulations.Count - 1) {
            leftArrow.DOFade(1f, 0.3f).SetUpdate(true);
            rightArrow.DOFade(0.3f, 0.3f).SetUpdate(true);
        } else {
            leftArrow.DOFade(1f, 0.3f).SetUpdate(true);
            rightArrow.DOFade(1f, 0.3f).SetUpdate(true);
        }
    }
}