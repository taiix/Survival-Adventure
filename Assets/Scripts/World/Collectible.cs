using System;
using UnityEngine;

public sealed class Collectible : MonoBehaviour
{
    [Header("Resource")]
    [SerializeField] private string resourceName = "Wood";
    [SerializeField, Min(1)] private int amount = 1;

    [Header("Collection")]
    [SerializeField, Min(0.1f)] private float collectRadius = 1.5f;
    [SerializeField] private bool autoCollectOnTrigger = true;

    public string ResourceName => resourceName;
    public int Amount => amount;

    public event Action<Collectible> OnCollected;

    private void Awake()
    {
        if (GetComponent<Collider>() == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = collectRadius;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!autoCollectOnTrigger)
        {
            return;
        }

        if (other.GetComponent<PlayerController>() != null)
        {
            Collect();
        }
    }

    public void Collect()
    {
        OnCollected?.Invoke(this);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}
