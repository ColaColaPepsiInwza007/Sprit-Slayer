using UnityEngine;

public class BossMovement : MonoBehaviour
{
    private BossManager manager;
    private Vector3 bossVelocity;
    private float gravityValue = -9.81f;

    private void Awake()
    {
        manager = GetComponent<BossManager>();
    }

    private void Update()
    {
        // ทำ Movement เฉพาะ State Chase เท่านั้น
        if (manager.currentState == BossManager.BossState.Chase)
        {
            HandleTacticalChase(Time.deltaTime); // <--- เรียกใช้ฟังก์ชัน Tactical Chase
        }
        else if (manager.currentState == BossManager.BossState.Attack)
        {
            // หยุด Movement เมื่อโจมตี
        }
        
        HandleGravity();
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
    
    // *** ฟังก์ชัน: จัดการการเคลื่อนไหวเชิงยุทธวิธี (Tactical Chase) ***
    public void HandleTacticalChase(float delta)
    {
        if (manager.playerTarget == null) return;
        
        Vector3 targetDirection = manager.playerTarget.position - transform.position;
        targetDirection.y = 0;
        
        float distanceToTarget = targetDirection.magnitude;
        
        // 1. หันหน้าเข้าหา Player เสมอ
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, manager.rotationSpeed * delta);
        
        // 2. ถ้าถึงระยะโจมตี ให้โจมตีทันที
        if (distanceToTarget <= manager.stoppingDistance)
        {
            manager.RequestAttack();
            return;
        }

        // 3. (*** Logic การตอบโต้: เดินเข้าหา/เดินสวนทาง ***)
        
        Vector3 desiredMovement = transform.forward * manager.movementSpeed;
        
        // ตรวจสอบ Player Lateral Movement: ใช้ Dot Product ของ Boss.Right กับ DirectionToPlayer
        Vector3 directionToPlayer = targetDirection.normalized;
        float dotProduct = Vector3.Dot(transform.right, directionToPlayer);
        
        // ถ้า Player อยู่ในองศาที่เอียง (Abs(dotProduct) > 0.3) และยังไม่ประชิด (ยังไม่ถึง stoppingDistance)
        if (Mathf.Abs(dotProduct) > 0.3f && distanceToTarget > manager.stoppingDistance) 
        {
            // Boss เดินสวนทาง (Strafe) พร้อมกับเดินเข้าหา
            
            float strafeDirection = Mathf.Sign(dotProduct) * -1f; // สวนทางกับทิศที่ Player เอียง
            
            // คำนวณ Movement Vector: เดินด้านข้าง + เดินไปข้างหน้าครึ่งหนึ่งของความเร็ว
            desiredMovement = (transform.right * strafeDirection * manager.strafeSpeed) + 
                              (transform.forward * (manager.movementSpeed * 0.5f));
            
            // ใช้ความเร็วในการเดินสวนทาง
            manager.controller.Move(desiredMovement * delta);
            return; 
        }

        // *** เงื่อนไข: เดินตรงเข้าหา (Default / Player เดินถอยหลัง/หยุด) ***
        
        // Boss เดินตรงเข้าหาด้วยความเร็วปกติ
        manager.controller.Move(desiredMovement * delta);
    }
}