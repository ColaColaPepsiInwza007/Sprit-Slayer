using UnityEngine;

public class BossDamageDealer : MonoBehaviour
{
    // *** Component นี้ต้องถูกใส่ไว้บน GameObject ที่เป็น Hitbox ของ Boss (เช่น อาวุธ) ***
    
    [Header("Damage Settings")]
    [SerializeField] private float attackDamage = 20f; 
    
    private Collider damageCollider; // ตัวแปรสำหรับเก็บ Collider (ต้องมี Collider ติดอยู่กับ GameObject นี้)
    private bool hasDealtDamage = false; 

    private void Awake()
    {
        // *** 1. หา Collider ***
        damageCollider = GetComponent<Collider>();
        
        // *** 2. ปิด Hitbox ทันทีเมื่อเกมเริ่ม เพื่อป้องกันดาเมจตอนเดิน ***
        if (damageCollider != null)
        {
            damageCollider.enabled = false; 
        }
        else
        {
            Debug.LogError("BossDamageDealer requires a Collider component on the same GameObject!");
        }
    }

    // ฟังก์ชันนี้ถูกเรียกโดย BossAnimationEvents เมื่อ Hitbox ควรจะทำงาน
    public void EnableDamageCollider()
    {
        // *** 1. รีเซ็ตสถานะการทำดาเมจ เพื่อให้ตีซ้ำรอบใหม่ได้ ***
        hasDealtDamage = false; 
        
        // *** 2. เปิด Hitbox ***
        if (damageCollider != null)
        {
            damageCollider.enabled = true; 
            Debug.Log("Boss Damage: Hitbox ENABLED and RESET.");
        }
    }

    // ฟังก์ชันนี้ถูกเรียกโดย BossAnimationEvents เมื่อ Hitbox ควรจะหยุดทำงาน
    public void DisableDamageCollider()
    {
        // ปิด Hitbox
        if (damageCollider != null)
        {
            damageCollider.enabled = false;
            Debug.Log("Boss Damage: Hitbox DISABLED.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. ถ้าดาเมจถูกทำไปแล้วในรอบนี้ ไม่ต้องทำซ้ำ
        if (hasDealtDamage) return;

        // 2. ตรวจสอบว่าชน Player หรือไม่ (ต้องมั่นใจว่า Player มี Tag "Player")
        if (other.CompareTag("Player"))
        {
            // 3. พยายามดึง PlayerStats component
            PlayerStats playerStats = other.GetComponent<PlayerStats>();

            if (playerStats != null)
            {
                // 4. สั่งให้ Player รับดาเมจ
                playerStats.TakeDamage(attackDamage);
                
                // 5. ป้องกันการทำดาเมจซ้ำในเฟรมเดียวกัน
                hasDealtDamage = true; 
            }
        }
    }
}