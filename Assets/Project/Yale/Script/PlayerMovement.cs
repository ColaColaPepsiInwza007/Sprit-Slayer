using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerManager manager; 
    private Vector3 playerVelocity;

    [Header("Movement Settings")]
    [SerializeField] private float playerSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravityValue = -9.81f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.8f; 
    [SerializeField] private float groundCheckStickForce = -2f; 
    [SerializeField] private float airControlMultiplier = 0.5f; // <--- (โค้ดใหม่: คุมตัวกลางอากาศ 50%)

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

    // (*** โค้ดอัปเกรด (แก้บั๊กวิ่งฟรี) ***)
    // <--- (เปลี่ยน 'isSprinting' เป็น 'isTryingToSprint')
    public void HandleStamina(float delta, bool isTryingToSprint, bool isRolling, float moveAmount)
    {
        bool isMoving = moveAmount > 0.1f;

        // (เช็ค "ความตั้งใจ" ที่จะวิ่ง)
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
    
    // (*** โค้ดอัปเกรด (แก้บั๊ก Momentum + Air Control) ***)
    public void HandleMovement(float delta, Vector2 moveInput, bool isSprinting, Transform lockedTarget, Transform cameraMainTransform, bool isLockOnSprinting)
    {
        float moveAmount = moveInput.magnitude;
        Vector3 moveDirection = (cameraMainTransform.forward * moveInput.y) + (cameraMainTransform.right * moveInput.x);
        moveDirection.y = 0;
        
        // (*** โค้ดใหม่: แยก Logic "พื้น" กับ "อากาศ" ***)
        
        if (manager.isGrounded)
        {
            // === 1. อยู่บนพื้น ===
            if (moveAmount > 0.1f)
            {
                float currentSpeed = playerSpeed;
                
                // (เช็ค "สถานะวิ่งจริง" ... Manager คำนวณ Stamina มาให้แล้ว)
                if (isSprinting) 
                {
                    currentSpeed = sprintSpeed;
                }
                
                manager.controller.Move(moveDirection.normalized * currentSpeed * delta);
                
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
        }
        else
        {
            // === 2. อยู่กลางอากาศ (Air Control) ===
            // (เรายังขยับตัวได้ แต่ "ช้าลง" ... นี่คือ Momentum ที่หายไป)
            float airSpeed = playerSpeed * airControlMultiplier;
            manager.controller.Move(moveDirection.normalized * airSpeed * delta);
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