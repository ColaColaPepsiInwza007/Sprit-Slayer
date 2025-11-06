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

public void AnimationAttackFinished()
{
    Debug.Log("🟢 AnimationAttackFinished() called from Animation Event");

    // ❗ ไม่ต้องเช็ก currentState == Attack อีกต่อไป
    manager.currentComboIndex = 0;
    manager.ResetComboTimers();

    if (manager.bossAnim != null && manager.bossAnim.animator != null)
    {
        manager.bossAnim.animator.ResetTrigger("Attack1");
        manager.bossAnim.animator.ResetTrigger("Attack2");
        manager.bossAnim.animator.ResetTrigger("Attack3");

        // ส่ง Trigger ออกจากคอมโบ
        manager.bossAnim.animator.SetTrigger("ComboExit");
        Debug.Log("🟢 ComboExit trigger sent to Animator");
    }

    // ให้กลับไปไล่ผู้เล่นต่อ
    manager.currentState = BossManager.BossState.Chase;
    Debug.Log("🟢 Boss: Combo ended — back to CHASE state.");
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

}