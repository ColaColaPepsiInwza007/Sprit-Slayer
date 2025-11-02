using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerManager manager; 
    private Vector3 playerVelocity;
    
    // (*** โค้ดใหม่ 1/4: ตัวแปร "จำ" โมเมนตัม ***)
    private Vector3 airMomentum; // (ตัวเก็บโมเมนตัมแนวนอน)

    [Header("Movement Settings")]
    [SerializeField] private float playerSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravityValue = -9.81f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.8f; 
    [SerializeField] private float groundCheckStickForce = -2f; 
    
    // (*** โค้ดใหม่ 2/4: เราลบ "airControlMultiplier" ทิ้งไปเลย ***)
    // [SerializeField] private float airControlMultiplier = 0.5f; // (ลบทิ้ง!)

    [Header("Stamina Settings")]
    [SerializeField] private float staminaDepleteRate = 15f; 
    [SerializeField] private float staminaRegenRate = 20f;   
    [SerializeField] private float staminaRegenDelay = 1.5f; 
    private float timeSinceLastSprint = 0f; 

    private void Awake()
    {
        manager = GetComponent<PlayerManager>();
        airMomentum = Vector3.zero; // (*** โค้ดใหม่ 3/4: รีเซ็ตค่าเริ่มต้น ***)
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
        // (ฟังก์ชันนี้เหมือนเดิมเป๊ะ! ... `airMomentum` จะถูกเซ็ตจาก HandleMovement)
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
    
    // (*** โค้ดอัปเกรด (V.10 - Committed Jumps) ***)
    public void HandleMovement(float delta, Vector2 moveInput, bool isSprinting, Transform lockedTarget, Transform cameraMainTransform, bool isLockOnSprinting)
    {
        float moveAmount = moveInput.magnitude;
        Vector3 moveDirection = (cameraMainTransform.forward * moveInput.y) + (cameraMainTransform.right * moveInput.x);
        moveDirection.y = 0;
        
        // (*** โค้ดใหม่ 4/4: แก้ไข Logic ทั้งหมดในนี้ ***)
        
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
                
                // (อัปเดต "โมเมนตัม" ตลอดเวลาที่เดินบนพื้น)
                // (นี่คือค่าที่เราจะ "จำ" ไว้ใช้ตอนกระโดด)
                airMomentum = moveDirection.normalized * currentSpeed;
                
                // (ใช้โมเมนตัมนี้เคลื่อนที่)
                manager.controller.Move(airMomentum * delta);
                
                // (Logic การหมุนตัว... เหมือนเดิม)
                if (lockedTarget != null && !isLockOnSprinting)
                {
                    // (LockOn)
                }
                else
                {
                    // (FreeLook / LockOn Sprint)
                    manager.movement.HandleFreeLookRotation(moveDirection.normalized, delta);
                }
            }
            else
            {
                // (ถ้าหยุดเดินบนพื้น... ก็ต้องรีเซ็ตโมเมนตัม)
                // (กันบั๊ก "ยืนโดด" แล้วตัวพุ่ง)
                airMomentum = Vector3.zero;
            }
        }
        else
        {
            // === 2. อยู่กลางอากาศ (Committed Jump) ===
            // (เราไม่ "อ่าน Input" ใหม่)
            // (เราใช้ `airMomentum` ที่ "จำ" ไว้จากตอนที่อยู่บนพื้น)
            // (HandleGravity() จะจัดการเรื่องดิ่งลงเอง)
            manager.controller.Move(airMomentum * delta);
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
}