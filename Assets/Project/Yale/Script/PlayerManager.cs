using UnityEngine;

// (RequireComponent... เหมือนเดิม)
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

    // (*** โค้ดใหม่ 1/6 ***)
    [Header("Mouse Lock Settings")]
    public bool isMouseLocked = true; // <--- สถานะเมาส์

    [Header("Camera")]
    public Transform cameraMainTransform;

    // (Header... Ground Check, Stamina ... เหมือนเดิม)
    // ...
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

    private void Awake()
    {
        // (โค้ด Awake... เหมือนเดิม)
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();
        inputHandler = GetComponent<PlayerInputHandler>();
        movement = GetComponent<PlayerMovement>();
        animHandler = GetComponent<PlayerAnimator>();
        lockOn = GetComponent<PlayerLockOn>();
        rollHandler = GetComponent<PlayerRoll>();

        if (Camera.main != null)
        {
            cameraMainTransform = Camera.main.transform;
        }

        groundCheckOffset = new Vector3(0, controller.center.y, 0); 
        animator.applyRootMotion = false; 
        
        weaponInHand.SetActive(false);
        swordInScabbard.SetActive(true);
        isWeaponDrawn = false;
        
        // (*** โค้ดใหม่ 2/6 ***)
        LockMouse(); // <--- สั่งล็อคเมาส์ทันทีที่เกมเริ่ม
    }
    
    // (*** โค้ดใหม่ 3/6: ฟังก์ชันสลับเมาส์ ***)
    public void ToggleMouseLock()
    {
        isMouseLocked = !isMouseLocked; // (สลับค่า true/false)
        if (isMouseLocked)
        {
            LockMouse();
        }
        else
        {
            UnlockMouse();
        }
    }

    // (*** โค้ดใหม่ 4/6: ฟังก์ชันล็อคเมาส์ ***)
    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked; // <--- ล็อคเมาส์ให้อยู่กลางจอ
        Cursor.visible = false;                   // <--- ซ่อนเมาส์
        isMouseLocked = true;
    }

    // (*** โค้ดใหม่ 5/6: ฟังก์ชันปลดล็อคเมาส์ ***)
    private void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;   // <--- ปล่อยเมาส์เป็นอิสระ
        Cursor.visible = true;                    // <--- โชว์เมาส์
        isMouseLocked = false;
    }

    // (HandleGroundCheck... เหมือนเดิม)
    private void HandleGroundCheck()
    {
        //...
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

    private void Update()
    {
        float delta = Time.deltaTime;

        HandleGroundCheck(); 
        animHandler.SetGrounded(isGrounded); 

        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= delta;
        }
        
        // (*** โค้ดใหม่ 6/6: เพิ่มการเช็คปุ่มสลับเมาส์ ***)
        if (inputHandler.toggleMouseInput)
        {
            ToggleMouseLock();
        }

        if (inputHandler.drawWeaponInput)
        {
            ToggleWeapon();
        }

        // (โค้ดที่เหลือเหมือนเดิมเป๊ะ)
        // ... (Sprint, Roll, Jump, Stamina) ...
        // ...
        
        bool isTryingToSprint = inputHandler.isSprinting; 
        isRolling = rollHandler.isRolling; 
        
        if (inputHandler.jumpInput && isGrounded && !isRolling && !isLanding && jumpCooldownTimer <= 0) 
        {
            // (โค้ด Jump)
            if (stats.currentStamina >= jumpStaminaCost) 
            {
                stats.currentStamina -= jumpStaminaCost; 
                stats.UpdateStaminaBar();
                movement.HandleJump(); 
                jumpCooldownTimer = jumpCooldown; 
            }
        }

        if (inputHandler.rollInput) { rollBufferTimer = rollHandler.rollBufferTime; }
        else { if (rollBufferTimer > 0) { rollBufferTimer -= delta; } }
        
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
    
    // (ฟังก์ชัน ToggleWeapon... เหมือนเดิม)
    public void ToggleWeapon()
    {
        if (isRolling || isLanding || !isGrounded) return; 
        isWeaponDrawn = !isWeaponDrawn; 
        weaponInHand.SetActive(isWeaponDrawn);        
        swordInScabbard.SetActive(!isWeaponDrawn);    
        // animHandler.SetArmed(isWeaponDrawn); 
    }
}