using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private PlayerManager manager;
    
    private void Awake()
    {
        manager = GetComponent<PlayerManager>();
    }

    public void UpdateMovementParameters(Vector2 moveInput, bool isSprinting, Transform lockedTarget, bool isLockOnSprinting)
    {
        if (lockedTarget != null && !isLockOnSprinting)
        {
            manager.animator.SetFloat("MoveY", moveInput.y, 0.1f, Time.deltaTime);
            manager.animator.SetFloat("MoveX", moveInput.x, 0.1f, Time.deltaTime);
        }
        else
        {
            float moveAmount = moveInput.magnitude;
            float targetAnimValue;
            if (isSprinting && moveAmount > 0.1f)
            {
                targetAnimValue = 2f; 
            }
            else
            {
                targetAnimValue = moveAmount; 
            }
            manager.animator.SetFloat("MoveY", targetAnimValue, 0.1f, Time.deltaTime); 
            manager.animator.SetFloat("MoveX", 0, 0.1f, Time.deltaTime); 
        }
    }

    public void SetLockedOn(bool isLocked)
    {
        manager.animator.SetBool("IsLockedOn", isLocked);
    }

    public void SetSprinting(bool isSprinting)
    {
        manager.animator.SetBool("IsSprinting", isSprinting);
    }

    public void TriggerRoll()
    {
        manager.animator.SetTrigger("Roll");
    }
    
    public void TriggerJump()
    {
        manager.animator.SetTrigger("Jump");
    }
    
    public void SetGrounded(bool isGrounded)
    {
        manager.animator.SetBool("IsGrounded", isGrounded);
    }

    // (*** ฟังก์ชันใหม่ 2 อัน (สำหรับ "กันสไลด์") ***)
    public void StartLanding()
    {
        manager.isLanding = true;
    }

    public void FinishLanding()
    {
        manager.isLanding = false;
    }
}