using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BossManager))]
public class BossAI : MonoBehaviour
{
    private BossManager boss;
    private Transform player;
    private float thinkTimer;
    private bool isStrafing = false; // 🚩 ใช้ flag นี้เพื่อป้องกันการรัน DecideNextAction ขณะ Strafe

    [Header("AI Settings")]
    public float thinkIntervalMin = 0.4f;
    public float thinkIntervalMax = 0.8f;
    public float detectRange = 10f;
    public float attackRange = 2.5f;

    [Header("Behavior Probability")]
    [Range(0, 1)] public float singleAttackChance = 0.5f;
    [Range(0, 1)] public float comboAttackChance = 0.3f;
    [Range(0, 1)] public float strafeChance = 0.2f;

    [Header("Movement")]
    public float strafeDuration = 1.5f;

    private void Start()
    {
        boss = GetComponent<BossManager>();
        player = boss.playerTarget;
        thinkTimer = Random.Range(thinkIntervalMin, thinkIntervalMax);
    }

    private void Update()
    {
        if (player == null || boss.currentState == BossManager.BossState.Dead)
            return;

        // 🚫 ถ้ากำลังโจมตี, ถูก Stun, หรือกำลัง Strafe ไม่ต้องคิด
        if (boss.currentState == BossManager.BossState.Attack || 
            boss.currentState == BossManager.BossState.Stunned || 
            isStrafing) // ❗ ใช้ isStrafing ควบคู่ไปกับ BossState.Idle
            return;

        thinkTimer -= Time.deltaTime;

        if (thinkTimer <= 0)
        {
            DecideNextAction();
            thinkTimer = Random.Range(thinkIntervalMin, thinkIntervalMax);
        }
        
        // ❌ ลบ FacePlayer() ออก เพราะ BossMovement.cs จัดการแล้ว (ใน HandleRotation)
    }

    private void DecideNextAction()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // 1. ผู้เล่นอยู่ไกลเกิน (Out of Range)
        if (distance > detectRange)
        {
            boss.currentState = BossManager.BossState.Idle; // หยุดการไล่
            return;
        }
        
        // 2. อยู่ในระยะโจมตี (Attack Range)
        if (distance <= attackRange)
        {
            float roll = Random.value;
            if (roll < singleAttackChance)
            {
                StartCoroutine(PrepareAttack(1));
                Debug.Log("🤖 AI: Single Attack (Close)");
            }
            else if (roll < singleAttackChance + comboAttackChance)
            {
                // ตรวจสอบเฟส (Phase) เพื่อความซับซ้อนในการคอมโบที่อาจจะมากกว่า 3
                int maxCombo = boss.currentPhase == BossManager.BossPhase.Phase1 ? 3 : 5;
                StartCoroutine(PrepareAttack(maxCombo));
                Debug.Log($"🤖 AI: Combo Attack (Close, Max {maxCombo} Hits)");
            }
            else
            {
                StartCoroutine(StrafeAroundPlayer());
                Debug.Log("🤖 AI: Strafe (Close)");
            }
            return;
        }
        
        // 3. อยู่ในระยะกลาง (Mid Range: ไล่หรือ Strafe)
        if (distance > attackRange && distance < detectRange)
        {
            // ระยะกลาง: มีโอกาสหยุดเดินเพื่อ Strafe (เพิ่มความหลากหลาย)
            if (Random.value < strafeChance && boss.currentState == BossManager.BossState.Chase) 
            {
                StartCoroutine(StrafeAroundPlayer());
                Debug.Log("🤖 AI: Strafe (Mid range)");
                return;
            }
            
            // ถ้าไม่ Strafe ให้ไล่ตามปกติ
            boss.currentState = BossManager.BossState.Chase;
            return;
        }

        // 4. Fallback: ไล่
        boss.currentState = BossManager.BossState.Chase;
    }

    private IEnumerator PrepareAttack(int combo)
    {
        // ❗ ตั้งเป็น Idle เพื่อหยุดการเคลื่อนที่ก่อนโจมตี
        boss.currentState = BossManager.BossState.Idle;
        
        // หน่วงเวลาเล็กน้อยเพื่อให้ AI ดูเหมือนกำลัง 'คิด' หรือ 'เตรียมตัว'
        yield return new WaitForSeconds(Random.Range(0.2f, 0.6f));

        boss.maxComboCount = combo;
        boss.RequestAttack(); // ➡️ สั่งให้ BossManager เริ่มต้นการโจมตี
    }

    private IEnumerator StrafeAroundPlayer()
    {
        isStrafing = true;
        // ❗ ตั้งเป็น Idle เพื่อหยุด BossMovement.HandleTacticalChase
        boss.currentState = BossManager.BossState.Idle; 

        float timer = 0f;
        // สุ่มทิศทาง Strafe
        float dir = Random.value > 0.5f ? 1f : -1f; 

        while (timer < strafeDuration)
        {
            timer += Time.deltaTime;

            // 🔹 Rotation: ให้หันหน้าหาผู้เล่นตลอดเวลาขณะ Strafe
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * boss.rotationSpeed
            );

            // 🔹 Movement: เดินวนรอบผู้เล่น (ใช้ transform.right)
            Vector3 strafeDir = transform.right * dir;
            Vector3 move = strafeDir * boss.strafeSpeed * Time.deltaTime;

            // เพิ่มแรงโน้มถ่วงกันอาการกระตุก (สำคัญ)
            move.y += -1f * Time.deltaTime;

            boss.controller.Move(move);

            yield return null;
        }

        isStrafing = false;
        // ❗ เมื่อ Strafe จบ ให้กลับไปที่สถานะ Chase
        boss.currentState = BossManager.BossState.Chase; 
    }
    
    // ❌ ลบ FacePlayer() ออก
}