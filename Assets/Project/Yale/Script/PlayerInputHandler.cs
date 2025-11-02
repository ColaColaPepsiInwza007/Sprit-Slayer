using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public PlayerControls playerControls;
    public Vector2 moveInput { get; private set; }
    
    // (*** โค้ดใหม่ 1/3: เปลี่ยนชื่อ isSprinting -> sprintInput ***)
    public bool sprintInput { get; private set; } // (นี่คือ IsPressed())
    
    // (*** โค้ดใหม่ 2/3: เพิ่มตัวเช็ค "ปล่อย" ***)
    public bool sprintInputReleased { get; private set; } // (นี่คือ WasReleasedThisFrame())
    
    // (*** เราลบ rollInput ทิ้งไปเลย! ***)
    // public bool rollInput { get; private set; } 
    
    public bool jumpInput { get; private set; } 
    public bool drawWeaponInput { get; private set; }
    public bool toggleMouseInput { get; private set; } 

    private PlayerManager manager;

    private void Awake()
    {
        manager = GetComponent<PlayerManager>();
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
        playerControls.Player.LockOn.performed += _ => manager.lockOn.TryToggleLockOn();
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();
        playerControls.Player.LockOn.performed -= _ => manager.lockOn.TryToggleLockOn();
    }

    // (*** โค้ดใหม่ 3/3: อัปเดต Update() ***)
    private void Update()
    {
        moveInput = playerControls.Player.Move.ReadValue<Vector2>();
        
        // (อ่านค่า Sprint 2 แบบ)
        sprintInput = playerControls.Player.Sprint.IsPressed();
        sprintInputReleased = playerControls.Player.Sprint.WasReleasedThisFrame();
        
        // (ลบ rollInput ทิ้ง)
        // rollInput = playerControls.Player.Roll.WasPressedThisFrame();
        
        jumpInput = playerControls.Player.Jump.WasPressedThisFrame(); 
        drawWeaponInput = playerControls.Player.DrawWeapon.WasPressedThisFrame();
        toggleMouseInput = playerControls.Player.ToggleMouse.WasPressedThisFrame(); 
    }
}