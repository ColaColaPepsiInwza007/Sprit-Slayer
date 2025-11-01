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
    public bool isSprinting;
    public bool isRolling;
    public bool isGrounded; // <--- (ตัวแปรนี้... จะถูกควบคุมโดยโค้ดใหม่ของเรา)
    public Transform lockedTarget; 

    [Header("Camera")]
    public Transform cameraMainTransform;

    // (*** โค้ดใหม่: สำหรับ Ground Check ***)
    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayer; // <--- (ต้องไปตั้งค่าใน Inspector)
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    private Vector3 groundCheckOffset; // (จุดที่ยิง SphereCast)

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

        // (*** โค้ดใหม่: คำนวณจุดยิง SphereCast ***)
        // (ยิงจาก "จุดกึ่งกลาง" ของ Controller ลงไป)
        groundCheckOffset = new Vector3(0, controller.center.y, 0); 
        
        // (*** โค้ดที่เติม (แก้บั๊กวิ่งค้าง) ***)
        animator.applyRootMotion = false; // <--- 1. ปิด Root Motion เป็นค่าเริ่มต้น
    }
    
    // (*** ฟังก์ชันใหม่: ตัวเช็คพื้น V.2 ***)
    private void HandleGroundCheck()
    {
        // 1. ตำแหน่งที่จะยิง SphereCast (อัปเดตตามตัวละคร)
        Vector3 spherePosition = transform.position + groundCheckOffset;

        // 2. ยิง SphereCast (ลูกบอลที่มองไม่เห็น) ลงไปที่พื้น
        // (ยิงจาก 'spherePosition', รัศมี 'groundCheckRadius', ทิศทาง 'Vector3.down', 
        //  ระยะทาง 'groundCheckDistance', เช็คเฉพาะ Layer 'groundLayer')
        if (Physics.SphereCast(spherePosition, groundCheckRadius, Vector3.down, 
                               out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            isGrounded = true; // (เจอดิน!)
        }
        else
        {
            isGrounded = false; // (ลอยอยู่)
        }
    }


    private void Update()
    {
        float delta = Time.deltaTime;

        // (*** โค้ดใหม่: เรียกใช้ตัวเช็คพื้น "ก่อน" ***)
        HandleGroundCheck(); // <--- (เรียกใช้ "ก่อน" ที่จะทำอย่างอื่น)

        // อ่าน Input (เหมือนเดิม)
        isSprinting = inputHandler.isSprinting;
        isRolling = rollHandler.isRolling; 
        // isGrounded = controller.isGrounded; // <--- (ลบบรรทัดนี้ทิ้ง! เราไม่ใช้แล้ว!)

        // (โค้ดบัฟเฟอร์... เหมือนเดิม)
        if (inputHandler.rollInput)
        {
            rollBufferTimer = rollHandler.rollBufferTime; 
        }
        else
        {
            if (rollBufferTimer > 0)
            {
                rollBufferTimer -= delta;
            }
        }
        
        // (โค้ด Stamina... เหมือนเดิม)
        movement.HandleStamina(delta, isSprinting, isRolling, inputHandler.moveInput.magnitude);

        // (โค้ดเช็คบัฟเฟอร์... เหมือนเดิม)
        if (rollBufferTimer > 0 && !isRolling)
        {
            rollBufferTimer = 0; 
            rollHandler.TryRoll(); // (คราวนี้... isGrounded จะ "เสถียร" แล้ว!)
        }
        
        isRolling = rollHandler.isRolling; 

        if (isRolling) return;

        // (โค้ดที่เหลือ... เหมือนเดิม)
        bool isLockOnSprinting = (lockedTarget != null && isSprinting && inputHandler.moveInput.magnitude > 0.1f);

        lockedTarget = lockOn.HandleLockOn(delta, inputHandler.moveInput, isRolling, isLockOnSprinting);
        animHandler.SetLockedOn(lockedTarget != null);
        animHandler.SetSprinting(isSprinting); 

        movement.HandleMovement(delta, inputHandler.moveInput, isSprinting, lockedTarget, cameraMainTransform, isLockOnSprinting);
        
        animHandler.UpdateMovementParameters(inputHandler.moveInput, isSprinting, lockedTarget, isLockOnSprinting);
    }

    private void FixedUpdate()
    {
        // (โค้ด Gravity... เหมือนเดิม)
        // (แต่คราวนี้มันจะ "เสถียร" ขึ้นด้วย เพราะ isGrounded ของเรานิ่งกว่า)
        movement.HandleGravity();
    }
}