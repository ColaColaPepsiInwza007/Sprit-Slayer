using UnityEngine;
using Random = UnityEngine.Random;

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
        Attack,     
        Stunned,
        Dead
    }

    public enum BossPhase { Phase1, Phase2, Phase3 }

    [Header("State Control")]
    public BossState currentState = BossState.Chase; 
    public BossPhase currentPhase = BossPhase.Phase1; 

    [Header("Core Components")]
    public CharacterController controller;
    public Transform playerTarget; 
    public BossAnimator bossAnim; 

    [Header("Boss Health")] 
    public float maxHealth = 1000f;
    public float currentHealth;

    [Header("Phase Transition Settings")]
    [SerializeField] private float phase2HealthThreshold = 500f; 
    [SerializeField] private float phase3HealthThreshold = 250f; 

    [Header("Boss State")]
    public float movementSpeed = 4.0f; 
    public float rotationSpeed = 10.0f;
    public float stoppingDistance = 1.5f; 

    [Header("Tactical Movement")] 
    [SerializeField] public float strafeSpeed = 4.5f;        
    [SerializeField] public float baitingDistance = 6.0f;   

    [Header("Attack Settings")]
    public float attackCooldown = 1.0f; // ⏱️ เย็นตัวหลังโจมตี
    private float attackTimer;

    [Header("Combo Settings")]
    public int maxComboCount = 3;
    public int currentComboIndex = 0; 
    [SerializeField] private float comboResetTime = 1.0f; 
    private float comboTimer; 
    
    [SerializeField] private float comboBufferTime = 0.15f; 
    private float continueComboTimer = 0f; 

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        bossAnim = GetComponent<BossAnimator>(); 
        currentHealth = maxHealth;
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
    }

    private void Update()
    {
        HandlePhaseTransition();
        HandleBossState();

        // Combo timer
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                currentComboIndex = 0;
                continueComboTimer = 0f;

                if (currentState == BossState.Attack)
                {
                    currentState = BossState.Chase;
                }
            }
        }

        // Combo continuation buffer
        if (continueComboTimer > 0)
        {
            continueComboTimer -= Time.deltaTime;
            if (continueComboTimer <= 0)
            {
                DecideAndExecuteAttack(); 
            }
        }

        // Cooldown timer
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }
    
    private void HandlePhaseTransition()
    {
        if (currentPhase == BossPhase.Phase1 && currentHealth <= phase2HealthThreshold)
        {
            currentPhase = BossPhase.Phase2;
            Debug.Log("BOSS PHASE TRANSITIONED TO PHASE 2!");
        }
        else if (currentPhase == BossPhase.Phase2 && currentHealth <= phase3HealthThreshold)
        {
            currentPhase = BossPhase.Phase3;
            Debug.Log("BOSS PHASE TRANSITIONED TO PHASE 3!");
        }
    }

    private void HandleBossState()
    {
        switch (currentState)
        {
            case BossState.Chase:
                if (bossAnim != null)
                {
                    bossAnim.UpdateMovement(1f);
                }
                break;

            case BossState.Attack:
                if (bossAnim != null) bossAnim.UpdateMovement(0f);

                if (playerTarget == null)
                {
                    currentState = BossState.Chase;
                    return;
                }

                // 🔹 ถ้าอยู่ไกลเกิน ให้กลับไป Chase
                float dist = Vector3.Distance(transform.position, playerTarget.position);
                if (dist > stoppingDistance + 0.75f)
                {
                    currentState = BossState.Chase;
                    bossAnim.UpdateMovement(1f);
                }
                break;

            case BossState.Idle:
                if (bossAnim != null) bossAnim.UpdateMovement(0f);
                break;
        }
    }

public void RequestAttack()
{
    if (attackTimer > 0) return;
    if (currentState == BossState.Attack) return;

    currentState = BossState.Attack;

    // สุ่มว่าจะคอมโบหรือไม่ (ให้ AI มีผล)
    DecideAndExecuteAttack();
}

    private void DecideAndExecuteAttack()
    {
        continueComboTimer = 0f; 
        int nextAttackIndex = 0;

        if (currentComboIndex == 0)
        {
            if (currentPhase == BossPhase.Phase1)
            {
                nextAttackIndex = 1;
                maxComboCount = 3;
            }
            else
            {
                int randomChance = Random.Range(1, 101);
                if (randomChance > 60)
                {
                    nextAttackIndex = 4;
                    maxComboCount = 1;
                }
                else
                {
                    nextAttackIndex = 1;
                    maxComboCount = 3;
                }
            }
            currentComboIndex = nextAttackIndex;
        }
        else
        {
            currentComboIndex++;
            if (currentComboIndex > maxComboCount)
                currentComboIndex = 1;

            nextAttackIndex = currentComboIndex;
        }

        if (bossAnim != null)
            bossAnim.TriggerAttack(nextAttackIndex);

        Debug.Log($"Boss: Trigger 'Attack' {nextAttackIndex} Fired! Phase: {currentPhase}");

        // ตั้งคูลดาวน์ใหม่
        attackTimer = attackCooldown;
        comboTimer = comboResetTime;
    }

    public void CheckForNextCombo()
    {
        if (currentComboIndex >= maxComboCount || comboTimer <= 0)
        {
            currentComboIndex = 0;
            comboTimer = 0;
            continueComboTimer = 0f;
            Debug.Log("Combo: Finished or timed out.");
        }
        else
        {
            continueComboTimer = comboBufferTime;
            Debug.Log($"Combo Check: Starting {comboBufferTime}s buffer for next hit (Hit {currentComboIndex + 1})");
        }
    }

    public void ResetComboTimers()
    {
        comboTimer = 0f;
        continueComboTimer = 0f;
    }
}
