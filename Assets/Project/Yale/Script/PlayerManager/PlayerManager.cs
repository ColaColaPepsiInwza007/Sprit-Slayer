using UnityEngine;

// (*** 🚀 PlayerManager (v11.3: Debug.Log ใน Awake) 🚀 ***)

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerStats))] 
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerLockOn))]
[RequireComponent(typeof(PlayerAudioController))] 
public class PlayerManager : MonoBehaviour
{
    // (ตัวแปรอื่นๆ เหมือนเดิมเป๊ะ... ไม่ต้องแก้)
    
    // ... (State, Core Components, VFX Root, ...)
    private PlayerBaseState currentState;
    public readonly PlayerIdleState idleState = new PlayerIdleState();
    public readonly PlayerMoveState moveState = new PlayerMoveState();
    public readonly PlayerRollState rollState = new PlayerRollState();
    public readonly PlayerAttackState attackState = new PlayerAttackState();
    
    [Header("Core Components")] 
    public CharacterController controller;
    public Animator animator;
    public PlayerStats stats; 
    public PlayerInputHandler inputHandler; 
    public PlayerMovement movement; 
    public PlayerAnimator animHandler;
    public PlayerLockOn lockOn;
    [HideInInspector] public PlayerAudioController audioController; 
    public WeaponHitbox weaponHitbox;
    public Transform cameraMainTransform;
    public GameObject swordVFXRoot; 
    private ParticleSystem[] vfxParticles;
    private TrailRenderer[] vfxTrails;
    
    [HideInInspector] public float lastAttackStartTime = 0f;
    
    [Header("Global Action States")]
    public bool isLanding = false; 
    public bool isAttacking = false; 
    public bool isRolling = false;   
    public bool isGrounded;
    public Transform lockedTarget; 
    
    [Header("Weapon Sockets")] 
    public GameObject weaponInHandModel; 
    public GameObject swordInScabbardModel; 
    public GameObject scabbardModel; 
    public bool isWeaponDrawn = false; 
    
    [Header("Mouse Lock Settings")] 
    public bool isMouseLocked = true; 
    
    [Header("Moveset & Combo")]
    public AttackData startingLightAttack; 
    [HideInInspector] public AttackData currentAttackData; 
    [HideInInspector] public AttackData attackToPlayNext;  
    public bool canCombo = false;     
    public bool canRollCancel = false; 
    
    [Header("Ground Check Settings")] 
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.2f; 
    private Vector3 groundCheckOffset;
    
    [Header("Rolling Settings")]
    [SerializeField] public float rollCost = 15f; 

    [Header("Stamina & Cooldowns")] 
    [SerializeField] public float jumpStaminaCost = 10f; 
    [SerializeField] public float jumpCooldown = 0.5f; 
    public float jumpCooldownTimer = 0f; 


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>(); 
        inputHandler = GetComponent<PlayerInputHandler>();
        movement = GetComponent<PlayerMovement>(); 
        animHandler = GetComponent<PlayerAnimator>();
        lockOn = GetComponent<PlayerLockOn>();
        audioController = GetComponent<PlayerAudioController>(); 
        weaponHitbox = GetComponentInChildren<WeaponHitbox>(); 
        if (Camera.main != null) { cameraMainTransform = Camera.main.transform; }
        groundCheckOffset = new Vector3(0, groundCheckDistance, 0); 
        weaponInHandModel.SetActive(isWeaponDrawn);        
        swordInScabbardModel.SetActive(!isWeaponDrawn);    
        if (scabbardModel != null) scabbardModel.SetActive(true); 
        animHandler.SetArmed(isWeaponDrawn);
        LockMouse(); 
        
        // --- ❗️❗️❗️ นี่คือส่วนที่อัปเกรด ❗️❗️❗️ ---
        if (swordVFXRoot != null)
        {
            vfxParticles = swordVFXRoot.GetComponentsInChildren<ParticleSystem>();
            vfxTrails = swordVFXRoot.GetComponentsInChildren<TrailRenderer>();
            
            // (1) ถ้าหาเจอ... มันจะบอกว่าเจอ "กี่อัน"
            Debug.Log($"[PlayerManager Awake] สำเร็จ! หาส่วนประกอบ VFX เจอ: {vfxParticles.Length} Particles และ {vfxTrails.Length} Trails.");
        }
        else
        {
            // (2) ถ้า 'Sword VFX Root' "ว่าง"... มันจะด่าเรา
            Debug.LogWarning("[PlayerManager Awake] ล้มเหลว! ช่อง 'Sword VFX Root' ใน Inspector มัน 'ว่าง' (None)!");
        }
        // --- ---------------------------------- ---
    }

    private void Start()
    {
        SwitchState(idleState);
        StopVFX(); // (เรียก StopVFX เหมือนเดิม)
    }

    // ... (โค้ด Update() และอื่นๆ เหมือนเดิมเป๊ะ) ...
    // ( ... SwitchState, HandleWeaponToggle, FinishRoll, IFrames, OpenHitbox, ...)
    // ( ... HandleGroundCheck, MouseLock, Combo, FinishAttack, ...)
    
    private void Update()
    {
        float delta = Time.deltaTime; 
        HandleGroundCheck(); 
        if (jumpCooldownTimer > 0) { jumpCooldownTimer -= delta; }
        if (inputHandler.drawWeaponInput) { HandleWeaponToggle(); }
        if (inputHandler.toggleMouseInput) { ToggleMouseLock(); }
        if (currentState != null)
        {
            currentState.Tick(this); 
        }
        stats.HandleStaminaRegen(delta);
    }

    public void SwitchState(PlayerBaseState newState)
    {
        if (currentState != null) { currentState.Exit(this); }
        currentState = newState;
        currentState.Enter(this);
    }
    
    
    private void HandleWeaponToggle()
    {
        isWeaponDrawn = !isWeaponDrawn; 
        animHandler.SetArmed(isWeaponDrawn);
        weaponInHandModel.SetActive(isWeaponDrawn);        
        swordInScabbardModel.SetActive(!isWeaponDrawn);    
        if (!isWeaponDrawn)
        {
            StopVFX();
        }
    }

    public void FinishRoll()
    {
        isRolling = false;
        if (currentState == rollState)
        {
            SwitchState(idleState);
        }
    }
    
    public void StartIFrames() { stats.isInvincible = true; }
    public void EndIFrames() { stats.isInvincible = false; } 
    
    public void OpenHitbox() 
    { 
        if (weaponHitbox != null) 
        {
            weaponHitbox.OpenHitbox(); 
        }
        StartVFX();
    }

    private void HandleGroundCheck()
    {
        Vector3 checkPoint = transform.position + groundCheckOffset;
        isGrounded = Physics.CheckSphere(checkPoint, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
        animHandler.SetGrounded(isGrounded); 
    }
    
    public void ToggleMouseLock()
    {
        isMouseLocked = !isMouseLocked;
        if (isMouseLocked) { LockMouse(); } else { UnlockMouse(); }
    }

    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isMouseLocked = true;
    }

    private void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isMouseLocked = false;
    }

    public void OpenComboWindow() 
    { 
        canCombo = true; 
        if (currentAttackData != null) { attackToPlayNext = currentAttackData.nextLightAttack; }
    }
    
    public void OpenRollCancelWindow() { canRollCancel = true; }

    public void FinishAttack()
    {
        if (Time.time - lastAttackStartTime < 0.2f) { return; }
        isAttacking = false; 
        canCombo = false;
        canRollCancel = false; 
        attackToPlayNext = null; 
        animator.SetTrigger("AttackExit"); 
        if (currentState == attackState) { SwitchState(idleState); }
    }
    
    public void CloseHitbox() 
    { 
        if (weaponHitbox != null) weaponHitbox.CloseHitbox(); 
        OpenRollCancelWindow();

        StopVFX(); 
        
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 offset = new Vector3(0, groundCheckDistance, 0); 
        Vector3 checkPoint = transform.position + offset;
        Gizmos.color = isGrounded ? Color.green : Color.red; 
        Gizmos.DrawWireSphere(checkPoint, groundCheckRadius);
    }
    
    // --- (ฟังก์ชัน StartVFX / StopVFX ที่มี Debug.Log) ---
    
    private void StartVFX()
    {
        // ❗️ เรายังเก็บ Log นี้ไว้
        Debug.Log("--- 🚀 STARTING VFX! 🚀 ---"); 

        if (vfxParticles == null || vfxTrails == null)
        {
            // ❗️ เพิ่ม Log บอกว่ามัน 'return' เพราะ 'null'
            Debug.LogWarning("StartVFX: vfxParticles หรือ vfxTrails เป็น 'null'! ...กำลัง 'return' ... (เช็ค Log สีเหลืองใน Awake)");
            return;
        }

        foreach (var p in vfxParticles)
        {
            p.Play(true); // (ใช้ Play(true) เพื่อบังคับให้มันรีสตาร์ท)
        }
        
        foreach (var t in vfxTrails)
        {
            t.Clear(); // (ล้าง Trail เก่าทิ้ง)
            t.emitting = true; 
        }
    }

    private void StopVFX()
    {
        Debug.Log("--- 🛑 STOPPING VFX! 🛑 ---");

        if (vfxParticles == null || vfxTrails == null)
        {
            return; // (ถ้ามัน 'null' ก็ไม่ต้องทำอะไร)
        }

        foreach (var p in vfxParticles)
        {
            p.Stop(true, ParticleSystemStopBehavior.StopEmitting); 
        }
        
        foreach (var t in vfxTrails)
        {
            t.emitting = false; 
        }
    }
}