using UnityEngine;

// (*** 🚀 PlayerManager (อัปเดต 9: "Refactor" ย้าย Input Buffering ออก) 🚀 ***)

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerStats))] 
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerLockOn))]
public class PlayerManager : MonoBehaviour
{
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
    public PlayerMovement movement; // (*** ❗️ ชื่อนี้ ❗️ ***)
    public PlayerAnimator animHandler;
    public PlayerLockOn lockOn;
    public WeaponHitbox weaponHitbox;
    public Transform cameraMainTransform;
    
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
        
        // (*** ❗️❗️❗️ "เช็ค" บรรทัดนี้ให้ดี ❗️❗️❗️ ***)
        movement = GetComponent<PlayerMovement>(); 
        
        animHandler = GetComponent<PlayerAnimator>();
        lockOn = GetComponent<PlayerLockOn>();
        weaponHitbox = GetComponentInChildren<WeaponHitbox>(); 
        if (Camera.main != null) { cameraMainTransform = Camera.main.transform; }
        
        groundCheckOffset = new Vector3(0, groundCheckDistance, 0); 
        
        weaponInHandModel.SetActive(isWeaponDrawn);        
        swordInScabbardModel.SetActive(!isWeaponDrawn);    
        if (scabbardModel != null) scabbardModel.SetActive(true); 
        animHandler.SetArmed(isWeaponDrawn);
        
        LockMouse(); 
    }

    private void Start()
    {
        SwitchState(idleState);
    }

    private void Update()
    {
        float delta = Time.deltaTime; 
        HandleGroundCheck(); 
        
        if (jumpCooldownTimer > 0) { jumpCooldownTimer -= delta; }

        // (*** (พอไม่แครช... 2 บรรทัดนี้จะกลับมาทำงาน) ***)
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
    public void OpenHitbox() { if (weaponHitbox != null) weaponHitbox.OpenHitbox(); }
    
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
    }
    
    private void OnDrawGizmosSelected()
    {
        Vector3 offset = new Vector3(0, groundCheckDistance, 0); 
        Vector3 checkPoint = transform.position + offset;
        Gizmos.color = isGrounded ? Color.green : Color.red; 
        Gizmos.DrawWireSphere(checkPoint, groundCheckRadius);
    }
}