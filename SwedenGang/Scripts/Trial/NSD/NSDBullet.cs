//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Gardens
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
[RequireComponent(typeof(TMP_Text))]
public class NSDBullet : MonoBehaviour
{
    [HideInInspector]
    public TMP_Text bullet => GetComponent<TMP_Text>();
    public Ease ease;
    public float duration;
    [SerializeField] AnimationCurve curve = null;
    Camera cam => NSDManager.instance.trialCamera.GetComponentInChildren<Camera>();
    //public float zPos = 900;
    public float tiltOffset = 3;
    public float angle = 65;
    public float offset = 0.4f;
    public float cameraFarPlane = 1;
    [SerializeField] float topAngleRange = 2.2f;
    //[SerializeField] float midAngleRange = 1.5f;
    [SerializeField] float botAngleRange = 1.2f;
    [SerializeField] float midAngle = 15;
    [SerializeField] float pivotX = 1;
    [SerializeField] float positionZOffset = 0;
    Vector3 resetPoint;
    Vector3 resetScale;
    RectTransform rect => GetComponent<RectTransform>();
    [SerializeField] TMP_Text bText = null;
    [SerializeField] RectTransform brect = null;

    

    public void FireBullet(Vector3 firePosition, string bulletString)
    {
        transform.position = resetPoint;
        transform.localScale = resetScale;
        rect.pivot = new Vector2(0, rect.pivot.y);
        //125
        bulletString += " ";
        Vector2 wid = rect.sizeDelta;
        wid.x = bulletString.Length * 125;
        rect.sizeDelta = wid;
        brect.sizeDelta = wid;
        bullet.text = bulletString;
        bText.text = bulletString;
        //transform.localRotation = Quaternion.Euler(0, angle, transform.localRotation.x + tilt);
        //0.125
        
        //Debug.Log(zPlane);
        
        firePosition.z = cameraFarPlane;
        //firePosition.z = bulletString.Length * 335;
        StartCoroutine(FireBulletAnimation(firePosition));
    }
    
    IEnumerator FireBulletAnimation(Vector3 firePosition)
    {
        yield return new WaitForSeconds(Time.deltaTime);
        
        Vector2 wid = rect.sizeDelta;
        wid.x = bullet.renderedWidth;
        rect.sizeDelta = wid;
        brect.sizeDelta = wid;

        float tilt = tiltOffset;
        if (firePosition.y < botAngleRange)
            tilt = 25; // was 0
        else if (firePosition.y >= topAngleRange)
            tilt = -tilt * firePosition.y / 10;
        else
            tilt = midAngle;
        if (firePosition.y < 0.23)
            tilt = 20;
        if (firePosition.x < 0)
            tilt += firePosition.x/10;
        Debug.LogWarning("Tilt: " + tilt);
        Debug.LogWarning("Fire PostiionY: " + firePosition.y);
        float angleOffset = 2 * (firePosition.x - (-204)) + 10;
        transform.localRotation = Quaternion.Euler(0, angleOffset, transform.localRotation.x + tilt);

        transform.DOMove(firePosition, duration)
            .SetUpdate(true)
            .SetEase(curve);
        transform.DOScale(1, duration)
            .SetUpdate(true)
            .SetEase(curve);

        rect.DOPivotX(pivotX, (duration / 2) * pivotX)
            .SetUpdate(true)
            .SetDelay(duration-offset);
        transform.DOMoveZ(firePosition.z + 0.005F + positionZOffset, duration / 2)
            .SetUpdate(true)
            .SetDelay(duration-offset);

        //Debug.LogWarning("Fire Position Z: " + firePosition.z + 0.005F + positionZOffset);
        //Debug.LogWarning("PivotX: " + pivotX);
        yield return new WaitForSecondsRealtime(duration - (offset/2));
        // Past here should be the moment the first letter hits the text
        
        RaycastHit hit;
        if (Physics.Raycast(cam.ScreenPointToRay(NSDManager.instance.mousePos), out hit))
        {
            if (!hit.collider.isTrigger && hit.transform.gameObject.GetComponent<PhraseText>() != null)
            {
                hit.transform.gameObject.GetComponent<PhraseText>().Check(hit.point);
                NSDManager.instance.RemoveSlow();
                //Debug.Log("Firing");
            }
            else if (hit.transform.gameObject.GetComponent<WhiteNoiseText>() != null)
            {
                Debug.Log("Time should be removed");
                NSDManager.instance.timer.RemoveTime(15);
                yield return new WaitForSecondsRealtime(offset * 2);
                NSDManager.instance.CanFire();
            }
            else
            {
                yield return new WaitForSecondsRealtime(offset*2);
                NSDManager.instance.CanFire();
                //Debug.Log("Reloading");
            }
        }
        else
        {
            yield return new WaitForSecondsRealtime(offset*2);
            NSDManager.instance.CanFire();
            //Debug.Log("Reloading");
        }
        yield return null;
    }
    IEnumerator BulletShatter(Vector3 firePosition)
    {
        
        yield return null;
    }
    private void Awake()
    {
        resetPoint = transform.position;
        resetScale = transform.localScale;
    }
}
