using System.Collections.Generic;
using UnityEngine;

public class KnightBossSpecialAttackHitbox : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField, Min(0.01f)] private float duration = 1.25f;
    [SerializeField, Min(0.01f)] private float tickRate = 0.05f;

    [Header("Path")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Hitbox Shape")]
    [SerializeField] private Vector3 halfExtents = new Vector3(0.75f, 1.0f, 0.75f);
    [SerializeField] private LayerMask playerMask = ~0;

    [Header("Damage")]
    [SerializeField] private float damage = 20f;
    [SerializeField, Min(0f)] private float stunDuration = 0.75f;

    private float elapsed;
    private float tickTimer;

    // Prevent multi-hitting the player every tick.
    private readonly HashSet<GameObject> hitPlayers = new HashSet<GameObject>();

    public void Play()
    {
        elapsed = 0f;
        tickTimer = 0f;
        hitPlayers.Clear();
        enabled = true;
    }

    private void Awake()
    {
        enabled = false;
    }

    private void Update()
    {
        if (startPoint == null || endPoint == null)
        {
            enabled = false;
            return;
        }

        elapsed += Time.deltaTime;
        tickTimer += Time.deltaTime;

        float t = Mathf.Clamp01(elapsed / duration);
        Vector3 pos = Vector3.Lerp(startPoint.position, endPoint.position, t);

        if (tickTimer >= tickRate)
        {
            tickTimer = 0f;
            DoHitCheck(pos);
        }

        if (elapsed >= duration)
        {
            enabled = false;
        }
    }

    private void DoHitCheck(Vector3 center)
    {
        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, playerMask, QueryTriggerInteraction.Collide);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider c = hits[i];
            if (c == null || !c.CompareTag("Player"))
            {
                continue;
            }

            GameObject player = c.gameObject;
            if (hitPlayers.Contains(player))
            {
                continue;
            }

            hitPlayers.Add(player);

            if (player.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damage);
            }

            //PlayerStun stun = player.GetComponent<PlayerStun>();
            //if (stun != null && stunDuration > 0f)
            //{
            //    stun.ApplyStun(stunDuration);
            //}
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (startPoint == null || endPoint == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(startPoint.position, halfExtents * 2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(endPoint.position, halfExtents * 2f);
    }
#endif
}
