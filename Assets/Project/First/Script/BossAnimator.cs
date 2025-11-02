using UnityEngine;

public class BossAnimator : MonoBehaviour
{
    private BossManager manager;
    private Animator animator;

    private void Awake()
    {
        manager = GetComponent<BossManager>();
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("BossAnimator: Animator component is missing on the Boss GameObject.");
        }
    }

    public void UpdateMovement(float moveAmount)
    {
        if (animator == null) return;
        // ใช้ MoveY เพื่อเปลี่ยนระหว่าง Idle/Walk
        animator.SetFloat("MoveY", moveAmount, 0.1f, Time.deltaTime);
    }
    
    // เราไม่ใช้ AttackIndex แล้ว ใช้แค่ Trigger
    public void TriggerAttack(int attackIndex) 
    {
        if (animator == null) return;
        
        // สั่ง Trigger "Attack" เท่านั้น
        animator.SetTrigger("Attack");
    }
}