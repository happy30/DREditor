//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
// Leothedev also helped out a lot
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class UIBreak : Graphic
{
    MeshFilter filter => GetComponent<MeshFilter>();
    ShatterScreen shatter => GetComponent<ShatterScreen>();
    public Texture mainTex;
    public RawImage blackBreakImage;
    public RawImage breakImage;
    public static bool finished = false;
    public override Texture mainTexture { get { return mainTex; } }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        //shatter.GenerateMesh(9, 16);
        //UIVertex vertex = UIVertex.simpleVert;

        /*foreach(Vector3 vert in filter.sharedMesh.vertices)
        {
            vertex.position = vert;
            vertex.uv0 = new Vector2(vert.x / rectTransform.sizeDelta.x + 0.5f, vert.y / rectTransform.sizeDelta.y + 0.5f);
            vh.AddVert(vertex);
        }*/
        foreach (UIVertex u in shatter.uIVertices)
        {
            vh.AddVert(u);
        }
        
        for (int i = 0; (i + 6) <= shatter.uIVertices.Count; i += 6)
        {
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i + 3, i + 4, i + 5);
        }
    }

    public float explosionRadius;
    public float forceMultiplier;
    public float explosionTime;
    public void ScreenShatter() => StartCoroutine(Shatter());
    IEnumerator Shatter()
    {
        //shatter.GenerateMesh(9, 16);
        List<UIVertex> v = shatter.uIVertices;
        Vector3[] triangleVelocity = new Vector3[v.Count / 3];
        //Vector3[] rotationVelocity = new Vector3[v.Count / 3];
        for (int i = 0; i < v.Count/3; i++)
        {
            Vector3 a = v[i * 3].position;
            Vector3 b = v[i * 3 + 1].position;
            Vector3 c = v[i * 3 + 2].position;
            Vector3 center = (a + b + c) / 3;
            Vector3 direction = center.normalized;
            float t = center.magnitude / explosionRadius;
            t = 1 - t;
            triangleVelocity[i] = direction * t * forceMultiplier;
        }
        //shatter.uIVertices = v;
        float time = 0;
        while(time <= explosionTime)
        {
            time += Time.deltaTime;
            for (int i = 0; i < v.Count / 3; i++)
            {
                Vector3 pos = v[i * 3].position;
                pos += triangleVelocity[i] * Time.deltaTime;
                UIVertex u = v[i * 3];
                u.position = pos;
                v[i * 3] = u;

                pos = v[i * 3 + 1].position;
                pos += triangleVelocity[i] * Time.deltaTime;
                u = v[i * 3 + 1];
                u.position = pos;
                v[i * 3 + 1] = u;

                pos = v[i * 3 + 2].position;
                pos += triangleVelocity[i] * Time.deltaTime;
                u = v[i * 3 + 2];
                u.position = pos;
                v[i * 3 + 2] = u;

                triangleVelocity[i] -= triangleVelocity[i] * Time.deltaTime * 1/explosionTime;
                //Debug.Log(v[i * 3].position);
            }
            shatter.uIVertices = v;
            yield return null;
            SetVerticesDirty();
        }
        yield break;
    }
    Camera cam => GameObject.Find("DRCameraAnchor").GetComponentInChildren<Camera>();
    public Camera shatterCamera;
    public void ShatterTheScreen() => StartCoroutine(SetCam());
    IEnumerator SetCam()
    {
        breakImage.transform.DOScale(1, 0);
        breakImage.DOFade(1, 0);
        blackBreakImage.DOFade(1, 0);
        shatterCamera.transform.position = cam.transform.position;
        shatterCamera.targetTexture = (RenderTexture)mainTex;
        shatterCamera.enabled = true;
        yield return new WaitForSeconds(Time.deltaTime);
        shatterCamera.targetTexture = null;
        shatter.GenerateMesh(9, 16);
        SetVerticesDirty();
        shatterCamera.enabled = false;
        ScreenShatter();
        blackBreakImage.enabled = true;
        breakImage.enabled = true;
        
        breakImage.transform.DOScale(2, 1)
            .SetDelay(1)
            .SetEase(Ease.InFlash);
        breakImage.DOFade(0, 1)
            .SetDelay(1.5f)
            .SetEase(Ease.InFlash);
        yield return new WaitForSeconds(2);
        
        blackBreakImage.DOFade(0, 1)
            .SetDelay(3);
        yield return new WaitForSeconds(2);
        breakImage.enabled = false;
        shatter.uIVertices.Clear();

        yield return new WaitForSeconds(2);
        finished = true;
        yield break;
    }
}
