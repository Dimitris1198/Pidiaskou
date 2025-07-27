using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ConeFromTip : MonoBehaviour
{
    public Transform coneTip;
    [Range(1f, 179f)]
    public float peakAngle = 45f;
    public float height = 2f;
    public int segments = 30;

    public Color coneColor = new Color(1, 0, 0, 0.5f); // Semi-transparent red

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        Mesh coneMesh = CreateConeMesh();

        meshFilter.mesh = coneMesh;
        meshCollider.sharedMesh = coneMesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        // Create and assign semi-transparent material
        Material transparentMat = CreateTransparentMaterial(coneColor);
        meshRenderer.material = transparentMat;

        UpdateConeTransform();
    }

    void Update()
    {
        // Optional: for live updates
        // UpdateConeTransform();
    }

    void UpdateConeTransform()
    {
        if (coneTip != null)
        {
            transform.position = coneTip.position;
            transform.rotation = coneTip.rotation;
        }
    }

    Mesh CreateConeMesh()
    {
        float halfAngleRad = Mathf.Deg2Rad * (peakAngle / 2f);
        float radius = height * Mathf.Tan(halfAngleRad);

        Mesh mesh = new Mesh();
        mesh.name = "Cone";

        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3 * 2];

        // Tip at origin
        vertices[0] = Vector3.zero;

        // Base vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = 2 * Mathf.PI * i / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices[i + 1] = new Vector3(x, -height, z);
        }

        // Center of base
        vertices[segments + 1] = new Vector3(0, -height, 0);

        int triIndex = 0;

        // Sides
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;

            triangles[triIndex++] = 0;
            triangles[triIndex++] = next + 1;
            triangles[triIndex++] = i + 1;
        }

        // Base
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;

            triangles[triIndex++] = segments + 1;
            triangles[triIndex++] = i + 1;
            triangles[triIndex++] = next + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    Material CreateTransparentMaterial(Color color)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        mat.color = color;
        return mat;
    }

    private void OnTriggerEnter(Collider other)
    {
       // Debug.Log("Something entered the cone trigger: " + other.name);
    }

    private void OnTriggerExit(Collider other)
    {
      //  Debug.Log("Something exited the cone trigger: " + other.name);
    }
}
