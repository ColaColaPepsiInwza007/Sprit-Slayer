using UnityEngine;
using UnityEngine.InputSystem; // <--- (ต้องมี)

public class PlayerInputHandler : MonoBehaviour
{
    public PlayerControls playerControls;
    public Vector2 moveInput { get; private set; }
    public bool isSprinting { get; private set; }
    
    // (*** โค้ดใหม่ ***)
    public bool rollInput { get; private set; } // <--- เปลี่ยนจากอีเวนต์มาเป็นตัวแปร

    // References ไปยัง Manager (หัวหน้า)
    private PlayerManager manager;

    private void Awake()
    {
        manager = GetComponent<PlayerManager>();
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
        
        // เมื่อกดปุ่ม ให้ "แจ้ง" หัวหน้า
        playerControls.Player.LockOn.performed += _ => manager.lockOn.TryToggleLockOn();
        
        // (*** โค้ดที่ลบ ***)
        // (เราไม่ใช้อีเวนต์ของ Roll แล้ว)
        // playerControls.Player.Roll.performed += _ => manager.rollHandler.TryRoll(); 
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();
        playerControls.Player.LockOn.performed -= _ => manager.lockOn.TryToggleLockOn();

        // (*** โค้ดที่ลบ ***)
        // playerControls.Player.Roll.performed -= _ => manager.rollHandler.TryRoll();
    }

    private void Update()
    {
        // อ่านค่าเก็บไว้ให้ Manager ดึงไปใช้
        moveInput = playerControls.Player.Move.ReadValue<Vector2>();
        isSprinting = playerControls.Player.Sprint.IsPressed();

        // (*** โค้ดใหม่ ***)
        // (เช็ค "ทุกเฟรม" ว่าปุ่มกลิ้งเพิ่งถูก "กด" ในเฟรมนี้หรือไม่)
        rollInput = playerControls.Player.Roll.WasPressedThisFrame();
    }
}