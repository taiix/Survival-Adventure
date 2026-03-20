using UnityEngine;

public sealed class PlayerWaterDetection
{
    private readonly float sphereDistance;
    private readonly float sphereYOffset;
    private readonly float sphereRadius;
    private readonly float raycastDistance;

    public PlayerWaterDetection(float sphereDistance, float sphereYOffset, float sphereRadius, float raycastDistance)
    {
        this.sphereDistance = sphereDistance;
        this.sphereYOffset = sphereYOffset;
        this.sphereRadius = sphereRadius;
        this.raycastDistance = raycastDistance;
    }

    public bool IsDetectingWater(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            return false;
        }

        Vector3 spherePosition =
            playerTransform.position +
            playerTransform.forward * sphereDistance +
            Vector3.up * sphereYOffset;

        if (!Physics.Raycast(spherePosition, Vector3.down, out RaycastHit firstHit, raycastDistance))
        {
            return false;
        }

        return firstHit.collider.CompareTag("Water");
    }
}
