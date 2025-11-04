using UnityEngine;

public class BossAnimationEvents : MonoBehaviour
{
    private BossManager manager;

    [Header("Damage Dealer Reference")]
    // ต้องลาก BossDamageDealer Component (ที่อยู่บน Hitbox/Weapon) มาใส่ใน Inspector
    public BossDamageDealer damageDealer; 

    private void Awake()
    {
        manager = GetComponent<BossManager>();
    }

    // --- ฟังก์ชันควบคุมจังหวะ State ---

    public void AnimationAttackFinished()
    {
        if (manager.currentState == BossManager.BossState.Attack)
        {
            manager.currentState = BossManager.BossState.Chase;
            Debug.Log("Boss: Attack Finished (Event Fired), Back to CHASE.");
        }
    }

    // --- ฟังก์ชันควบคุม Hitbox/Damage ---

    /// <summary>
    /// ฟังก์ชันนี้ถูกเรียกโดย Animation Event เมื่อ Hitbox ควรจะทำงาน
    /// </summary>
    public void EnableAttackDamage()
    {
        if (damageDealer != null) 
        {
            damageDealer.EnableDamageCollider();
        }
    }

    /// <summary>
    /// ฟังก์ชันนี้ถูกเรียกโดย Animation Event เมื่อ Hitbox ควรจะหยุดทำงาน
    /// </summary>
    public void DisableAttackDamage()
    {
        if (damageDealer != null) 
        {
            damageDealer.DisableDamageCollider();
        }
    }
}