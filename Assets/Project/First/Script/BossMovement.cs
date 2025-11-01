using UnityEngine;

public class BossMovement : MonoBehaviour
{
    private BossManager manager;
    private Vector3 bossVelocity;
    private float gravityValue = -9.81f;

    private void Awake()
    {
        // บรรทัดนี้คือส่วนที่ "เรียกหา" BossManager
        manager = GetComponent<BossManager>();
        // ถ้า BossManager ไม่ถูกสร้าง (หรือชื่อผิด) บรรทัดนี้จะ Error!
    }

    private void Update()
    {
        HandleChasePlayer(Time.deltaTime);
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

    // BossMovement.cs
    // ...

    public void HandleChasePlayer(float delta)
    {
        if (manager.playerTarget == null) return;

        Vector3 targetDirection = manager.playerTarget.position - transform.position;
        targetDirection.y = 0;

        // (*** โค้ดที่แก้ไข: เช็คระยะห่าง ***)
        float distanceToTarget = targetDirection.magnitude;

        // 1. ถ้า Boss อยู่ไกลกว่าระยะหยุด (Stopping Distance) ให้เดินตาม
        if (distanceToTarget > manager.stoppingDistance)
        {
            // 2. หันหน้าเข้าหา Player (เหมือนเดิม)
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, manager.rotationSpeed * delta);

            // 3. เดินไปข้างหน้า
            manager.controller.Move(transform.forward * manager.movementSpeed * delta);
        }
        else
        {
            // 4. ถ้าอยู่ในระยะ: Boss จะหยุดเดิน
            // เรายังคงให้ Boss หันหน้าหา Player อยู่ (เพื่อให้พร้อมโจมตี)
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, manager.rotationSpeed * delta);

            // (*** เพิ่ม: ตรวจสอบไม่ให้ Boss เคลื่อนที่ไปข้างหน้า ***)
            // (ไม่มี controller.Move() ในส่วนนี้)
        }
    }
}