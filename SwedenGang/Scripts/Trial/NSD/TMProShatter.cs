using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEditor;

// OG Author LeoTheDev : Tidied up by Sweden. 
[RequireComponent(typeof(TMP_Text))]
public class TMProShatter : MonoBehaviour
{
    public TMP_Text Container => GetComponent<TMP_Text>();
    Triangle[] Triangles;
    Vector3[] newVertices;
    int[] newTriangles;
    Vector2[][] newUVs;
    Color[] vertexColors;
    public float MaxAlpha = 1, MinAlpha = 0;
    Vector3[] triangleVelocity;
    Vector3[] rotationVelocity;
    [Range(0, 1)]
    public float AirResistince = 0.8f;
    public float HitForce = .025f;
    public float MaxHitArea = 3f;
    public float AngularVelocityMultiplier = 100;
    [SerializeField] bool fades = false;
    [SerializeField] bool freezeXYRot = false;
    [SerializeField] bool freezeZPos = false;

    public Vector3 ShatterPoint;
    [HideInInspector]
    public bool ready;

    Mesh resetMesh;
    public void SetMesh()
    {
        resetMesh = new Mesh();
        resetMesh.vertices = Container.mesh.vertices;
        resetMesh.triangles = Container.mesh.triangles;
        resetMesh.uv = Container.mesh.uv;
        resetMesh.normals = Container.mesh.normals;
        resetMesh.colors = Container.mesh.colors;
        resetMesh.tangents = Container.mesh.tangents;
    }
    public void ResetMesh()
    {
        if(resetMesh == null)
        {
            return;
        }
        ready = false;

        //Debug.Log(resetMesh.vertices.Length);
        //Debug.Log(Container.mesh.vertices.Length);
        Container.mesh.triangles = resetMesh.triangles;
        Container.mesh.vertices = resetMesh.vertices;
        
        Container.mesh.uv = resetMesh.uv;
        Container.mesh.normals = resetMesh.normals;
        Container.mesh.colors = resetMesh.colors;
        Container.mesh.tangents = resetMesh.tangents;
        //Debug.Log("test");
        Container.UpdateGeometry(Container.mesh, 0);
    }
    [SerializeField] float multiplier = 2f;
    public void Update()
    {
        if (!ready)
            return;
        for (int i = 0; i < Triangles.Length; i++)
        {
            if (fades)
            {
                for (int x = 0; x < 3; x++)
                {
                    vertexColors[Triangles[i].triangle[x]].a = Mathf.Lerp(MinAlpha, MaxAlpha, rotationVelocity[i].magnitude / multiplier);
                }
            }
            Triangles[i].Rotate(rotationVelocity[i], newVertices);
            Triangles[i].Move(triangleVelocity[i], newVertices, freezeZPos);
            rotationVelocity[i] *= (1 - (AirResistince * Time.deltaTime));
            triangleVelocity[i] *= (1 - (AirResistince * Time.deltaTime));
        }
        Container.mesh.vertices = newVertices;
        Container.mesh.colors = vertexColors;
        Container.UpdateGeometry(Container.mesh, 0);
    }

    public void Shatter(Vector3 HitPoint) // Call Function
    {
        Triangulate(Container.mesh);
        for (int i = 0; i < Triangles.Length; i++)
        {
            Vector3 direction = (Triangles[i].Center - HitPoint).normalized;
            float dist = Vector3.Distance(Triangles[i].Center, HitPoint);
            float force = Mathf.Lerp(HitForce, 0, dist / MaxHitArea);
            triangleVelocity[i] = direction * force;
            if(freezeXYRot)
                rotationVelocity[i] = new Vector3(0, 0, Random.Range(0, 1f)) * force * AngularVelocityMultiplier;
            else
                rotationVelocity[i] = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0, 1f)) * force * AngularVelocityMultiplier;

        }
        ready = true;
    }

    public void Triangulate(Mesh mesh)
    {
        if (ready)
            return;
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        //Force mesh update to get valid data
        Container.ForceMeshUpdate();
        int triCount = mesh.triangles.Length / 3 - 2;
        int triPairs = triCount / 2;
        int newVertexCount = triCount * 3;

        //InitializeArrays
        vertexColors = new Color[newVertexCount];
        triangleVelocity = new Vector3[triCount];
        rotationVelocity = new Vector3[triCount];
        Triangles = new Triangle[triCount];
        newVertices = new Vector3[newVertexCount];
        newTriangles = new int[newVertexCount];
        newUVs = new Vector2[2][];
        newUVs[0] = new Vector2[newVertexCount];
        newUVs[1] = new Vector2[newVertexCount];

        //UV stuff ------------------------- (not important for now)
        int originalVertIdx = 0;
        int vertIdx = 0;
        int triIdx = 0;
        //Loop through every triangle pair and generate 2 new triangles to the buffer
        for (int i = 0; i < triPairs; i++)
        {
            //Add triangles to the buffer
            Triangles[triIdx] = CreateTri(mesh, originalVertIdx, vertIdx, true);
            Triangles[triIdx + 1] = CreateTri(mesh, originalVertIdx, vertIdx, false);

            //Update idx
            originalVertIdx += 4;
            vertIdx += 6;
            triIdx += 2;
        }
        watch.Stop();

        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
        mesh.uv = newUVs[0];
        mesh.uv2 = newUVs[1];
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        ready = true;
        //Debug.Log("Finsihed triangulating");
    }

    Triangle CreateTri(Mesh mesh, int originalVertIdx, int vertIdx, bool isLeft)
    {
        int pos1, pos2, pos3, offset1, offset2;

        if (isLeft)
        {
            pos1 = 0;
            pos2 = 1;
            pos3 = 2;
            offset1 = 1;
            offset2 = 2;
        }
        else
        {
            pos1 = 3;
            pos2 = 4;
            pos3 = 5;
            offset1 = 2;
            offset2 = 3;
        }
        Triangle T = new Triangle();
        //Add necessary verticies
        newVertices[vertIdx + pos1] = mesh.vertices[originalVertIdx]; //[Object Space]
        newVertices[vertIdx + pos2] = mesh.vertices[originalVertIdx + offset1]; //[Object Space]
        newVertices[vertIdx + pos3] = mesh.vertices[originalVertIdx + offset2]; //[Object Space]

        //Set apropriate color
        vertexColors[vertIdx + pos1] = mesh.colors[originalVertIdx];
        vertexColors[vertIdx + pos2] = mesh.colors[originalVertIdx + offset1];
        vertexColors[vertIdx + pos3] = mesh.colors[originalVertIdx + offset2];

        //Add necessary UVs
        //UV
        newUVs[0][vertIdx + pos1] = mesh.uv[originalVertIdx];
        newUVs[0][vertIdx + pos2] = mesh.uv[originalVertIdx + offset1];
        newUVs[0][vertIdx + pos3] = mesh.uv[originalVertIdx + offset2];
        //UV2
        newUVs[1][vertIdx + pos1] = mesh.uv2[originalVertIdx];
        newUVs[1][vertIdx + pos2] = mesh.uv2[originalVertIdx + offset1];
        newUVs[1][vertIdx + pos3] = mesh.uv2[originalVertIdx + offset2];


        //Add necessary idxes
        newTriangles[vertIdx + pos1] = vertIdx + pos1;
        newTriangles[vertIdx + pos2] = vertIdx + pos2;
        newTriangles[vertIdx + pos3] = vertIdx + pos3;

        //Calculate triangle center (will be used in the future to rotate the triangle around it's center) [Object Space]
        T.Center = (newVertices[vertIdx + pos1] + newVertices[vertIdx + pos2] + newVertices[vertIdx + pos3]) / 3;

        //Triangle order (clockwise)
        T.triangle[0] = vertIdx + pos1;
        T.triangle[1] = vertIdx + pos2;
        T.triangle[2] = vertIdx + pos3;

        return T;
    }
}

[System.Serializable]
public class Triangle
{
    public int[] triangle = new int[3];
    public Vector3 Center;

    public void Move(Vector3 Offset, Vector3[] verts, bool frozenZ)
    {
        Center += Offset;
        for (int i = 0; i < 3; i++)
        {
            if (frozenZ)
            {
                float z = verts[triangle[i]].z;
                verts[triangle[i]] += Offset;
                verts[triangle[i]].z = z;
            }
            else
                verts[triangle[i]] += Offset;
        }
    }
    public void Rotate(Vector3 offset, Vector3[] verts)
    {
        for (int i = 0; i < 3; i++)
        {
            verts[triangle[i]] = RotateVertex(verts[triangle[i]], offset);
        }
    }
    Vector3 RotateVertex(Vector3 original, Vector3 rotationOffset)
    {
        Vector3 dir = original - Center;
        dir = Quaternion.Euler(rotationOffset) * dir;
        original = dir + Center;
        return original;
    }
}
