using UnityEngine;

public class BossAnimationEvents : MonoBehaviour
{
    private BossManager manager;

    [Header("Damage Dealer Reference")]
    public BossDamageDealer damageDealer; 

    private void Awake()
    {
        manager = GetComponent<BossManager>();
    }
    [HideInInspector] public bool isRecoveringFromAttack = false;

public void AnimationAttackFinished()
{
    Debug.Log("🟢 AnimationAttackFinished() called from Animation Event");

    manager.currentComboIndex = 0;
    manager.ResetComboTimers();

    if (manager.bossAnim != null && manager.bossAnim.animator != null)
    {
        manager.bossAnim.animator.ResetTrigger("Attack1");
        manager.bossAnim.animator.ResetTrigger("Attack2");
        manager.bossAnim.animator.ResetTrigger("Attack3");
        manager.bossAnim.animator.SetTrigger("ComboExit");
        Debug.Log("🟢 ComboExit trigger sent to Animator");
    }

    // ให้กลับไปไล่ผู้เล่นต่อ
    manager.currentState = BossManager.BossState.Chase;

    // 🔹 เพิ่มบรรทัดนี้: เข้าสู่โหมด recovery
    manager.isRecoveringFromAttack = true;

    // ❗️❗️❗️ บรรทัดที่ต้องเพิ่มอยู่ตรงนี้ ❗️❗️❗️
    // ------------------------------------------------------------------
    manager.isPlayingAnimation = false; 
    Debug.Log("🟢 Boss: Combo ended — setting isPlayingAnimation to FALSE.");
    // ------------------------------------------------------------------

    Debug.Log("🟢 Boss: Combo ended — entering Tactical Recovery.");
}

    public void AnimationComboCheck()
    {
        manager.CheckForNextCombo();
    }
    
    public void EnableAttackDamage()
    {
        if (damageDealer != null) 
        {
            damageDealer.EnableDamageCollider();
            Debug.Log("Boss Damage: Hitbox ENABLED.");
        }
    }

    public void DisableAttackDamage()
    {
        if (damageDealer != null)
        {
            damageDealer.DisableDamageCollider();
            Debug.Log("Boss Damage: Hitbox DISABLED.");
        }
    }
    public void AnimationAttackEndIfSingle()
    {
        // 🔹 ถ้าคอมโบมีแค่ 1 hit ให้จบเลย
        if (manager.maxComboCount == 1)
        {
            Debug.Log("Boss: Single attack finished — returning to Chase.");
            manager.currentComboIndex = 0;
            manager.ResetComboTimers();

            if (manager.bossAnim != null && manager.bossAnim.animator != null)
            {
                manager.bossAnim.animator.SetTrigger("ComboExit");
            }

            manager.currentState = BossManager.BossState.Chase;
        }
        else
        {
            // 🔸 ถ้ามีมากกว่า 1 hit ไม่ต้องทำอะไร ให้ต่อคอมโบได้
            Debug.Log("Boss: Combo continues — not exiting yet.");
        }
    }
public void AnimationAttackStart()
{
    if (manager == null) manager = GetComponent<BossManager>();
    manager.isPlayingAnimation = true;
    Debug.Log("Boss Animation Start → movement locked.");
}

    public void AnimationAttackEnd()
    {
        if (manager == null) manager = GetComponent<BossManager>();

        // ✅ 1. ปลดล็อคสถานะการเล่น Animation
        manager.isPlayingAnimation = false;

        // ✅ 2. รีเซ็ตคอมโบ (เผื่อเป็นท่าสุดท้าย)
        manager.currentComboIndex = 0;
        manager.ResetComboTimers();

        // ✅ 3. สั่ง Animator ให้กลับ Idle
        if (manager.bossAnim != null && manager.bossAnim.animator != null)
        {
            manager.bossAnim.animator.SetTrigger("ComboExit");
        }

        // ✅ 4. ❗️❗️❗️ เปลี่ยน: เข้าสู่สถานะ Recovery ❗️❗️❗️
        // (BossManager.cs จะเปลี่ยนกลับเป็น Chase ให้อัตโนมัติหลัง postAttackRecoveryTime)
        manager.isRecoveringFromAttack = true;
        manager.recoveryTimer = manager.postAttackRecoveryTime; // ❗️ เริ่มนับถอยหลัง
        manager.currentState = BossManager.BossState.Idle; // ❗️ เปลี่ยน: ให้เป็น Idle ชั่วคราว

        Debug.Log("Boss Animation End → Entering Recovery state.");
    }
// 🟢 ให้เรียกจาก Event ได้ เพื่อ reset
    public void ResetStrafeState()
    {
        var move = manager.GetComponent<BossMovement>();
        if (move != null)
        {
            // (เรายังไม่มีฟังก์ชันนี้ใน BossMovement แต่เพิ่มไว้ก่อนได้)
            // move.ResetStrafeState(); 
        }
    }

}