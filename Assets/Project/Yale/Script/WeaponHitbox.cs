using UnityEngine;
using System.Collections.Generic; 

public class WeaponHitbox : MonoBehaviour
{
    public Collider weaponCollider; 
    private List<Collider> targetsHit;
    
    // ❗️❗️ 1. เพิ่มตัวแปรสำหรับค่าดาเมจ ❗️❗️
    [Header("Damage Settings")]
    public float baseDamage = 50f; 
    private PlayerManager manager;

    private void Awake()
    {
        if (weaponCollider == null)
        {
            weaponCollider = GetComponent<Collider>();
        }
        weaponCollider.enabled = false; 
        targetsHit = new List<Collider>();
        
        // ❗️ 2. ดึง PlayerManager เพื่อคำนวณดาเมจ ❗️
        manager = GetComponentInParent<PlayerManager>(); 
    }

    public void OpenHitbox() 
    { 
        targetsHit.Clear(); 
        weaponCollider.enabled = true; 
    }
    
    public void CloseHitbox() 
    { 
        weaponCollider.enabled = false; 
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. ตรวจสอบว่าเป็น "บอส" หรือ "ศัตรู"
        if (other.CompareTag("Enemy") || other.CompareTag("Enemy")) 
        {
            if (targetsHit.Contains(other))
            {
                return; // ไม่ตีซ้ำ
            }
            
            targetsHit.Add(other);
            
            // 2. ดึง Script BossManager
            BossManager boss = other.GetComponent<BossManager>();
            
            if (boss != null)
            {
                // 3. คำนวณดาเมจ
                float finalDamage = baseDamage;
                
                // ถ้า Player กำลังตีอยู่ ให้ดึง Damage Multiplier จาก AttackData
                if (manager != null && manager.isAttacking && manager.currentAttackData != null)
                {
                    // ใช้ Damage Multiplier จาก Scriptable Object (AttackData)
                    finalDamage *= manager.currentAttackData.damageMultiplier; 
                }
                
                // 4. เรียกฟังก์ชันลดเลือด
                boss.TakeDamage(finalDamage); 
                Debug.Log($"Hit Boss: {boss.name} for {finalDamage} damage.");
            }
        }
    }
}