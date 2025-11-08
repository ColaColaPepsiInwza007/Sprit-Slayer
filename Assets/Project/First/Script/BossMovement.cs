using UnityEngine;

public class BossMovement : MonoBehaviour
{
    private BossManager manager;
    private Vector3 bossVelocity;
    private float gravityValue = -9.81f;

    private float strafeTimer = 0f;
    private float maxStrafeTime = 15f; 
    private float lastStrafeDir = 0f;
    private PlayerMovement playerController;

    [Header("Bait Decision")]
    private float baitDecisionTimer = 0f;
    private float baitDecisionInterval = 3.0f; // (ตัวแปรเก่า)
    private bool isBaitStrafing = false;
    
    // --- ❗️❗️❗️ เพิ่ม 2 บรรทัดนี้ (สำหรับแก้ปัญหาที่ 2) ❗️❗️❗️ ---
    private float baitPatienceTimer = 0f; // ตัวนับเวลา "ความอดทน"
    private float baitMaxPatienceTime = 5.0f; // ❗️ 5 วินาทีในโหมด Bait แล้วจะเลิก
    // --- -------------------------------------------- ---

    private void Awake()
    {
        manager = GetComponent<BossManager>();

        if (manager.playerTarget != null)
            playerController = manager.playerTarget.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        HandleRotation(Time.deltaTime);

        if (manager.currentState == BossManager.BossState.Attack ||
            manager.currentState == BossManager.BossState.Stunned ||
            manager.currentState == BossManager.BossState.Dead ||
            manager.isPlayingAnimation ||
            manager.isRecoveringFromAttack)
        {
            bossVelocity = Vector3.zero;
            return;
        }

        if (manager.currentState == BossManager.BossState.Chase)
        {
            HandleChaseMovement(Time.deltaTime);
        }
        else if (manager.currentState == BossManager.BossState.Bait)
        {
            HandleBaitMovement(Time.deltaTime); 
        }
        else if (manager.currentState == BossManager.BossState.Reposition)
        {
            HandleRepositionMovement(Time.deltaTime);
        }
        else // ❗️ ถ้าหลุดจาก Bait (เช่นไป Chase, Attack)
        {
            baitDecisionTimer = 0f; 
            baitPatienceTimer = 0f; // ❗️ รีเซ็ตความอดทนด้วย
        }

        HandleGravity();
    }

    public void HandleRotation(float delta)
    {
        if (manager.playerTarget == null || manager.currentState == BossManager.BossState.Dead)
            return;

        // ให้หมุนตัวตอน Chase, Bait, หรือ Reposition
        bool canRotate = (manager.currentState == BossManager.BossState.Chase || 
                          manager.currentState == BossManager.BossState.Bait ||
                          manager.currentState == BossManager.BossState.Reposition);

        if (canRotate && !manager.isPlayingAnimation && !manager.isRecoveringFromAttack)
        {
            Vector3 targetDirection = manager.playerTarget.position - transform.position;
            targetDirection.y = 0;

            if (targetDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, manager.rotationSpeed * delta);
            }
        }
    }

    public void HandleGravity()
    {
        if (manager.controller.isGrounded && bossVelocity.y < 0)
        {
            bossVelocity.y = 0f;
        }

        bossVelocity.y += gravityValue * Time.deltaTime;
        manager.controller.Move(bossVelocity * Time.deltaTime);
    }

    private void HandleChaseMovement(float delta)
    {
        if (manager.playerTarget == null)
            return;

        Vector3 toPlayer = manager.playerTarget.position - transform.position;
        toPlayer.y = 0;
        float distance = toPlayer.magnitude;

        if (distance > manager.stoppingDistance)
        {
            MoveForward(delta);
            return;
        }

        if (distance <= manager.stoppingDistance)
        {
            manager.RequestAttack();
        }
    }

    private void MoveForward(float delta)
    {
        Vector3 move = transform.forward * manager.movementSpeed * delta;
        manager.controller.Move(move);
    }
    
    // ❗️ ฟังก์ชันสำหรับถอยคุมเชิง
    private void HandleRepositionMovement(float delta)
    {
        Vector3 backwardMove = -transform.forward * (manager.movementSpeed * 0.5f) * delta;
        if (lastStrafeDir == 0) lastStrafeDir = 1; 
        Vector3 sideMove = transform.right * lastStrafeDir * (manager.strafeSpeed * 0.5f) * delta;
        manager.controller.Move(backwardMove + sideMove);
    }

    // --- ❗️❗️❗️ แก้ไขฟังก์ชันนี้ทั้งหมด (แก้ทั้ง 2 ปัญหา) ❗️❗️❗️ ---
    private void HandleBaitMovement(float delta)
    {
        // --- 1. Logic "ความอดทน" (แก้ปัญหาที่ 2) ---
        baitPatienceTimer += delta;
        if (baitPatienceTimer > baitMaxPatienceTime)
        {
            Debug.Log("Boss: หมดความอดทน! กลับไป Chase!");
            manager.currentState = BossManager.BossState.Chase; // ❗️ บังคับกลับไป Chase
            baitPatienceTimer = 0f; // รีเซ็ต
            baitDecisionTimer = 0f; // รีเซ็ต
            return; // ออกจาก Bait state ทันที
        }

        // --- 2. Logic "การตัดสินใจ" (Strafe vs. Still) ---
        baitDecisionTimer -= delta;
        if (baitDecisionTimer <= 0)
        {
            Debug.Log("Boss: กำลังตัดสินใจ... (Bait)");
            int randomChoice = Random.Range(0, 100);
            
            if (randomChoice > 50) 
            {
                isBaitStrafing = true;
                Debug.Log("Boss: ตัดสินใจ 'เดินวน'");
            }
            else
            {
                isBaitStrafing = false;
                Debug.Log("Boss: ตัดสินใจ 'ยืนนิ่ง'");
            }
            baitDecisionTimer = Random.Range(2.0f, 4.0f);
        }

        // --- 3. Logic "การกระทำ" (แก้ปัญหาที่ 1 "เดินลอย") ---
        if (isBaitStrafing)
        {
            // "ฉันจะเดินวน"
            bool isMoving = HandleSmartStrafe(delta); 
            
            // ❗️❗️❗️ นี่คือการแก้บั๊ก "ลอย" ❗️❗️❗️
            // ถ้า isMoving เป็น true (กำลังเดินวน) -> เล่น Animation
            // ถ้า isMoving เป็น false (หยุดรอ Player) -> หยุด Animation
            manager.bossAnim?.UpdateMovement(isMoving ? 1f : 0f);
        }
        else
        {
            // "ฉันจะยืนนิ่ง"
            // ❗️❗️❗️ นี่คือการแก้บั๊ก "ลอย" ❗️❗️❗️
            // ถ้าตัดสินใจจะยืนนิ่ง ก็ต้องสั่ง Animator ให้ Idle
            manager.bossAnim?.UpdateMovement(0f);
        }
    }

    // ❗️ ฟังก์ชันเดินวน (ต้องมี) ❗️
    private bool HandleSmartStrafe(float delta) 
    {
        if (playerController == null)
            return false; 

        float playerMoveX = playerController.MoveInput.x;

        // 1. ถ้า Player หยุดเดิน บอสก็หยุดเดินวน
        if (Mathf.Abs(playerMoveX) < 0.1f)
        {
            strafeTimer = 0f;
            return false; // ❗️ คืนค่าว่า "ไม่ขยับ"
        }

        strafeTimer += delta;

        // 2. ถ้าวนนานไป ให้กลับไป Chase (เป็นการป้องกันอีกชั้น)
        if (strafeTimer >= maxStrafeTime)
        {
            strafeTimer = 0f;
            manager.currentState = BossManager.BossState.Chase;
            return false; // ❗️ คืนค่าว่า "ไม่ขยับ"
        }

        // 3. คำนวณทิศทาง (ใช้ + ตามที่คุณยืนยัน)
        float strafeDir = +Mathf.Sign(playerMoveX); 
        lastStrafeDir = strafeDir;

        Vector3 strafeMove = transform.right * strafeDir * manager.strafeSpeed * delta;
        manager.controller.Move(strafeMove);
        
        return true; // ❗️ คืนค่าว่า "กำลังขยับ"
    }
}