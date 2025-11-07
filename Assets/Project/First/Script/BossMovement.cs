using UnityEngine;

public class BossMovement : MonoBehaviour
{
    private BossManager manager;
    private Vector3 bossVelocity;
    private float gravityValue = -9.81f;

    // 🔹 ตัวแปรใหม่สำหรับพฤติกรรม Bait / Strafe
    private float strafeTimer = 0f;
    private float maxStrafeTime = 15f;
    private bool isStrafing = false;
    private float lastPlayerXPos = 0f;

    private PlayerMovement playerController; // ✅ reference ไปยัง Player
    private float lastPlayerLocalX = 0f;     // สำหรับตรวจทิศเคลื่อนของ player
    private float lastStrafeDir = 0f;        // สำหรับกันการสลับทิศรัว
    private float recoveryTimer = 0f;

    private void Awake()
    {
        manager = GetComponent<BossManager>();

        if (manager.playerTarget != null)
            playerController = manager.playerTarget.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        HandleRotation(Time.deltaTime);

        // ❌ ถ้าอยู่ใน Attack, Stunned, หรือ Dead → หยุดขยับทุกอย่าง
        if (manager.currentState == BossManager.BossState.Attack ||
            manager.currentState == BossManager.BossState.Stunned ||
            manager.currentState == BossManager.BossState.Dead)
        {
            bossVelocity = Vector3.zero;
            return;
        }

        // ✅ ให้เดินเฉพาะตอนอยู่ในโหมด Chase
        if (manager.currentState == BossManager.BossState.Chase)
        {
            HandleSmartMovement(Time.deltaTime);
        }

        HandleGravity();

        // ✅ Force play walk animation if idle
        if (manager.bossAnim != null && manager.bossAnim.animator != null)
        {
            float targetMoveAmount = (manager.playerTarget != null) ? 1f : 0f;

            if (targetMoveAmount > 0.1f &&
                manager.bossAnim.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                manager.bossAnim.animator.Play("Walk", 0);
            }
        }
    }

    public void HandleRotation(float delta)
    {
        if (manager.playerTarget == null || manager.currentState == BossManager.BossState.Dead)
            return;

        // ✅ หมุนเฉพาะตอน Chase เท่านั้น
        if (manager.currentState == BossManager.BossState.Chase)
        {
            Vector3 targetDirection = manager.playerTarget.position - transform.position;
            targetDirection.y = 0;

            if (targetDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, manager.rotationSpeed * delta);
            }
        }
        // ❌ ถ้าเป็น Attack / Stunned / Dead → ไม่หมุนเลย
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


private void HandleSmartMovement(float delta)
{
    if (manager.playerTarget == null)
        return;

    // ❌ ถ้าอยู่ในสถานะพิเศษ → ไม่เคลื่อนไหวเลย
    if (manager.currentState == BossManager.BossState.Attack ||
        manager.currentState == BossManager.BossState.Stunned ||
        manager.currentState == BossManager.BossState.Dead ||
        manager.isRecoveringFromAttack ||           // ✅ เพิ่ม: กำลังฟื้นตัวหลังคอมโบ
        manager.isPlayingAnimation)                 // ✅ เพิ่ม: ระหว่าง animation ห้ามเคลื่อนไหว
    {
        isStrafing = false;
        strafeTimer = 0f;
        return;
    }

    Vector3 toPlayer = manager.playerTarget.position - transform.position;
    toPlayer.y = 0;
    float distance = toPlayer.magnitude;

    // 🟩 1. อยู่นอกระยะ bait → เดินเข้าหาผู้เล่น
    if (distance > manager.baitingDistance)
    {
        isStrafing = false;
        strafeTimer = 0f;
        MoveForward(delta);
        return;
    }

    // 🟨 2. อยู่ในระยะ bait แต่ยังไม่ถึงระยะตี → เดินวน (ถ้ามี input)
    if (distance <= manager.baitingDistance && distance > manager.stoppingDistance)
    {
        HandleSmartStrafe(delta, toPlayer);

        // ✅ ถ้า player ไม่ได้กดซ้าย/ขวา → หยุดวนทันที
        if (playerController != null && Mathf.Abs(playerController.MoveInput.x) < 0.1f)
        {
            isStrafing = false;
            strafeTimer = 0f;
            return;
        }
        return;
    }

    // 🟥 3. ถึงระยะโจมตี → โจมตี
    if (distance <= manager.stoppingDistance)
    {
        isStrafing = false;
        strafeTimer = 0f;
        manager.RequestAttack();
        return;
    }
}


    private void MoveForward(float delta)
    {
        Vector3 move = transform.forward * manager.movementSpeed * delta;
        manager.controller.Move(move);
    }

private void HandleSmartStrafe(float delta, Vector3 toPlayer)
{
    // ถ้ากำลังเล่น animation (เช่น โจมตี) → ไม่ขยับเลย
    if (manager.isPlayingAnimation)
        return;

    if (playerController == null)
        return;

    // อ่าน input จาก player (-1 = ซ้าย, +1 = ขวา)
    float playerMoveX = playerController.MoveInput.x;
    float strafeDir = lastStrafeDir;

    // ถ้าผู้เล่นกดซ้าย/ขวา → ให้บอสเดินวน "ตรงข้าม"
    if (Mathf.Abs(playerMoveX) > 0.1f)
    {
        isStrafing = true;
        strafeTimer += delta;

        if (strafeTimer >= maxStrafeTime)
        {
            isStrafing = false;
            strafeTimer = 0f;
            MoveForward(delta);
            return;
        }

        strafeDir = Mathf.Sign(playerMoveX); // เดินตามทิศ playerMoveX
        lastStrafeDir = strafeDir;

        // เดินวนเฉพาะแนวข้าง
        Vector3 strafeMove = transform.right * strafeDir * manager.strafeSpeed * delta;
        manager.controller.Move(strafeMove);
    }
    else
    {
        // ✅ ถ้า player ไม่ได้กด → หยุดเดินวนทันที
        isStrafing = false;
        strafeTimer = 0f;
    }
}

}
