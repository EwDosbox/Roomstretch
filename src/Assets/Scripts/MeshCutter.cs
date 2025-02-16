using UnityEngine;
using System.Collections.Generic;

public class MeshCutter : MonoBehaviour
{
    public GameObject arch; // Assign the arch GameObject in the Inspector

    private Mesh originalMesh;
    private MeshFilter meshFilter;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null)
        {
            Debug.LogError("No MeshFilter or Mesh found on this GameObject!");
            return;
        }

        originalMesh = meshFilter.mesh;
        CutMesh();
    }

    void CutMesh()
    {
        if (arch == null)
        {
            Debug.LogError("No arch assigned!");
            return;
        }

        Mesh newMesh = new Mesh();
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        Vector3[] vertices = originalMesh.vertices;
        int[] triangles = originalMesh.triangles;
        Bounds archBounds = arch.GetComponent<Collider>().bounds; // Arch must have a collider

        Dictionary<int, int> vertexMap = new Dictionary<int, int>(); // To map old vertex indices to new

        // Keep only vertices outside the arch
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            if (!archBounds.Contains(worldPos)) // Keep vertex if it's outside the arch
            {
                vertexMap[i] = newVertices.Count;
                newVertices.Add(vertices[i]);
            }
        }

        // Rebuild triangles with valid vertices
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (vertexMap.ContainsKey(triangles[i]) &&
                vertexMap.ContainsKey(triangles[i + 1]) &&
                vertexMap.ContainsKey(triangles[i + 2]))
            {
                newTriangles.Add(vertexMap[triangles[i]]);
                newTriangles.Add(vertexMap[triangles[i + 1]]);
                newTriangles.Add(vertexMap[triangles[i + 2]]);
            }
        }

        // Apply the modified mesh
        newMesh.vertices = newVertices.ToArray();
        newMesh.triangles = newTriangles.ToArray();
        newMesh.RecalculateNormals();

        meshFilter.mesh = newMesh;
    }
}
