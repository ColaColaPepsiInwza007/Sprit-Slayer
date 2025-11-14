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
    public BossCombatFX combatFX;

    [Header("Boss Health")]
    public float maxHealth = 1000f;
    public float currentHealth;
    
    [Header("Stance System")]
    public float maxStance = 100f;
    public float currentStance;
    public float stanceDecayRate = 10f; // ค่า Stance ที่ลดลงต่อวินาที
    public float stanceDecayDelay = 3f;  // เวลาที่รอ (หลังจากโดนตี) ก่อนที่ค่า Stance จะเริ่มลด
    private float stanceDecayTimer;      // ตัวนับเวลา
    
    // ❗️❗️ 1. (แก้ไข) เพิ่มตัวแปร Stun ที่ขาดหายไป ❗️❗️
    [Header("Stun Settings")]
    public float stunFallTime = 1.5f;     // 1. เวลาที่แอนิเมชั่น "ล้ม" เล่น
    public float stunPauseTime = 2.0f;    // 2. เวลาที่ "นั่งนิ่งๆ"
    public float stunRecoveryTime = 1.0f; // 3. เวลาที่ใช้ "ลุกขึ้น"
    
    private float stunTimer; // ตัวนับเวลา
    
    private bool isStunnedDown = false;     // สถานะ: บอสล้มถึงพื้นหรือยัง?
    private bool isRecovering = false;  // สถานะ: บอสกำลังลุกหรือไม่?

    // ❗️❗️ 2. (แก้ไข) จัดระเบียบ Header ที่ซ้ำซ้อน ❗️❗️
    [Header("UI References")]
    public HPBarUI bossHPBarUI;
    public BossStanceBarUI bossStanceBarUI; // อ้างอิงถึง Script Stance Bar

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

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        bossAnim = GetComponent<BossAnimator>();
        combatFX = GetComponent<BossCombatFX>();
        currentHealth = maxHealth;
        
        // ❗️❗️ 3. (แก้ไข) ย้ายการตั้งค่า Stance มาไว้ด้วยกัน ❗️❗️
        currentStance = 0f; // เริ่มที่ 0
        if (bossStanceBarUI != null)
        {
            bossStanceBarUI.UpdateStanceBar(currentStance, maxStance);
        }
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTarget = playerObj.transform;
            
        UpdateHealthBar(); 
    }

    private void Update()
    {
        HandlePhaseTransition();
        HandleBossState();

        // ❗️❗️ นี่คือ Logic 3 จังหวะ (ที่ตอนนี้ทำงานได้แล้ว) ❗️❗️
        if (currentState == BossState.Stunned)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0)
            {
                // จังหวะ 1: ล้มเสร็จ (isStunnedDown == false)
                if (isStunnedDown == false && isRecovering == false)
                {
                    isStunnedDown = true;
                    stunTimer = stunPauseTime; // << เริ่ม "จังหวะ 2: นั่งนิ่งๆ"
                    Debug.Log("Boss is Down. Pausing.");
                    
                    // (เสริม) ถ้ามีแอนิเมชั่น "นั่งนิ่งๆ" (Idle Stun Loop) ให้ Trigger ที่นี่
                    // bossAnim?.TriggerStunIdle();
                }
                
                // จังหวะ 2: นั่งนิ่งๆ เสร็จ (isStunnedDown == true)
                else if (isStunnedDown == true && isRecovering == false)
                {
                    isRecovering = true;
                    stunTimer = stunRecoveryTime; // << เริ่ม "จังหวะ 3: ลุกขึ้น"
                    Debug.Log("Boss is Recovering.");
                    
                    // (เสริม) ถ้ามีแอนิเมชั่น "ลุกขึ้น" ให้ Trigger ที่นี่
                    // bossAnim?.TriggerGetUp();
                }
                
                // จังหวะ 3: ลุกขึ้นเสร็จ
                else if (isRecovering == true)
                {
                    RecoverFromStun(); // กลับไป Chase
                }
            }
        }
        // (ย้าย Stance Decay มาไว้ใน else if เหมือนเดิม)
        else if (currentStance > 0) 
        {
            HandleStanceDecay(Time.deltaTime);
        }

        // --- (โค้ดเดิมของคุณทั้งหมด) ---

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

    private void HandleStanceDecay(float delta)
    {
        if (currentStance <= 0) return; // ไม่ต้อง Decay ถ้าค่าเป็น 0

        // ถ้าตัวนับเวลายังไม่หมด (เพิ่งโดนตี)
        if (stanceDecayTimer > 0)
        {
            stanceDecayTimer -= delta;
        }
        // ถ้าตัวนับเวลาหมดแล้ว
        else
        {
            // ให้ค่า Stance ค่อยๆ ลดลง
            currentStance -= stanceDecayRate * delta;
            if (currentStance < 0) currentStance = 0;
            
            // อัปเดต UI
            bossStanceBarUI?.UpdateStanceBar(currentStance, maxStance);
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
            
            // ❗️❗️ 4. (แก้ไข) เพิ่ม Case Stunned ❗️❗️
            // เพื่อให้บอส "หยุด" เคลื่อนที่ตอนล้ม
            case BossManager.BossState.Stunned:
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

    public void TakeDamage(float damageAmount)
    {
        if (currentState == BossState.Dead) return;

        currentHealth -= damageAmount;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            currentState = BossState.Dead;
            Die();
        }
        // else { bossAnim?.TriggerHurt(); } // อาจจะเพิ่มแอนิเมชั่นบาดเจ็บ
        
        HandlePhaseTransition();
    }

    public void TakeStanceDamage(float stanceDamageAmount)
    {
        if (currentState == BossState.Dead || currentState == BossState.Stunned) return;

        // 1. เพิ่มค่า Stance
        currentStance += stanceDamageAmount;
        
        // 2. รีเซ็ตตัวนับเวลา Decay (เพื่อให้เริ่มนับใหม่)
        stanceDecayTimer = stanceDecayDelay;

        // 3. อัปเดต UI Bar
        bossStanceBarUI?.UpdateStanceBar(currentStance, maxStance);

        // 4. ตรวจสอบว่า Stun หรือยัง
        if (currentStance >= maxStance)
        {
            // รีเซ็ตค่า Stance
            currentStance = 0f;
            
            // อัปเดต UI อีกครั้งให้เป็น 0
            bossStanceBarUI?.UpdateStanceBar(currentStance, maxStance);
            
            // ❗️ เรียกสถานะ Stun ❗️
            TriggerStun();
        }
    }
    
    // ❗️❗️ 5. (แก้ไข) TriggerStun ที่ถูกต้อง ❗️❗️
    private void TriggerStun()
    {
        Debug.Log("BOSS IS STUNNED!");
        
        // ❗️ 1. (สำคัญมาก!) เปลี่ยน State เป็น Stunned ❗️
        currentState = BossState.Stunned;

        // 2. เริ่มนับเวลา "ท่าล้ม" (1.5 วิ)
        stunTimer = stunFallTime; 
        isStunnedDown = false;    // ยังไม่ถึงพื้น
        isRecovering = false;     // ยังไม่ลุก
        
        // 3. เล่น Animation Stun
        bossAnim?.TriggerStun(); 
        
        // 4. หยุดการโจมตีทั้งหมด (โค้ดเดิม)
        isRecoveringFromAttack = false;
        recoveryTimer = 0f;
        ResetComboTimers();
    }

    private void UpdateHealthBar()
    {
        if (bossHPBarUI != null)
        {
            bossHPBarUI.UpdateHealthBar(currentHealth, maxHealth); 
        }
    }

    private void Die()
    {
        Debug.Log("Boss has been defeated! Current Phase: " + currentPhase.ToString());
        bossAnim?.TriggerDie();
        controller.enabled = false;
        // โค้ดอื่นๆ เมื่อบอสตาย
    }

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

    // ฟังก์ชันฟื้นตัวจาก Stun
    public void RecoverFromStun()
    {
        Debug.Log("Boss recovered from stun. Resuming CHASE.");
        
        isStunnedDown = false;
        isRecovering = false;
        currentState = BossState.Chase;
    }
}