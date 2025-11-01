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

        // (*** โค้ด Logic V.5 (Omnidirectional Dodge) ***)
        // 1. ถ้า "กด" ทิศทาง (W,A,S,D)
        if (moveAmount > 0.1f)
        {
            // === กลิ้งตามทิศ (Camera-Relative) ===
            rollDirection = (manager.cameraMainTransform.forward * moveInput.y) + (manager.cameraMainTransform.right * moveInput.x);
        }
        else
        {
            // === กลิ้งแบบไม่กดทิศ (Neutral Dodge) ===
            if (manager.lockedTarget != null)
            {
                rollDirection = -transform.forward; // (Backstep)
            }
            else
            {
                rollDirection = transform.forward; // (Forward Dodge)
            }
        }
            
        rollDirection.y = 0; 

        // (หมุนตัวไปทางที่จะกลิ้ง)
        if (rollDirection.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(rollDirection.normalized);
        }
        
        // (โค้ด Stamina... เหมือนเดิม)
        isRolling = true; 
        manager.stats.currentStamina -= rollCost; 
        manager.stats.UpdateStaminaBar();

        // (*** โค้ดแก้บั๊ก Root Motion ***)
        manager.animator.applyRootMotion = true; // <--- "เปิด" Root Motion (สำหรับกลิ้ง)

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
    
    // (*** โค้ด "พิสูจน์บั๊ก" ***)
    public void FinishRoll() 
    { 
        // (*** โค้ดพิสูจน์ ***)
        Debug.Log("!!! FINISH ROLL (ปุ่มถูกกดแล้ว) !!!"); // <--- เพิ่มบรรทัดนี้

        isRolling = false; 
        
        // (*** โค้ดแก้บั๊ก Root Motion ***)
        manager.animator.applyRootMotion = false; // "ปิด" Root Motion

        manager.lockOn.SetRollDamping(false);
    }
}