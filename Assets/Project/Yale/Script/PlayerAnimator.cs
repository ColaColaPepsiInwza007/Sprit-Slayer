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
        float moveY, moveX;
        
        if (lockedTarget == null || isLockOnSprinting)
        {
            manager.animator.SetBool("IsLockedOn", false);
            
            float moveAmount = moveInput.magnitude;
            float targetAnimValue;
            
            if (isSprinting && moveAmount > 0.1f) { targetAnimValue = 2f; }
            else { targetAnimValue = moveAmount; }
            
            moveY = targetAnimValue;
            moveX = 0;
        }
        else 
        {
            manager.animator.SetBool("IsLockedOn", true);
            moveY = moveInput.y;
            moveX = moveInput.x;
        }
        
        manager.animator.SetFloat("MoveY", moveY, 0.1f, Time.deltaTime);
        manager.animator.SetFloat("MoveX", moveX, 0.1f, Time.deltaTime);
    }

    public void SetRollDirection(Vector2 direction)
    {
        manager.animator.SetFloat("MoveY", direction.y, 0f, Time.deltaTime);
        manager.animator.SetFloat("MoveX", direction.x, 0f, Time.deltaTime);
    }
    
    // (*** (โค้ดที่เหลือเหมือนเดิม) ***)
    public void SetSprinting(bool isSprinting) { manager.animator.SetBool("IsSprinting", isSprinting); }
    public void TriggerRoll() { manager.animator.SetTrigger("Roll"); }
    public void TriggerJump() { manager.animator.SetTrigger("Jump"); }
    
    // (*** ❗️❗️❗️ แก้บั๊กบรรทัดข้างล่างนี้ (ลบ 60, 61) ❗️❗️❗️ ***)
    public void SetGrounded(bool isGrounded) { manager.animator.SetBool("IsGrounded", isGrounded); }
    
    public void StartLanding() { manager.isLanding = true; }
    public void FinishLanding() { manager.isLanding = false; }
    public void SetArmed(bool isArmed) { manager.animator.SetBool("IsArmed", isArmed); }
    public void TriggerAttack(int combo) { } 
}