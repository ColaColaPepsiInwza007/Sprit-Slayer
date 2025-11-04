using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(BossMovement))] 
[RequireComponent(typeof(BossAnimator))] 
[RequireComponent(typeof(BossAnimationEvents))] 
public class BossManager : MonoBehaviour
{
    // === ENUM: สถานะของ Boss (ลบ Baiting) ===
    public enum BossState
    {
        Idle,
        Chase,      // เดินตรงเข้าหา/ตอบโต้การเคลื่อนไหว
        Attack,     
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

    [Header("Tactical Movement")] // ตัวแปรที่ยังอาจใช้ใน BossMovement
    [SerializeField] public float strafeSpeed = 4.5f;        
    [SerializeField] public float baitingDistance = 6.0f;   

    [Header("Attack Settings")]
    public float attackCooldown = 0.5f;
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
                // BossManager สั่งหยุดแอนิเมชันเดินเท่านั้น การหันจะถูกจัดการใน BossMovement
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
                
            case BossState.Idle:
                if (bossAnim != null) bossAnim.UpdateMovement(0f);
                break;
        }
    }

    public void RequestAttack()
    {
        if (currentState == BossState.Attack || attackTimer > 0)
        {
            return;
        }
        
        Debug.Log("Boss: Requesting Attack. State set to ATTACK.");
        
        currentState = BossState.Attack;
        DecideAndExecuteAttack();
    }

    private void DecideAndExecuteAttack()
    {
        if (bossAnim != null) bossAnim.TriggerAttack(1); 
        
        Debug.Log("Boss: Trigger 'Attack' Fired!");
        
        attackTimer = attackCooldown;
    }
}