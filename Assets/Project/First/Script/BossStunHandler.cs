using UnityEngine;

// ❗️ 1. ต้องมี 2 บรรทัดนี้ ❗️
[RequireComponent(typeof(BossManager))]
[RequireComponent(typeof(BossAnimator))]
public class BossStunHandler : MonoBehaviour
{
    // ❗️ 2. ดึง Component หลัก ❗️
    private BossManager bossManager;
    private BossAnimator bossAnim;

    // ❗️ 3. ย้ายการตั้งค่า Stun ทั้งหมดมาที่นี่ ❗️
    [Header("Stun Settings")]
    public float stunFallTime = 1.5f;     // 1. เวลาที่แอนิเมชั่น "ล้ม" เล่น
    public float stunPauseTime = 2.0f;    // 2. เวลาที่ "นั่งนิ่งๆ"
    public float stunRecoveryTime = 1.0f; // 3. เวลาที่ใช้ "ลุกขึ้น"

    private float stunTimer; // ตัวนับเวลา

    // ❗️ 4. สร้าง State Machine ของการ Stun ❗️
    private enum StunPhase
    {
        None,       // (ไม่ได้ Stun)
        Falling,    // (กำลังล้ม)
        Paused,     // (นั่งนิ่งๆ)
        Recovering  // (กำลังลุก)
    }
    private StunPhase currentPhase = StunPhase.None;

    // ❗️ 5. ตรวจสอบว่ากำลัง Stun อยู่หรือไม่ ❗️
    public bool IsStunned => currentPhase != StunPhase.None;

    private void Awake()
    {
        bossManager = GetComponent<BossManager>();
        bossAnim = GetComponent<BossAnimator>();
    }

    // ❗️ 6. ฟังก์ชันที่ BossManager จะเรียก (จาก Update) ❗️
    public void HandleStunUpdate()
    {
        if (!IsStunned) return; // ถ้าไม่ Stun ก็ไม่ต้องทำอะไร

        // นับเวลาถอยหลัง
        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0)
        {
            // เช็คว่าจบจังหวะไหน
            switch (currentPhase)
            {
                // จบจังหวะ 1 (Falling)
                case StunPhase.Falling:
                    currentPhase = StunPhase.Paused;
                    stunTimer = stunPauseTime; // เริ่มจังหวะ 2
                    Debug.Log("Boss is Down. Pausing.");
                    // (เสริม) bossAnim?.TriggerStunIdleLoop();
                    break;

                // จบจังหวะ 2 (Paused)
                case StunPhase.Paused:
                    currentPhase = StunPhase.Recovering;
                    stunTimer = stunRecoveryTime; // เริ่มจังหวะ 3
                    Debug.Log("Boss is Recovering.");
                    // (เสริม) bossAnim?.TriggerGetUp();
                    break;

                // จบจังหวะ 3 (Recovering)
                case StunPhase.Recovering:
                    FinishStun();
                    break;
            }
        }
    }

    // ❗️ 7. ฟังก์ชันเริ่ม Stun (BossManager จะเรียก) ❗️
    public void StartStun()
    {
        if (IsStunned) return; // ไม่ Stun ซ้ำ

        Debug.Log("BOSS IS STUNNED! (Phase 1: Falling)");
        currentPhase = StunPhase.Falling;
        stunTimer = stunFallTime; // เริ่มจังหวะ 1

        bossAnim?.TriggerStun(); // เล่นแอนิเมชั่น "ล้ม"
    }

    // ❗️ 8. ฟังก์ชันจบ Stun (เรียก BossManager กลับมา) ❗️
    private void FinishStun()
    {
        Debug.Log("Stun Handler finished.");
        currentPhase = StunPhase.None;
        
        // บอก BossManager ว่า "ฉันลุกเสร็จแล้ว"
        bossManager.RecoverFromStun(); 
    }
}