using UnityEngine;
using UnityEngine.InputSystem;

// (*** 🚀 ไฟล์อัปเดต! (ย้าย Logic 'Input Buffering' มาไว้ที่นี่) 🚀 ***)

public class PlayerInputHandler : MonoBehaviour
{
    public PlayerControls playerControls;
    
    [Header("Raw Inputs")]
    public Vector2 moveInput { get; private set; }
    public bool sprintInput { get; private set; } 
    public bool sprintInputReleased { get; private set; } 
    public bool jumpInput { get; private set; } 
    public bool drawWeaponInput { get; private set; }
    public bool toggleMouseInput { get; private set; } 
    public bool attackInput { get; private set; } 

    // (*** ❗️❗️❗️ 1. "ย้าย" ตัวแปรมาจาก PlayerManager ❗️❗️❗️ ***)
    [Header("Input Buffering")]
    [SerializeField] private float attackBufferTime = 0.2f;
    [SerializeField] private float rollBufferTime = 0.2f; 
    [SerializeField] private float tapRollThreshold = 0.2f; 

    // (*** ❗️❗️❗️ 2. "ย้าย" Timer มาไว้ที่นี่ ❗️❗️❗️ ***)
    public float attackBufferTimer { get; private set; }
    public float rollBufferTimer { get; private set; }
    private float sprintInputTimer = 0f; 
    private bool isSprintButtonHeld = false; 

    
    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
        
        PlayerManager manager = GetComponent<PlayerManager>();
        if (manager != null)
        {
            playerControls.Player.LockOn.performed += _ => manager.lockOn.TryToggleLockOn();
        }
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        // (*** 1. อ่าน Input ดิบ (เหมือนเดิม) ***)
        moveInput = playerControls.Player.Move.ReadValue<Vector2>();
        sprintInput = playerControls.Player.Sprint.IsPressed();
        sprintInputReleased = playerControls.Player.Sprint.WasReleasedThisFrame();
        jumpInput = playerControls.Player.Jump.WasPressedThisFrame(); 
        drawWeaponInput = playerControls.Player.DrawWeapon.WasPressedThisFrame();
        toggleMouseInput = playerControls.Player.ToggleMouse.WasPressedThisFrame(); 
        attackInput = playerControls.Player.Attack.WasPressedThisFrame(); 

        // (*** ❗️❗️❗️ 3. "ย้าย" Logic การบัฟเฟอร์มาไว้ที่นี่ ❗️❗️❗️ ***)
        
        // (นับถอยหลัง Timer)
        if (attackBufferTimer > 0) { attackBufferTimer -= delta; }
        if (rollBufferTimer > 0) { rollBufferTimer -= delta; } 

        // (เช็ค Attack Buffer)
        if (attackInput) { attackBufferTimer = attackBufferTime; }

        // (เช็ค Sprint-to-Roll Buffer)
        if (sprintInput){
            if (!isSprintButtonHeld) { isSprintButtonHeld = true; sprintInputTimer = 0f; }
            sprintInputTimer += delta; 
        }
        if (sprintInputReleased){
            if (isSprintButtonHeld && sprintInputTimer < tapRollThreshold) { rollBufferTimer = rollBufferTime; } 
            isSprintButtonHeld = false; sprintInputTimer = 0f;
        }
    }
    
    // (*** ❗️❗️❗️ 4. "เพิ่ม" ฟังก์ชัน "เคลียร์" บัฟเฟอร์ ❗️❗️❗️ ***)
    public void ConsumeAttackBuffer()
    {
        attackBufferTimer = 0f;
    }

    public void ConsumeRollBuffer()
    {
        rollBufferTimer = 0f;
    }
}