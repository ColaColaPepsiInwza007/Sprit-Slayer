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
        if (manager.currentState == BossManager.BossState.Chase)
        {
            HandleChasePlayer(Time.deltaTime);
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

    public void HandleChasePlayer(float delta)
    {
        if (manager.playerTarget == null) return;

        Vector3 targetDirection = manager.playerTarget.position - transform.position;
        targetDirection.y = 0;

        float distanceToTarget = targetDirection.magnitude;

        if (distanceToTarget > manager.stoppingDistance)
        {
            // เดินตาม
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, manager.rotationSpeed * delta);
            manager.controller.Move(transform.forward * manager.movementSpeed * delta);
        }
        else
        {
            // ถึงระยะหยุด: ส่ง RequestAttack ไปยัง Manager
            manager.RequestAttack();

            // ยังคงหันหน้าหา Player ไว้
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, manager.rotationSpeed * delta);
        }
    }
}