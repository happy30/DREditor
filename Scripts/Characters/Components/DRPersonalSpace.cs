//DR Personal Space script for DRDistrust by SeleniumSoul
//To be used explicitly inconjunction with DRSpriteDepth

using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class DRPersonalSpace : MonoBehaviour
{
    public float SocialDistance = 2.5f;
    private GameObject _parent;
    private DRSpriteDepth _childshadow;
    private SphereCollider _collider;
    
    void Start()
    {
        _collider = GetComponent<SphereCollider>();
        _parent = transform.parent.gameObject;
        _childshadow = GetComponent<DRSpriteDepth>();

        _collider.isTrigger = true;
        _collider.radius = SocialDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        _parent.layer = 11;
        _childshadow.HideShadow(true);
    }

    private void OnTriggerExit(Collider other)
    {
        _parent.layer = 10;
        _childshadow.HideShadow(false);
    }
}
