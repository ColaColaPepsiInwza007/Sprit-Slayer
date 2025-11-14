using UnityEngine;

public class BossCombatFX : MonoBehaviour
{
    [Header("Impact Effects Settings")]
    public GameObject impactParticlePrefab; // Asset Particle Effect (ลากใส่ใน Unity Inspector)
    public AudioClip impactSound;           // Asset เสียงกระทบ
    private AudioSource audioSource;         

    private void Awake()
    {
        // ตรวจสอบและเพิ่ม AudioSource ถ้ายังไม่มี
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // ฟังก์ชันสำหรับเล่น Impact Effect (เรียกจาก WeaponHitbox)
    public void PlayImpactEffect(Vector3 impactPosition)
    {
        // 1. **แสดง Particle Effect**
        if (impactParticlePrefab != null)
        {
            // สร้าง Effect ที่ตำแหน่งที่ดาบชน
            GameObject impact = Instantiate(impactParticlePrefab, impactPosition, Quaternion.identity);
            
            // ให้ Effect ทำลายตัวเองเมื่อเล่นเสร็จ
            ParticleSystem ps = impact.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(impact, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(impact, 2f); 
            }
        }

        // 2. **เล่นเสียงกระทบ**
        if (audioSource != null && impactSound != null)
        {
            audioSource.PlayOneShot(impactSound); 
        }
    }
}