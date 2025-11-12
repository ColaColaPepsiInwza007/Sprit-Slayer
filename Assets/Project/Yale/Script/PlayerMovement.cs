using UnityEngine;

// (*** 🚀 PlayerMovement (ตัวใหม่) ... อัปเดตแก้บั๊ก "ติดอยู่กับที่" 🚀 ***)

public class PlayerMovement : MonoBehaviour
{
    private PlayerManager manager; 
    private Vector3 playerVelocity;
    private Vector3 airMomentum; 
    public Vector2 MoveInput { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float playerSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravityValue = -9.81f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.8f; 
    [SerializeField] private float groundCheckStickForce = -2f; 
    
    [Header("Root Motion Multipliers")]
    [SerializeField] private float rollDistanceMultiplier = 1.0f; 
    [SerializeField] private float attackDistanceMultiplier = 1.0f; 

    [Header("Stamina Settings")]
    [SerializeField] private float staminaDepleteRate = 15f; 
    [SerializeField] private float staminaRegenRate = 20f;   
    [SerializeField] private float staminaRegenDelay = 1.5f;
    private float timeSinceLastSprint = 0f;

    private void Awake()
    {
        manager = GetComponent<PlayerManager>();
        airMomentum = Vector3.zero; 
        MoveInput = Vector2.zero; 
    }

    private void OnAnimatorMove()
    {
        if (manager.animator.applyRootMotion)
        {
            Vector3 delta = manager.animator.deltaPosition;
            playerVelocity.x = 0;
            playerVelocity.z = 0;

            if (manager.isRolling) { delta.x *= rollDistanceMultiplier; delta.z *= rollDistanceMultiplier; }
            else if (manager.isAttacking) { delta.x *= attackDistanceMultiplier; delta.z *= attackDistanceMultiplier; }
            
            delta.y = playerVelocity.y * Time.deltaTime; 
            manager.controller.Move(delta);
        }
    }

    public void ClearHorizontalVelocity()
    {
        playerVelocity.x = 0;
        playerVelocity.z = 0;
    }
    
    // (*** 🚀 FIX 1 (Stuck): "เพิ่ม" ฟังก์ชัน "สั่งขยับ" 🚀 ***)
    public void ApplyVelocity()
    {
        // (*** (State 'Idle' กับ 'Move' จะเรียกใช้ฟังก์ชันนี้... 'Roll'/'Attack' จะไม่เรียก) ***)
        manager.controller.Move(playerVelocity * Time.deltaTime);
    }

    public void SetMoveInput(Vector2 input)
    {
        MoveInput = input;
    }

    // (*** 🚀 FIX 1 (Stuck): "ลบ" controller.Move() ออกจาก Gravity! 🚀 ***)
    public void HandleGravity()
    {
        if (manager.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = groundCheckStickForce; 
        }
        playerVelocity.y += gravityValue * Time.deltaTime;
        
        // (*** (ลบ 'if (!applyRootMotion)' ... ทิ้ง!) ***)
    }
    
    public void HandleJump()
    {
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        manager.animHandler.TriggerJump();
    }

    public void HandleStamina(float delta, bool isTryingToSprint, bool isRolling, float moveAmount)
    {
        // (*** 🚀 FIX: (ย้าย Logic Stamina Drain มาไว้ที่นี่) 🚀 ***)
        bool isMoving = moveAmount > 0.1f;
        if (isTryingToSprint && manager.stats.currentStamina > 0 && isMoving && !isRolling) 
        {
            if (manager.stats.HasEnoughStamina(staminaDepleteRate * delta))
            {
                manager.stats.UseStamina(staminaDepleteRate * delta);
            }
        }
    }
    
    public void HandleMovement(float delta, Vector2 moveInput, bool isSprinting, Transform lockedTarget, Transform cameraMainTransform, bool isLockOnSprinting)
    {
        float moveAmount = moveInput.magnitude;
        Vector3 moveDirection = (cameraMainTransform.forward * moveInput.y) + (cameraMainTransform.right * moveInput.x);
        moveDirection.y = 0;
        
        if (manager.isGrounded) {
            if (moveAmount > 0.1f) {
                float currentSpeed = playerSpeed;
                if (isSprinting) { currentSpeed = sprintSpeed; }
                airMomentum = moveDirection.normalized * currentSpeed;
                
                playerVelocity.x = airMomentum.x;
                playerVelocity.z = airMomentum.z;

                if (lockedTarget != null && !isLockOnSprinting) { }
                else { manager.movement.HandleFreeLookRotation(moveDirection.normalized, delta); }
            } else {
                airMomentum = Vector3.zero;
                playerVelocity.x = 0;
                playerVelocity.z = 0;
            }
        } else {
            playerVelocity.x = airMomentum.x;
            playerVelocity.z = airMomentum.z;

            if (moveAmount > 0.1f) {
                if (lockedTarget != null && !isLockOnSprinting) { }
                else { manager.movement.HandleFreeLookRotation(moveDirection.normalized, delta); }
            }
        }
        
        // (*** 🚀 FIX 1 (Stuck): "ย้าย" controller.Move() มาไว้ที่นี่! 🚀 ***)
        // (*** ('HandleMovement' จะ "คำนวณ" X/Z... แล้ว 'ApplyVelocity' จะ "ขยับ") ***)
        ApplyVelocity();
    }
    
    public void HandleFreeLookRotation(Vector3 moveDirection, float delta)
    {
        if (moveDirection.sqrMagnitude > 0.001f) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * delta);
        }
    }
    public void HandleLockOnRotation(Vector3 targetDirection, float delta)
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * delta);
    }
}