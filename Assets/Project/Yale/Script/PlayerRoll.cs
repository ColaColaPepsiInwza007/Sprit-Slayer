using UnityEngine;

public class PlayerRoll : MonoBehaviour
{
    private PlayerManager manager;

    [Header("Rolling Settings")]
    [SerializeField] private float rollCost = 15f; 
    [SerializeField] private float rollDistanceMultiplier = 0.7f; 
    [SerializeField] public float rollBufferTime = 0.2f; // (บัฟเฟอร์อินพุต)

    public bool isRolling { get; private set; } 

    private void Awake()
    {
        manager = GetComponent<PlayerManager>();
        isRolling = false;
    }

    public void TryRoll()
    {
        // (เช็ค Stamina และ พื้น... เหมือนเดิม)
        if (isRolling || !manager.isGrounded) { return; } 
        
        if (manager.stats.currentStamina < rollCost) 
        {
            Debug.Log("Stamina ไม่พอ!");
            return; 
        }

        Vector2 moveInput = manager.inputHandler.moveInput; 
        float moveAmount = moveInput.magnitude;
        Vector3 rollDirection; // (สร้างตัวแปร)

        // (*** โค้ด Logic ใหม่ (Omnidirectional Dodge) ***)
        // ----------------------------------------------------

        // 1. ถ้า "กด" ทิศทาง (W,A,S,D)
        if (moveAmount > 0.1f)
        {
            // === กลิ้งตามทิศ (Camera-Relative) ===
            // (ใช้ Logic นี้ "เสมอ" ไม่ว่าจะล็อคเป้าหรือไม่)
            rollDirection = (manager.cameraMainTransform.forward * moveInput.y) + (manager.cameraMainTransform.right * moveInput.x);
        }
        else
        {
            // === กลิ้งแบบไม่กดทิศ (Neutral Dodge) ===
            // (ค่อยมาเช็คสถานะตัวละคร)
            if (manager.lockedTarget != null)
            {
                // ถ้าล็อคเป้าอยู่: ให้ "ถอยหลัง" (Backstep)
                rollDirection = -transform.forward; 
            }
            else
            {
                // ถ้าไม่ได้ล็อค: ให้ "พุ่งไปข้างหน้า" (Forward Dodge)
                rollDirection = transform.forward;
            }
        }
        // ----------------------------------------------------
            
        rollDirection.y = 0; 

        // (หมุนตัวไปทางที่จะกลิ้ง)
        if (rollDirection.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(rollDirection.normalized);
        }
        
        // (โค้ดที่เหลือ... เหมือนเดิม)
        isRolling = true; 
        manager.stats.currentStamina -= rollCost; 
        manager.stats.UpdateStaminaBar();

        // (สั่งให้กล้องหน่วง... (อันนี้ไม่ทำงานในโหมด "กล้อง FreeLook" แต่ใส่ไว้ก็ไม่เสียหาย))
        manager.lockOn.SetRollDamping(true);
        
        manager.animHandler.TriggerRoll(); 
    }

    // (OnAnimatorMove... เหมือนเดิม)
    private void OnAnimatorMove()
    {
        if (isRolling) 
        {
            Vector3 delta = manager.animator.deltaPosition * rollDistanceMultiplier;
            manager.controller.Move(delta);
        }
    }

    // (i-Frames... เหมือนเดิม)
    public void StartIFrames() { manager.stats.isInvincible = true; }
    public void EndIFrames() { manager.stats.isInvincible = false; }
    
    // (FinishRoll... เหมือนเดิม)
    public void FinishRoll() 
    { 
        isRolling = false; 
        manager.lockOn.SetRollDamping(false);
    }
}