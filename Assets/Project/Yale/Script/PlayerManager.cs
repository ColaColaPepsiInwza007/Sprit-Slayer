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
    public bool isSprinting; // <--- (ค่า "ที่แท้จริง" จะถูกคำนวณใน Update)
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

    // (*** Update() ฉบับอัปเกรด (Final V.9) ***)
    private void Update()
    {
        float delta = Time.deltaTime;

        HandleGroundCheck(); 
        animHandler.SetGrounded(isGrounded); 

        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= delta;
        }

        // (*** โค้ดแก้บั๊ก "วิ่งฟรี" ***)
        bool isTryingToSprint = inputHandler.isSprinting; // <--- 1. อ่าน "ความตั้งใจ" (ปุ่ม)
        isRolling = rollHandler.isRolling; 
        
        // (*** โค้ดแก้บั๊ก "กระโดด" ***)
        // (เรายังไม่เช็ค isGrounded... เพื่อให้โค้ดส่วน "Stamina" ทำงานกลางอากาศได้)
        
        // (เช็คอินพุต "กระโดด")
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

        // (Roll Buffer)
        if (inputHandler.rollInput) { rollBufferTimer = rollHandler.rollBufferTime; }
        else { if (rollBufferTimer > 0) { rollBufferTimer -= delta; } }
        
        // (*** โค้ดแก้บั๊ก "วิ่งฟรี" ***)
        // (ส่ง "ความตั้งใจ" (isTryingToSprint) ไปให้ "Stamina" จัดการ)
        movement.HandleStamina(delta, isTryingToSprint, isRolling, inputHandler.moveInput.magnitude);

        // (*** โค้ดแก้บั๊ก "วิ่งฟรี" ***)
        // (คำนวณ "สถานะวิ่งจริง" ... ต้อง "ตั้งใจ" + "มี Stamina")
        isSprinting = isTryingToSprint && stats.currentStamina > 0; // <--- 2. นี่คือ "สถานะวิ่งจริง"

        // (Roll Check)
        if (rollBufferTimer > 0 && !isRolling && !isLanding) 
        {
            rollBufferTimer = 0; 
            rollHandler.TryRoll(); 
        }
        
        isRolling = rollHandler.isRolling; 

        // (*** "กันสไลด์" ***)
        // (ถ้า "กลิ้ง" หรือ "ลงพื้น" ... ให้หยุด "โค้ดส่วนล่าง" ทันที)
        if (isRolling || isLanding) return;
        
        // (*** "บั๊ก Momentum" ถูกแก้แล้ว ***)
        // (เรา "ลบ" if (!isGrounded) return; ทิ้งไปแล้ว!)
        // (ดังนั้น... HandleMovement จะ "ทำงานกลางอากาศ" ได้!)

        // (โค้ดที่เหลือ... ส่ง "สถานะวิ่งจริง" (isSprinting) ไปให้ลูกน้อง)
        bool isLockOnSprinting = (lockedTarget != null && isSprinting && inputHandler.moveInput.magnitude > 0.1f);

        lockedTarget = lockOn.HandleLockOn(delta, inputHandler.moveInput, isRolling, isLockOnSprinting);
        animHandler.SetLockedOn(lockedTarget != null);
        animHandler.SetSprinting(isSprinting); // <--- (ใช้ "สถานะวิ่งจริง")

        movement.HandleMovement(delta, inputHandler.moveInput, isSprinting, lockedTarget, cameraMainTransform, isLockOnSprinting); // <--- (ใช้ "สถานะวิ่งจริง")
        
        animHandler.UpdateMovementParameters(inputHandler.moveInput, isSprinting, lockedTarget, isLockOnSprinting); // <--- (ใช้ "สถานะวิ่งจริง")
    }

    private void FixedUpdate()
    {
        movement.HandleGravity();
    }
}