using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpot : MonoBehaviour
{
    [SerializeField] private int SpotID;
    private AssistantDirector AD;
    private Animation Anim;

    [SerializeField] private GameObject Character;

    private void Awake()
    {
        if (AD == null) AD = GameObject.FindGameObjectWithTag("Director").GetComponent<AssistantDirector>();
        if (Anim == null) Anim = GetComponent<Animation>();
    }

    private void OnEnable()
    {
        switch (AD.SceneType)
        {
            case SceneType.PointAndClick:
                Debug.Log("Point and click");
                Anim.Play("Char_Enable");
                break;
            default:
                break;
        }
    }

    public void ChangeCharacter(GameObject chara)
    {
        Character = chara;
        Anim.Play("Char_Enable");
    }

    public void Disable()
    {
        StartCoroutine(DisableAnim());
    }

    private IEnumerator DisableAnim()
    {
        if (Character == null) Character = gameObject.transform.GetChild(0).GetChild(0).gameObject;

        Anim.Play("Char_Leave");

        yield return new WaitForSeconds(Anim.GetClip("Char_Leave").length);

        Destroy(Character);
        yield return null;
    }
}
