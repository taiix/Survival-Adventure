using UnityEngine;

public class PlayerWaterDetection : MonoBehaviour
{
    [SerializeField] private float sphereDistance = 1.5f;
    [SerializeField] private float sphereYOffset = 0f;
    [SerializeField] private float sphereRadius = 0.5f;
    [SerializeField] private float raycastDistance = 3f;
    [SerializeField] private Color gizmoColor = Color.cyan;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        Vector3 spherePosition = transform.position + transform.forward * sphereDistance + Vector3.up * sphereYOffset;
        Gizmos.DrawWireSphere(spherePosition, sphereRadius);
        Gizmos.DrawLine(spherePosition, spherePosition + Vector3.down * raycastDistance);
    }

    public bool IsDetectingWater()
    {
        Vector3 spherePosition = transform.position + transform.forward * sphereDistance + Vector3.up * sphereYOffset;

        if (!Physics.Raycast(spherePosition, Vector3.down, out RaycastHit firstHit, raycastDistance))
        {
            return false;
        }

        if (!firstHit.collider.CompareTag("Water"))
        {
            return false;
        }

        return true;
    }
}
