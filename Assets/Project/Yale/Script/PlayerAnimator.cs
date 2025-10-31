using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private PlayerManager manager;

    // (ลบ [Header] ที่คุม Speed ทิ้งไปเลย)

    private void Awake()
    {
        manager = GetComponent<PlayerManager>();
    }

    // (อัปเดต V.7: ลบ SetFloat("AnimationSpeed") ทิ้ง)
    public void UpdateMovementParameters(Vector2 moveInput, bool isSprinting, Transform lockedTarget, bool isLockOnSprinting)
    {
        if (lockedTarget != null && !isLockOnSprinting)
        {
            // === โหมดล็อคเป้า (Locked-On) ===
            manager.animator.SetFloat("MoveY", moveInput.y, 0.1f, Time.deltaTime);
            manager.animator.SetFloat("MoveX", moveInput.x, 0.1f, Time.deltaTime);
            // (ลบ SetFloat("AnimationSpeed") ทิ้ง)
        }
        else
        {
            // === โหมดอิสระ (Free Look) หรือ โหมดวิ่งล็อคเป้า (LockOn Sprinting) ===
            float moveAmount = moveInput.magnitude;
            float targetAnimValue;
            if (isSprinting && moveAmount > 0.1f)
            {
                targetAnimValue = 2f; 
                // (ลบ SetFloat("AnimationSpeed") ทิ้ง)
            }
            else
            {
                targetAnimValue = moveAmount; 
                // (ลบ SetFloat("AnimationSpeed") ทิ้ง)
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
}