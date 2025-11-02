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

    // (*** โค้ดใหม่: อ้างอิงแบบแยกส่วน ***)
    [Header("Weapon Sockets")]
    public GameObject weaponInHand;     // (ลาก "ดาบในมือ" (ลูกของ Weapon_Socket) มาใส่)
    public GameObject swordInScabbard;  // (ลาก "ดาบในปลอก" (ลูกของ ปลอกดาบ) มาใส่)

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
    public bool isWeaponDrawn = false; // <--- (โค้ดใหม่: สถานะชักดาบ)
    public bool isSprinting; 
    public bool isRolling;
    public bool isGrounded;
    public Transform lockedTarget; 

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

        if (Camera.main != null)
        {
            cameraMainTransform = Camera.main.transform;
        }

        groundCheckOffset = new Vector3(0, controller.center.y, 0); 
        animator.applyRootMotion = false; 
        
        // (*** โค้ดใหม่: เซ็ตสถานะเริ่มต้น ***)
        // (เราจะใช้ Logic ที่ถูกต้องสำหรับโมเดลที่แยกส่วน)
        weaponInHand.SetActive(false);
        swordInScabbard.SetActive(true);
        isWeaponDrawn = false;
    }
    
    // (*** โค้ดใหม่: ฟังก์ชันสลับดาบ ***)
    public void ToggleWeapon()
    {
        // (กันไม่ให้ชักดาบตอนกลิ้ง/โดด/ตก)
        if (isRolling || isLanding || !isGrounded) return; 

        // (สลับค่า true/false)
        isWeaponDrawn = !isWeaponDrawn; 

        // (นี่คือ Logic ที่นายต้องการ!)
        weaponInHand.SetActive(isWeaponDrawn);        // ถ้าชักดาบ (true) -> โชว์ดาบที่มือ
        swordInScabbard.SetActive(!isWeaponDrawn);    // ถ้าชักดาบ (true) -> ซ่อนดาบในปลอก
        
        // (*** โค้ดสำหรับ Part 2 (ที่นายข้าม) และ Part 3 ***)
        // (เดี๋ยวเราจะกลับมาเปิดใช้บรรทัดนี้)
        // animHandler.SetArmed(isWeaponDrawn); 
    }

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

    // (*** โค้ดใหม่: เพิ่ม 1 บรรทัดใน Update() ***)
    private void Update()
    {
        float delta = Time.deltaTime;

        HandleGroundCheck(); 
        animHandler.SetGrounded(isGrounded); 

        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= delta;
        }
        
        // (*** โค้ดใหม่: เพิ่มการเช็คปุ่มชักดาบ ***)
        if (inputHandler.drawWeaponInput)
        {
            ToggleWeapon();
        }

        // (โค้ดที่เหลือเหมือนเดิมเป๊ะ)
        bool isTryingToSprint = inputHandler.isSprinting; 
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
}