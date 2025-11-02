using UnityEngine;

// มั่นใจว่ามี Component ที่จำเป็นอยู่บน GameObject เดียวกัน
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(BossMovement))] 
[RequireComponent(typeof(BossAnimator))] 
[RequireComponent(typeof(BossAnimationEvents))] 
public class BossManager : MonoBehaviour
{
    // === ENUM: สถานะของ Boss ===
    public enum BossState
    {
        Idle,
        Chase,
        Attack,     // Boss จะติดอยู่ใน State นี้จนกว่า Animation Event จะถูกยิง
        Stunned,
        Dead
    }

    [Header("State Control")]
    public BossState currentState = BossState.Chase; 

    [Header("Core Components")]
    public CharacterController controller;
    public Transform playerTarget; 
    public BossAnimator bossAnim; 

    [Header("Boss State")]
    public float movementSpeed = 4.0f; 
    public float rotationSpeed = 10.0f;
    public float stoppingDistance = 1.5f; 

    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    private float attackTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        bossAnim = GetComponent<BossAnimator>(); 
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
    }

    private void Update()
    {
        HandleBossState();
    }
    
    private void HandleBossState()
    {
        switch (currentState)
        {
            case BossState.Chase:
                // อัปเดตแอนิเมชันเดิน (เมื่อเดิน)
                float moveAmount = (playerTarget != null) ? 1f : 0f;
                if (bossAnim != null) bossAnim.UpdateMovement(moveAmount);
                break;

            case BossState.Attack:
                // (*** โค้ดแก้บั๊กสำคัญ: หยุดแอนิเมชันเดินทันที! ***)
                if (bossAnim != null) bossAnim.UpdateMovement(0f); 
                
                if (playerTarget == null) 
                {
                    currentState = BossState.Chase; 
                    return;
                }
                
                // นับ Cooldown
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }
                break;
                
            // ... (สถานะอื่น ๆ)
        }
    }

    // ฟังก์ชันที่ BossMovement เรียกเพื่อเริ่มการโจมตี
    public void RequestAttack()
    {
        if (currentState == BossState.Attack || attackTimer > 0)
        {
            return;
        }
        
        // DEBUG: ยืนยันการเปลี่ยน State
        Debug.Log("Boss: Requesting Attack. State set to ATTACK.");
        
        currentState = BossState.Attack;
        DecideAndExecuteAttack();
    }

    private void DecideAndExecuteAttack()
    {
        // 1. สั่งให้ทำแอนิเมชันโจมตีท่าที่ 1
        if (bossAnim != null) bossAnim.TriggerAttack(1); 
        
        // DEBUG: ยืนยันการยิง Trigger
        Debug.Log("Boss: Trigger 'Attack' Fired!");
        
        // 2. เริ่ม Cooldown
        attackTimer = attackCooldown;
        
        // 3. Boss จะติดอยู่ใน State.Attack จนกว่า Animation Event จะถูกยิง
    }
}