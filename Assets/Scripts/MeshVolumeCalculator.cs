using UnityEngine;

public static class MeshVolumeCalculator
{
    public static float CalculateMeshVolume(Mesh mesh, Vector3 scale)
    {
        float volume = 0f;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = Vector3.Scale(vertices[triangles[i]], scale);
            Vector3 p2 = Vector3.Scale(vertices[triangles[i + 1]], scale);
            Vector3 p3 = Vector3.Scale(vertices[triangles[i + 2]], scale);

            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }

        return Mathf.Abs(volume);
    }

    private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return Vector3.Dot(p1, Vector3.Cross(p2, p3)) / 6.0f;
    }
}
