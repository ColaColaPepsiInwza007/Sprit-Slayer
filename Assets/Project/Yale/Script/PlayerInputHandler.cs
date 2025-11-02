using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public PlayerControls playerControls;
    public Vector2 moveInput { get; private set; }
    public bool isSprinting { get; private set; }
    public bool rollInput { get; private set; } 
    public bool jumpInput { get; private set; } 
    public bool drawWeaponInput { get; private set; }
    
    // (*** โค้ดใหม่ 1/2 ***)
    public bool toggleMouseInput { get; private set; } // <--- เพิ่มตัวแปรปุ่มสลับเมาส์

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

    private void Update()
    {
        moveInput = playerControls.Player.Move.ReadValue<Vector2>();
        isSprinting = playerControls.Player.Sprint.IsPressed();
        rollInput = playerControls.Player.Roll.WasPressedThisFrame();
        jumpInput = playerControls.Player.Jump.WasPressedThisFrame(); 
        drawWeaponInput = playerControls.Player.DrawWeapon.WasPressedThisFrame();
        
        // (*** โค้ดใหม่ 2/2 ***)
        toggleMouseInput = playerControls.Player.ToggleMouse.WasPressedThisFrame(); // <--- อ่านค่าปุ่มสลับเมาส์
    }
}