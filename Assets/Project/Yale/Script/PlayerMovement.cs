using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerManager manager; // หัวหน้า
    private Vector3 playerVelocity;

    [Header("Movement Settings")]
    [SerializeField] private float playerSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravityValue = -9.81f;

    [Header("Stamina Settings")]
    [SerializeField] private float staminaDepleteRate = 15f; 
    [SerializeField] private float staminaRegenRate = 20f;   
    [SerializeField] private float staminaRegenDelay = 1.5f; 
    private float timeSinceLastSprint = 0f; 

    private void Awake()
    {
        manager = GetComponent<PlayerManager>();
    }

    public void HandleGravity()
    {
        if (manager.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        playerVelocity.y += gravityValue * Time.deltaTime;
        manager.controller.Move(playerVelocity * Time.deltaTime);
    }

    public void HandleStamina(float delta, bool isSprinting, bool isRolling, float moveAmount)
    {
        bool isMoving = moveAmount > 0.1f;

        if (isSprinting && manager.stats.currentStamina > 0 && isMoving && !isRolling) 
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
    
    // (*** โค้ดที่อัปเกรด (Final V.6) ***)
    // (เปลี่ยน Logic การเดินทั้งหมด ให้ "อิงจากกล้อง" เสมอ!)
    public void HandleMovement(float delta, Vector2 moveInput, bool isSprinting, Transform lockedTarget, Transform cameraMainTransform, bool isLockOnSprinting)
    {
        // (*** Logic ใหม่: Camera-Relative "เสมอ" ***)
        // (ไม่ว่าคุณจะ LockOn หรือ FreeLook, การเดิน W/A/S/D จะอิงจากกล้องเสมอ)

        float moveAmount = moveInput.magnitude;
        
        // 1. (Logic เดียวกับ PlayerRoll V.5)
        Vector3 moveDirection = (cameraMainTransform.forward * moveInput.y) + (cameraMainTransform.right * moveInput.x);
        moveDirection.y = 0;
        
        if (moveAmount > 0.1f)
        {
            // 2. (ถ้า 'isSprinting' = true, มันจะวิ่งเร็วเอง)
            float currentSpeed = isSprinting ? sprintSpeed : playerSpeed;
            manager.controller.Move(moveDirection.normalized * currentSpeed * delta);
            
            // (*** Logic การ "หมุนตัว" ***)
            // (นี่คือ "หัวใจ" ที่แยก LockOn กับ FreeLook ออกจากกัน)

            if (lockedTarget != null && !isLockOnSprinting)
            {
                // ถ้า "ล็อคเป้า" (และไม่วิ่ง):
                // "ไม่ต้อง" หันตัวตามทิศที่เดิน
                // (เพราะ PlayerLockOn จะสั่งให้ "หันหน้าหาศัตรู" (HandleLockOnRotation) อยู่แล้ว)
            }
            else
            {
                // ถ้า "FreeLook" (หรือ วิ่งล็อคเป้า):
                // "หันตัว" ไปตามทิศที่เดิน (Logic เดิม)
                manager.movement.HandleFreeLookRotation(moveDirection.normalized, delta);
            }
        }
    }

    // ฟังก์ชันนี้ถูกเรียกโดย PlayerMovement (ในโหมด FreeLook / LockOnSprint)
    public void HandleFreeLookRotation(Vector3 moveDirection, float delta)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * delta);
    }

    // ฟังก์ชันนี้ถูกเรียกโดย PlayerLockOn (ในโหมด LockOn ปกติ)
    public void HandleLockOnRotation(Vector3 targetDirection, float delta)
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * delta);
    }
}