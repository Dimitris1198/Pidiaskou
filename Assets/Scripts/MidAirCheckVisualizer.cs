using UnityEngine;

public class MidAirCheckVisualizer : MonoBehaviour
{
    public float rayLength = 10f;                  // How far down to check
    private LayerMask groundLayerMask;             // Assign layers for ground objects (Trash, Bin)

    private void Start()
    {
        groundLayerMask = LayerMask.GetMask("Ground");
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayLength);
    }

    void Update()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = Vector3.down;

        // Cast ray downward
        RaycastHit hit;
        bool isGrounded = Physics.Raycast(rayOrigin, rayDirection, out hit, rayLength, groundLayerMask);

        // Draw ray for visualization
        if (isGrounded)
        {
            Debug.DrawRay(rayOrigin, rayDirection * hit.distance, Color.red);
            Debug.Log($" [GROUND CHECKER]On ground: Hit {hit.collider.gameObject.name} with tag {hit.collider.tag}");
        }
        else
        {
            Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.red);
            Debug.Log("  [GROUND CHECKER] Mid-air: No ground detected below");
        }
    }
}
