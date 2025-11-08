using UnityEngine;
using UnityEngine.InputSystem; // ✅ สำหรับระบบ Input System ใหม่


public class PlayerMovement : MonoBehaviour
{
    private PlayerManager manager; 
    private Vector3 playerVelocity;
    
    private Vector3 airMomentum; // (ตัวเก็บโมเมนตัมแนวนอน)

    [Header("Movement Settings")]
    [SerializeField] private float playerSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravityValue = -9.81f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.8f; 
    [SerializeField] private float groundCheckStickForce = -2f; 
    
    [Header("Stamina Settings")]
    [SerializeField] private float staminaDepleteRate = 15f; 
    [SerializeField] private float staminaRegenRate = 20f;   
    [SerializeField] private float staminaRegenDelay = 1.5f;
    private float timeSinceLastSprint = 0f;
    public Vector2 MoveInput { get; private set; }

    private void Awake()
    {
        manager = GetComponent<PlayerManager>();
        airMomentum = Vector3.zero; 
    }

    public void HandleGravity()
    {
        if (manager.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = groundCheckStickForce; 
        }
        
        playerVelocity.y += gravityValue * Time.deltaTime;
        manager.controller.Move(playerVelocity * Time.deltaTime);
    }
    
    public void HandleJump()
    {
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        manager.animHandler.TriggerJump();
    }

    // (HandleStamina... เหมือนเดิม)
    public void HandleStamina(float delta, bool isTryingToSprint, bool isRolling, float moveAmount)
    {
        // (โค้ดเหมือนเดิม)
        bool isMoving = moveAmount > 0.1f;
        if (isTryingToSprint && manager.stats.currentStamina > 0 && isMoving && !isRolling) 
        {
            manager.stats.currentStamina -= staminaDepleteRate * delta; 
            manager.stats.currentStamina = Mathf.Max(manager.stats.currentStamina, 0); 
            timeSinceLastSprint = 0f; 
        }
        else
        {
            if (timeSinceLastSprint >= staminaRegenDelay && !isRolling) 
            {
                if (manager.stats.currentStamina < manager.stats.maxStamina)
                {
                    manager.stats.currentStamina += staminaRegenRate * delta; 
                    manager.stats.currentStamina = Mathf.Min(manager.stats.currentStamina, manager.stats.maxStamina); 
                }
            }
            else { timeSinceLastSprint += delta; }
        }
        manager.stats.UpdateStaminaBar();
    }
    
    // (*** โค้ดอัปเกรด (V.11 - Air Rotation) ***)
    public void HandleMovement(float delta, Vector2 moveInput, bool isSprinting, Transform lockedTarget, Transform cameraMainTransform, bool isLockOnSprinting)
    {
        float moveAmount = moveInput.magnitude;
        Vector3 moveDirection = (cameraMainTransform.forward * moveInput.y) + (cameraMainTransform.right * moveInput.x);
        moveDirection.y = 0;
        
        if (manager.isGrounded)
        {
            // === 1. อยู่บนพื้น ===
            if (moveAmount > 0.1f)
            {
                float currentSpeed = playerSpeed;
                
                if (isSprinting) 
                {
                    currentSpeed = sprintSpeed;
                }
                
                // (อัปเดต "โมเมนตัม")
                airMomentum = moveDirection.normalized * currentSpeed;
                
                // (เคลื่อนที่)
                manager.controller.Move(airMomentum * delta);
                
                // (Logic การหมุนตัว... บนพื้น)
                if (lockedTarget != null && !isLockOnSprinting)
                {
                    // (ไม่ต้องทำอะไร... PlayerLockOn.cs จัดการให้)
                }
                else
                {
                    // (FreeLook / LockOn Sprint บนพื้น)
                    manager.movement.HandleFreeLookRotation(moveDirection.normalized, delta);
                }
            }
            else
            {
                // (รีเซ็ตโมเมนตัม (กันโดดยืนแล้วพุ่ง))
                airMomentum = Vector3.zero;
            }
        }
        else
        {
            // === 2. อยู่กลางอากาศ (Committed Jump + Air Rotation) ===
            
            // (1. Movement: ใช้โมเมนตัมที่ "จำ" ไว้)
            manager.controller.Move(airMomentum * delta);
            
            // (*** โค้ดใหม่: เพิ่ม Logic การหมุนตัวกลางอากาศ ***)
            if (moveAmount > 0.1f) // (เช็คว่ากดปุ่ม W,A,S,D)
            {
                if (lockedTarget != null && !isLockOnSprinting)
                {
                    // (ไม่ต้องทำอะไร... PlayerLockOn.cs จัดการให้)
                    // (เพราะ PlayerManager เรียก HandleLockOn() กลางอากาศอยู่แล้ว)
                }
                else
                {
                    // (FreeLook / LockOn Sprint... กลางอากาศ)
                    // (นี่คือสิ่งที่นายขอนั่นเอง!)
                    manager.movement.HandleFreeLookRotation(moveDirection.normalized, delta);
                }
            }
        }
    }

    // (HandleFreeLookRotation... เหมือนเดิม)
    public void HandleFreeLookRotation(Vector3 moveDirection, float delta)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * delta);
    }

    // (HandleLockOnRotation... เหมือนเดิม)
    public void HandleLockOnRotation(Vector3 targetDirection, float delta)
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * delta);
    }
    // ✅ เพิ่มฟังก์ชันนี้เข้าไปแทน
private void Update()
{
    float horizontal = 0f;
    float vertical = 0f;

    // ✅ ใช้ระบบ Input System ใหม่ (Keyboard)
    if (Keyboard.current != null)
    {
        if (Keyboard.current.aKey.isPressed) horizontal = -1f;
        else if (Keyboard.current.dKey.isPressed) horizontal = 1f;

        if (Keyboard.current.wKey.isPressed) vertical = 1f;
        else if (Keyboard.current.sKey.isPressed) vertical = -1f;
    }

    MoveInput = new Vector2(horizontal, vertical);
}

}