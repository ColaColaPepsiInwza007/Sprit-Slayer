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
}