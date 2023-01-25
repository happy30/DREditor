//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
// LeotheDev also helped out a lot
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShatterScreen : MonoBehaviour
{
    public MeshFilter targetMeshFilter;
    public float verticleSize = Screen.height, horizontalSize = Screen.width;
    public List<UIVertex> uIVertices = new List<UIVertex>();
    public void GenerateMesh(int vResolution, int hResolution)
    {
        // Calculate quad size based on resolution
        float w = horizontalSize / hResolution;
        float h = verticleSize / vResolution;

        // Create a mesh
        Mesh mesh = new Mesh();

        // Compute Vertices
        List<Vector3> vertices = new List<Vector3>();
        for(int x = 0; x < hResolution; x++)
        {
            for(int y = 0; y < vResolution; y++)
            {
                Vector3 offset = new Vector3(w * x, h * y);
                Vector3[] v = GenerateQuad(w, h, offset - new Vector3(horizontalSize / 2, verticleSize / 2, 0)
                    + new Vector3(w / 2, h / 2, 0));
                vertices.AddRange(v);
            }
        }
        

        float asize = w + h / 2;
        RandomizeMesh(hResolution, vResolution, asize, vertices);
        mesh.vertices = vertices.ToArray();

        List<Vector2> uVs = new List<Vector2>();
        for(int i = 0; i<vertices.Count; i++)
        {
            Vector3 a = vertices[i];
            Vector2 b = GetUV(vertices[i]);
            UIVertex uiv = UIVertex.simpleVert;
            uiv.position = a;
            uiv.uv0 = b;
            uIVertices.Add(uiv);
            uVs.Add(b);
        }

        // Set UVs
        mesh.uv = uVs.ToArray();

        // Set Triangles
        mesh.triangles = Enumerable.Range(0, vResolution * hResolution * 6).ToArray();

        mesh.RecalculateBounds();
        targetMeshFilter.mesh = mesh;
        
    }

    Vector3[] GenerateQuad(float w, float h, Vector3 offset)
    {
        Vector3[] verts = new Vector3[6];
        float w2 = w / 2;
        float h2 = h / 2;
        // Triangle 1
        verts[0] = new Vector3(-w2, -h2) + offset;
        verts[1] = new Vector3(-w2, h2) + offset;
        verts[2] = new Vector3(w2, h2) + offset;

        // Triangle 2
        verts[3] = new Vector3(-w2, -h2) + offset;
        verts[4] = new Vector3(w2, h2) + offset;
        verts[5] = new Vector3(w2, -h2) + offset;

        return verts;
    }

    public float maxOffset;
    public void RandomizeMesh(int hRes, int vRes, float averageSize, List<Vector3> vertices)
    {
        int vertIdx = 2;
        int validConnections = (hRes - 1) * (vRes - 1);
        int a = vRes - 2;
        float localMaxOffset = maxOffset * averageSize;
        for(int i = 0; i < validConnections; i++)
        {
            // The vertices that we are suppose to move at each intersection
            int[] idxes = new int[6];

            idxes[0] = vertIdx;
            idxes[1] = vertIdx + 2;
            idxes[2] = vertIdx + 9;
            int idxOffset = vRes * 6;
            idxes[3] = vertIdx + idxOffset - 1;
            idxes[4] = vertIdx + idxOffset + 4;
            idxes[5] = vertIdx + idxOffset + 7;
            Vector3 point = (Vector3)Random.insideUnitCircle * localMaxOffset;
            for(int x = 0; x < 6; x++)
            {
                int idx = idxes[x];
                vertices[idx] += point;
                vertices[idx] = GetNewPosition(vertices[idx]);
            }
            if (i == a)
            {
                a += vRes - 1;
                vertIdx += 12;
                continue;
            }
            vertIdx += 6;
        }
    }

    public float explosionRadius, centerBreakFactor;
    Vector3 GetNewPosition(Vector3 position)
    {
        float distToCenter = Vector3.Distance(Vector3.zero, position);
        float multiplier = distToCenter / (explosionRadius / 2);
        return position * multiplier * centerBreakFactor;
    }

    public Vector2 GetUV(Vector2 pos)
    {
        // Ignore the Z component
        Vector2 a = (Vector2)pos + new Vector2(horizontalSize / 2, verticleSize / 2);
        return a / new Vector2(horizontalSize, verticleSize);
    }

    
}
