using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class FlattenToTerrain : MonoBehaviour
{
    public Terrain terrain;
    public MeshFilter meshFilter;

    void Start()
    {
        if (terrain == null || meshFilter == null)
        {
            Debug.LogError("Terrain or MeshFilter not assigned!");
            return;
        }

        MatchMesh();
    }

    void MatchMesh()
    {
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        Dictionary<int, float> indexHeight = new Dictionary<int, float>();
        float maxY = 0;

        for (int i = 0; i < vertices.Length; i++) {
            indexHeight[i] = vertices[i].y;
            maxY = Mathf.Max(maxY, vertices[i].y);
        }
        for (int i = 0; i < vertices.Length; i++) {
            Vector3 worldPosition = transform.TransformPoint(vertices[i]);
            
            worldPosition.y = terrain.SampleHeight(worldPosition);
            vertices[i].y = transform.InverseTransformPoint(worldPosition).y + .2f + indexHeight[i] - maxY;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}