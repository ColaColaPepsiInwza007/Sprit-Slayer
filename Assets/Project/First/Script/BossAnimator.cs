using UnityEngine;

public class BossAnimator : MonoBehaviour
{
    private BossManager manager;
    private Animator animator;

    [Header("Animation Speed")]
    [SerializeField] private float animationSpeedMultiplier = 1.0f; // 1.0 = ความเร็วปกติ, 2.0 = เร็ว 2 เท่า

    private void Awake()
    {
        manager = GetComponent<BossManager>();
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("BossAnimator: Animator component is missing on the Boss GameObject.");
        }
        
        // *** โค้ดที่แก้ไข: ตั้งค่าความเร็วเริ่มต้นของ Animator Controller ทั้งหมด ***
        if (animator != null)
        {
             animator.speed = animationSpeedMultiplier;
        }
    }

    /// <summary>
    /// ปรับค่า MoveY (Animation Blend Tree) เพื่อควบคุมการเดิน/วิ่ง
    /// </summary>
    public void UpdateMovement(float moveAmount)
    {
        if (animator == null) return;
        // 0.1f คือ damp time ช่วยให้การเปลี่ยนแอนิเมชันนุ่มนวล
        animator.SetFloat("MoveY", moveAmount, 0.1f, Time.deltaTime);
    }
    
    /// <summary>
    /// ยิง Trigger เพื่อเริ่มแอนิเมชันโจมตี (Attack1, Attack2, ฯลฯ)
    /// </summary>
    public void TriggerAttack(int attackIndex) 
    {
        // ตอนนี้เราใช้ Trigger "Attack" ตัวเดียวตามโค้ดก่อนหน้า
        if (animator == null) return;
        
        animator.SetTrigger("Attack");
    }
    
    /// <summary>
    /// ฟังก์ชันสำหรับปรับความเร็วแอนิเมชันของ Boss แบบ Dynamic (ถ้าจำเป็น)
    /// </summary>
    public void SetAnimationSpeed(float multiplier)
    {
        if (animator == null) return;
        animator.speed = multiplier;
        animationSpeedMultiplier = multiplier; // อัปเดตตัวแปรใน Inspector ด้วย
    }
}