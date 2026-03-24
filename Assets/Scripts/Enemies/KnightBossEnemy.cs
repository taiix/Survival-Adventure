using UnityEngine;

public class KnightBossEnemy : BaseEnemy
{
    [Header("Attacks")]
    [SerializeField] private Collider swordCollider;


    protected override void Awake()
    {
        base.Awake();
        currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
    }




}
