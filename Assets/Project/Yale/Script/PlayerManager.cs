using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerLockOn))]
[RequireComponent(typeof(PlayerRoll))]
public class PlayerManager : MonoBehaviour
{
    [Header("Action States")]
    public bool isLanding = false; 

    [Header("Weapon Sockets")]
    public GameObject weaponInHand;     
    public GameObject swordInScabbard;  

    [Header("Core Components")]
    public CharacterController controller;
    public Animator animator;
    public PlayerStats stats;
    public PlayerInputHandler inputHandler;
    public PlayerMovement movement;
    public PlayerAnimator animHandler;
    public PlayerLockOn lockOn;
    public PlayerRoll rollHandler;

    [Header("Player State")]
    public bool isWeaponDrawn = false; 
    public bool isSprinting; 
    public bool isRolling;
    public bool isGrounded;
    public Transform lockedTarget; 

    [Header("Mouse Lock Settings")]
    public bool isMouseLocked = true; 

    [Header("Camera")]
    public Transform cameraMainTransform;

    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    private Vector3 groundCheckOffset;

    [Header("Stamina & Cooldowns")]
    [SerializeField] private float jumpStaminaCost = 10f; 
    [SerializeField] private float jumpCooldown = 0.5f; 
    private float jumpCooldownTimer = 0f;

    private float rollBufferTimer; 

    [Header("Tap vs Hold Input")]
    [SerializeField] private float tapRollThreshold = 0.2f; // (เวลาก่อนจะนับเป็น "Hold" ... 0.2 วิ)
    private float sprintInputTimer = 0f; // (ตัวนับเวลา)
    private bool isSprintButtonHeld = false; // (เช็คว่าปุ่มค้างอยู่มั้ย)

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();
        inputHandler = GetComponent<PlayerInputHandler>();
        movement = GetComponent<PlayerMovement>();
        animHandler = GetComponent<PlayerAnimator>();
        lockOn = GetComponent<PlayerLockOn>();
        rollHandler = GetComponent<PlayerRoll>();

        if (Camera.main != null) { cameraMainTransform = Camera.main.transform; }
        groundCheckOffset = new Vector3(0, controller.center.y, 0); 
        animator.applyRootMotion = false; 
        
        weaponInHand.SetActive(false);
        swordInScabbard.SetActive(true);
        isWeaponDrawn = false;
        
        LockMouse(); 
    }
    
    // (*** โค้ดใหม่: เอากลับมาแล้ว! ***)
    private void HandleGroundCheck()
    {
        Vector3 spherePosition = transform.position + groundCheckOffset;
        if (Physics.SphereCast(spherePosition, groundCheckRadius, Vector3.down, 
                               out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            isGrounded = true; 
        }
        else
        {
            isGrounded = false; 
        }
    }
    
    // (*** โค้ดใหม่: เอากลับมาแล้ว! ***)
    public void ToggleMouseLock()
    {
        isMouseLocked = !isMouseLocked; 
        if (isMouseLocked) { LockMouse(); }
        else { UnlockMouse(); }
    }

    // (*** โค้ดใหม่: เอากลับมาแล้ว! ***)
    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;                   
        isMouseLocked = true;
    }

    // (*** โค้ดใหม่: เอากลับมาแล้ว! ***)
    private void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;   
        Cursor.visible = true;                    
        isMouseLocked = false;
    }
    
    // (*** โค้ดใหม่: เอากลับมาแล้ว! ***)
    public void ToggleWeapon()
    {
        if (isRolling || isLanding || !isGrounded) return; 
        isWeaponDrawn = !isWeaponDrawn; 
        weaponInHand.SetActive(isWeaponDrawn);        
        swordInScabbard.SetActive(!isWeaponDrawn);    
        // animHandler.SetArmed(isWeaponDrawn); 
    }

    // (*** นี่คือ Update() ฉบับสมบูรณ์ ***)
    private void Update()
    {
        float delta = Time.deltaTime;

        HandleGroundCheck(); // <--- (*** บรรทัดนี้จะหาย Error แล้ว ***)
        animHandler.SetGrounded(isGrounded); 

        if (jumpCooldownTimer > 0) { jumpCooldownTimer -= delta; }

        // (Logic เช็ค Input)
        if (inputHandler.toggleMouseInput) { ToggleMouseLock(); }
        if (inputHandler.drawWeaponInput) { ToggleWeapon(); }

        // (Logic Tap vs Hold)
        bool isTryingToSprint = false; 
        if (rollBufferTimer > 0) { rollBufferTimer -= delta; } 

        bool sprintHeld = inputHandler.sprintInput; 
        bool sprintReleased = inputHandler.sprintInputReleased; 

        if (sprintHeld)
        {
            if (!isSprintButtonHeld) 
            {
                isSprintButtonHeld = true; 
                sprintInputTimer = 0f; 
            }
            sprintInputTimer += delta; 
            if (sprintInputTimer > tapRollThreshold)
            {
                isTryingToSprint = true; 
            }
        }
        if (sprintReleased)
        {
            if (isSprintButtonHeld && sprintInputTimer < tapRollThreshold)
            {
                rollBufferTimer = rollHandler.rollBufferTime; 
            }
            isSprintButtonHeld = false;
            sprintInputTimer = 0f;
        }
        
        // (โค้ดที่เหลือ... เหมือนเดิม)
        isRolling = rollHandler.isRolling; 
        
        if (inputHandler.jumpInput && isGrounded && !isRolling && !isLanding && jumpCooldownTimer <= 0) 
        {
            if (stats.currentStamina >= jumpStaminaCost) 
            {
                stats.currentStamina -= jumpStaminaCost; 
                stats.UpdateStaminaBar();
                movement.HandleJump(); 
                jumpCooldownTimer = jumpCooldown; 
            }
        }
        
        movement.HandleStamina(delta, isTryingToSprint, isRolling, inputHandler.moveInput.magnitude);
        isSprinting = isTryingToSprint && stats.currentStamina > 0; 

        if (rollBufferTimer > 0 && !isRolling && !isLanding) 
        {
            rollBufferTimer = 0; 
            rollHandler.TryRoll(); 
        }
        
        isRolling = rollHandler.isRolling; 

        if (isRolling || isLanding) return;
        
        bool isLockOnSprinting = (lockedTarget != null && isSprinting && inputHandler.moveInput.magnitude > 0.1f);

        lockedTarget = lockOn.HandleLockOn(delta, inputHandler.moveInput, isRolling, isLockOnSprinting);
        animHandler.SetLockedOn(lockedTarget != null);
        animHandler.SetSprinting(isSprinting); 

        movement.HandleMovement(delta, inputHandler.moveInput, isSprinting, lockedTarget, cameraMainTransform, isLockOnSprinting); 
        
        animHandler.UpdateMovementParameters(inputHandler.moveInput, isSprinting, lockedTarget, isLockOnSprinting);
    }

    private void FixedUpdate()
    {
        movement.HandleGravity();
    }
}