using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(BossMovement))]
[RequireComponent(typeof(BossAnimator))]
[RequireComponent(typeof(BossAnimationEvents))]
public class BossManager : MonoBehaviour
{
    // ✅ เพิ่ม Bait และ Reposition
    public enum BossState { Idle, Chase, Bait, Reposition, Attack, Stunned, Dead }
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
    

    // ❗️❗️ เพิ่มตัวแปรอ้างอิง UI ❗️❗️
    [Header("UI References")]
    public HPBarUI bossHPBarUI; // อ้างอิงถึง Script ที่ควบคุมแถบเลือด (HPBarUI.cs)

    [Header("Phase Thresholds")]
    [SerializeField] private float phase2HealthThreshold = 500f;
    [SerializeField] private float phase3HealthThreshold = 250f;
    

    [Header("Movement Settings")]
    public float movementSpeed = 4.0f;
    public float rotationSpeed = 10.0f;
    public float stoppingDistance = 1.5f;
    public float baitingDistance = 6.0f;
    public float strafeSpeed = 4.5f;

    [Header("Attack Settings")]
    public float attackCooldown = 1.0f;
    private float attackTimer;

    [Header("Combo Settings")]
    public int maxComboCount = 3;
    public int currentComboIndex = 0;
    [SerializeField] private float comboResetTime = 1.0f;
    private float comboTimer;
    [SerializeField] private float comboBufferTime = 0.15f;
    private float continueComboTimer = 0f;
    [HideInInspector] public int lastAttackIndex = 0;

    [Header("Recovery Settings")]
    public float postAttackRecoveryTime = 0.8f;
    public float recoveryTimer = 0f;
    [HideInInspector] public bool isPlayingAnimation = false;
    [HideInInspector] public bool isRecoveringFromAttack = false;

    [Header("Reposition Settings")]
    public float repositionTime = 1.5f;
    [HideInInspector] public float repositionTimer = 0f;


    [HideInInspector] public bool allowRootMotion = true;
    public BossCombatFX combatFX;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        bossAnim = GetComponent<BossAnimator>();
        combatFX = GetComponent<BossCombatFX>();
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTarget = playerObj.transform;
            
        // ❗️❗️ กำหนดเลือดเริ่มต้นและอัปเดตแถบเลือด UI ครั้งแรก ❗️❗️
        UpdateHealthBar(); 
    }

    private void Update()
    {
        HandlePhaseTransition();
        HandleBossState();

        // Combo timer logic
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                currentComboIndex = 0;
                continueComboTimer = 0f;

                if (currentState == BossManager.BossState.Attack)
                    currentState = BossManager.BossState.Chase;
            }
        }

        // Combo continuation
        if (continueComboTimer > 0)
        {
            continueComboTimer -= Time.deltaTime;
            if (continueComboTimer <= 0)
                DecideAndExecuteAttack();
        }

        // Cooldown
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        // Recovery phase check (ที่อัปเดตแล้ว)
        if (isRecoveringFromAttack)
        {
            recoveryTimer -= Time.deltaTime;
            if (recoveryTimer <= 0)
            {
                isRecoveringFromAttack = false;

                // Logic การตัดสินใจใหม่ (สุ่มถอย)
                int randomChoice = Random.Range(0, 100);
                if (randomChoice > 60) // 40% ที่จะถอย
                {
                    currentState = BossManager.BossState.Reposition;
                    repositionTimer = repositionTime; // เริ่มจับเวลาถอย
                    Debug.Log("Boss: Recovery finished, deciding to REPOSITION.");
                }
                else // 60% ที่จะไล่ต่อ
                {
                    currentState = BossManager.BossState.Chase;
                    Debug.Log("Boss: Recovery finished, deciding to CHASE.");
                }
            }
        }

        // Timer ของ Reposition
        if (currentState == BossManager.BossState.Reposition)
        {
            repositionTimer -= Time.deltaTime;
            if (repositionTimer <= 0)
            {
                currentState = BossManager.BossState.Chase; // ถอยเสร็จแล้ว ไล่ต่อ
                Debug.Log("Boss: Reposition finished, resuming CHASE.");
            }
        }
    }

    private void HandlePhaseTransition()
    {
        if (currentPhase == BossPhase.Phase1 && currentHealth <= phase2HealthThreshold)
            currentPhase = BossPhase.Phase2;
        else if (currentPhase == BossPhase.Phase2 && currentHealth <= phase3HealthThreshold)
            currentPhase = BossPhase.Phase3;
    }

    private void HandleBossState()
    {
        switch (currentState)
        {
            case BossManager.BossState.Chase:
                bossAnim?.UpdateMovement(1f); // วิ่ง
                break;

            case BossManager.BossState.Bait:
                // (ปล่อยให้ BossMovement สั่งแอนิเมชั่น)
                break;
            
            case BossManager.BossState.Reposition:
                bossAnim?.UpdateMovement(-1f); // ใช้แอนิเมชั่นเดินถอยหลัง
                break;

            case BossManager.BossState.Attack:
                bossAnim?.UpdateMovement(0f);
                break;

            case BossManager.BossState.Idle:
                bossAnim?.UpdateMovement(0f);
                break;
            
            case BossManager.BossState.Dead: // ❗️ เพิ่ม
                bossAnim?.UpdateMovement(0f);
                break;
        }
    }

    public void RequestAttack()
    {
        if (attackTimer > 0 || currentState == BossManager.BossState.Attack)
            return;

        // --- เพิ่มโค้ด (Snap Rotation) ---
        if (playerTarget != null)
        {
            Vector3 targetDirection = playerTarget.position - transform.position;
            targetDirection.y = 0;
            if (targetDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
                transform.rotation = targetRotation; // สั่งหมุนทันที
            }
        }
        // --- --------------------------------- ---

        currentState = BossManager.BossState.Attack;
        DecideAndExecuteAttack();
    }

    // ❗️❗️ ฟังก์ชันหลักสำหรับรับความเสียหาย ❗️❗️
    public void TakeDamage(float damageAmount)
    {
        if (currentState == BossState.Dead) return;

        currentHealth -= damageAmount;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        // อัปเดตแถบเลือด UI
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            currentState = BossState.Dead;
            Die();
        }
        // else { bossAnim?.TriggerHurt(); } // อาจจะเพิ่มแอนิเมชั่นบาดเจ็บ
        
        // ตรวจสอบ Phase Health
        HandlePhaseTransition();
    }

    // ❗️❗️ ฟังก์ชันอัปเดตแถบเลือด UI ❗️❗️
    private void UpdateHealthBar()
    {
        if (bossHPBarUI != null)
        {
            bossHPBarUI.UpdateHealthBar(currentHealth, maxHealth); 
        }
    }

    // ❗️❗️ ฟังก์ชันเมื่อบอสตาย ❗️❗️
    private void Die()
    {
        Debug.Log("Boss has been defeated! Current Phase: " + currentPhase.ToString());
        bossAnim?.TriggerDie();
        controller.enabled = false;
        // โค้ดอื่นๆ เมื่อบอสตาย
    }

    // โค้ดสำหรับสุ่มคอมโบ
    private void DecideAndExecuteAttack()
    {
        continueComboTimer = 0f;
        int nextAttackIndex;

        if (currentComboIndex == 0)
        {
            maxComboCount = 3; 
            nextAttackIndex = Random.Range(1, 4); 
            currentComboIndex = 1; 
        }
        else
        {
            nextAttackIndex = Random.Range(1, 4);
            while (nextAttackIndex == lastAttackIndex) 
            {
                nextAttackIndex = Random.Range(1, 4);
            }
            currentComboIndex++;
        }

        lastAttackIndex = nextAttackIndex;
        bossAnim?.TriggerAttack(nextAttackIndex);
        attackTimer = attackCooldown;
        comboTimer = comboResetTime;
    }

    // โค้ดสำหรับเช็ค Player หนี
    public void CheckForNextCombo()
    {
        // 1. ตรวจสอบว่าคอมโบจบหรือยัง
        if (currentComboIndex >= maxComboCount || comboTimer <= 0)
        {
            currentComboIndex = 0;
            comboTimer = 0;
            continueComboTimer = 0;
            if (currentState != BossManager.BossState.Chase)
            {
                currentState = BossManager.BossState.Chase;
                bossAnim.animator.SetTrigger("ComboExit");
            }
            return;
        }

        // 2. ตรวจสอบว่า Player ยังอยู่ในระยะหรือไม่
        if (playerTarget != null)
        {
            float dist = Vector3.Distance(transform.position, playerTarget.position);
            
            if (dist > stoppingDistance + 1.0f) 
            {
                Debug.Log("Boss: Player หนีไปไกล! บังคับจบคอมโบ");
                currentComboIndex = 0;
                comboTimer = 0;
                continueComboTimer = 0;

                // บังคับจบ
                bossAnim.animator.SetTrigger("ComboExit");
                currentState = BossManager.BossState.Chase; 
                isPlayingAnimation = false; 
                
                return;
            }
        }

        // 3. ถ้าทุกอย่าง OK ให้เตรียมต่อคอมโบ
        continueComboTimer = comboBufferTime;
    }

    public void ResetComboTimers()
    {
        comboTimer = 0f;
        continueComboTimer = 0f;
    }
}