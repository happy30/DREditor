//DR Sprite Depth by SeleniumSoul for DR:Distrust
//Note: This SHOULD be a shader script. Until I figure out how to use shaders, this will be stuck as just a monoscript.

using UnityEngine;

public class DRSpriteDepth : MonoBehaviour
{
    public GameObject Shadow;
    public GameObject[] Shadows;
    public int Depth;

    public GameObject Parent;
    public Material CharShadow;

    void Start()
    {
        if (!Shadow)
        {
            Shadow = transform.GetChild(0).gameObject;
        }

        Parent = transform.parent.gameObject;
        CharShadow = Shadow.GetComponent<Renderer>().material;
        Shadows = new GameObject[Depth];
        Shadows[0] = Shadow;

        for (int x = 1; x < Depth; x++)
        {
            GameObject _duplicatesprite = Instantiate(Shadows[0]);
            _duplicatesprite.transform.SetParent(Shadows[0].transform.parent);
            _duplicatesprite.transform.localScale = Shadows[0].transform.localScale;
            _duplicatesprite.transform.localPosition = Shadows[x-1].transform.localPosition + new Vector3(0f, 0f, -0.001f);
            _duplicatesprite.transform.localRotation = Shadows[0].transform.localRotation;
            _duplicatesprite.name = "Shadow" + x;

            Shadows[x] = _duplicatesprite;
        }
    }

    void Update()
    {
        //if (CharShadow.mainTexture != ParentTex.mainTexture)
            //CharShadow.mainTexture = ParentTex.mainTexture;
    }

    public void UpdateMat(Material face)
    {
        CharShadow.mainTexture = face.mainTexture;
    }

    public void HideShadow(bool toggle)
    {
        if (toggle)
        {
            foreach (GameObject go in Shadows)
            {
                go.gameObject.layer = 11;
            }
        }
        else
        {
            foreach (GameObject go in Shadows)
            {
                go.gameObject.layer = 15;
            }
        }
    }
}
