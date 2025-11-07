using UnityEngine;

public class BossAI : MonoBehaviour
{
    private BossManager manager;
    private float thinkTimer;

    [Header("AI Think Time")]
    [SerializeField] private float thinkIntervalMin = 0.6f;
    [SerializeField] private float thinkIntervalMax = 1.2f;

    private void Awake()
    {
        manager = GetComponent<BossManager>();
    }

    private void Update()
    {
        // ❌ ถ้าตายหรือกำลังตี ไม่ต้องคิดอะไร
        if (manager.currentState == BossManager.BossState.Dead ||
            manager.currentState == BossManager.BossState.Attack ||
            manager.currentState == BossManager.BossState.Stunned)
            return;

        thinkTimer -= Time.deltaTime;

        if (thinkTimer <= 0f)
        {
            ThinkBehavior();
            thinkTimer = Random.Range(thinkIntervalMin, thinkIntervalMax);
        }
    }

    private void ThinkBehavior()
    {
        if (manager.playerTarget == null) return;

        float distance = Vector3.Distance(manager.transform.position, manager.playerTarget.position);

        // ถ้าผู้เล่นอยู่นอกระยะ bait → เดินเข้าหา
        if (distance > manager.baitingDistance)
        {
            if (manager.currentState != BossManager.BossState.Chase)
            {
                manager.currentState = BossManager.BossState.Chase;
                Debug.Log("BossAI: switching to Chase (player too far)");
            }
        }
        // ถ้าอยู่ในระยะ bait แต่ยังไม่ถึงระยะตี → ให้เดินวน (BossMovement จะจัดการ)
        else if (distance > manager.stoppingDistance && distance <= manager.baitingDistance)
        {
            if (manager.currentState != BossManager.BossState.Chase)
            {
                manager.currentState = BossManager.BossState.Chase;
                Debug.Log("BossAI: staying in Chase for bait/strafe behavior");
            }
        }
        // ถ้าอยู่ในระยะตี → ให้บอสตีทันที
        else if (distance <= manager.stoppingDistance)
        {
            if (manager.currentState != BossManager.BossState.Attack)
            {
                manager.RequestAttack();
                Debug.Log("BossAI: Requesting attack (in range)");
            }
        }
    }
}
