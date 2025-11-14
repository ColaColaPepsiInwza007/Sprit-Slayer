using UnityEngine;
using System.Collections.Generic; 

public class WeaponHitbox : MonoBehaviour
{
    public Collider weaponCollider; 
    private List<Collider> targetsHit;
    
    [Header("Damage Settings")]
    public float baseDamage = 50f; 
    private PlayerManager manager;

    // ❗️❗️ เพิ่มตัวแปรเพื่อเก็บค่า Stance Damage ของการโจมตีปัจจุบัน ❗️❗️
    [HideInInspector] public float currentStanceDamage = 0f;

    private void Awake()
    {
        if (weaponCollider == null)
        {
            weaponCollider = GetComponent<Collider>();
        }
        weaponCollider.enabled = false; 
        targetsHit = new List<Collider>();
        
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
        if (other.CompareTag("Enemy") || other.CompareTag("Boss")) // ใช้ Boss แทน Enemy เพื่อความชัวร์
        {
            if (targetsHit.Contains(other))
            {
                return; // ไม่ตีซ้ำ
            }
            
            targetsHit.Add(other);
            
            BossManager boss = other.GetComponent<BossManager>();
            
            if (boss != null)
            {
                // 1. กำหนดตำแหน่งการชน
                // ใช้ตำแหน่งกึ่งกลางของ Collider ของบอสเป็นการประมาณจุดชน
                Vector3 impactPoint = other.bounds.center; 

                // 2. เรียกฟังก์ชันแสดง Effect ผ่าน BossCombatFX
                BossCombatFX fx = other.GetComponent<BossCombatFX>();
                if (fx != null)
                {
                    fx.PlayImpactEffect(impactPoint); // 💥 เล่นเอฟเฟกต์!
                }
                
                // 3. คำนวณดาเมจ
                float finalDamage = baseDamage;
                
                if (manager != null && manager.isAttacking && manager.currentAttackData != null)
                {
                    finalDamage *= manager.currentAttackData.damageMultiplier; 
                    
                    // ❗️❗️ ส่งค่า Stance Damage ไปให้ Boss (ต้องเพิ่มฟังก์ชัน TakeStanceDamage ใน BossManager.cs) ❗️❗️
                    // boss.TakeStanceDamage(manager.currentAttackData.poiseDamage); 
                }
                
                // 4. เรียกฟังก์ชันลดเลือด
                boss.TakeDamage(finalDamage); 
                Debug.Log($"Hit Boss: {boss.name} for {finalDamage} damage. (Stance Damage: {manager.currentAttackData?.poiseDamage})");
            }
        }
    }
}