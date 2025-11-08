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

        // 1. 🟢 ถ้าผู้เล่นอยู่ไกล -> ไล่ (Chase)
        // ✅ แก้ไข: เพิ่ม BossManager. ข้างหน้า BossState
        if (distance > manager.baitingDistance)
        {
            if (manager.currentState != BossManager.BossState.Chase) 
            {
                manager.currentState = BossManager.BossState.Chase;
                Debug.Log("BossAI: Player อยู่ไกล -> CHASE");
            }
        }
        // 2. 🟡 ถ้าผู้เล่นอยู่ระยะกลาง -> คุมเชิง (Bait)
        // ✅ แก้ไข: เพิ่ม BossManager. ข้างหน้า BossState
        else if (distance > manager.stoppingDistance && distance <= manager.baitingDistance)
        {
            if (manager.currentState != BossManager.BossState.Bait && manager.currentState != BossManager.BossState.Attack)
            {
                manager.currentState = BossManager.BossState.Bait;
                Debug.Log("BossAI: Player อยู่ระยะกลาง -> BAIT");
            }
        }
        // 3. 🔴 ถ้าผู้เล่นอยู่ใกล้ -> ตี (Attack)
        // ✅ แก้ไข: เพิ่ม BossManager. ข้างหน้า BossState
        else if (distance <= manager.stoppingDistance)
        {
            if (manager.currentState != BossManager.BossState.Attack)
            {
                manager.RequestAttack();
                Debug.Log("BossAI: Player อยู่ใกล้ -> ATTACK");
            }
        }
    }
}
