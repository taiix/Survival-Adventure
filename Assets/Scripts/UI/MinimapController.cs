using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Minimalistic minimap using a secondary camera rendered to a RenderTexture.
/// Attach to a UI RawImage. Set the minimap camera's Culling Mask and Target Texture accordingly.
/// Addresses issue #47 (mini-map, optional).
/// </summary>
public class MinimapController : MonoBehaviour
{
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RawImage minimapDisplay;
    [SerializeField] private RenderTexture minimapTexture;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float cameraHeight = 30f;
    [SerializeField] private float zoomSize = 20f;

    private void Start()
    {
        if (minimapCamera == null) return;

        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = zoomSize;

        if (minimapTexture != null)
            minimapCamera.targetTexture = minimapTexture;

        if (minimapDisplay != null && minimapTexture != null)
            minimapDisplay.texture = minimapTexture;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (minimapCamera == null || playerTransform == null) return;

        Vector3 target = playerTransform.position;
        minimapCamera.transform.position = new Vector3(target.x, target.y + cameraHeight, target.z);
    }
}
