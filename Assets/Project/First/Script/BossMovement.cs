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
        HandleRotation(Time.deltaTime); 
        
        if (manager.currentState == BossManager.BossState.Chase)
        {
            HandleTacticalChase(Time.deltaTime);
        }
        
        HandleGravity();

        // Logic บังคับ Animation เดิน (Force Play Fix)
        if (manager.currentState == BossManager.BossState.Chase)
        {
            if (manager.bossAnim != null && manager.bossAnim.animator != null)
            {
                float targetMoveAmount = (manager.playerTarget != null) ? 1f : 0f;
                
                if (targetMoveAmount > 0.1f && manager.bossAnim.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    manager.bossAnim.animator.Play("Walk", 0); 
                    Debug.Log("Forced Animator Play: JUMP to Walk State.");
                }
            }
        }
    }
    
    // จัดการการหันหน้าหา Player ตลอดเวลา (รวมถึงตอนโจมตี)
    public void HandleRotation(float delta)
    {
        if (manager.playerTarget == null || manager.currentState == BossManager.BossState.Dead) return;

        // (*** FIX: ใช้ BossManager.BossState ***)
        if (manager.currentState == BossManager.BossState.Chase || 
            manager.currentState == BossManager.BossState.Attack)
        {
            Vector3 targetDirection = manager.playerTarget.position - transform.position;
            targetDirection.y = 0;
            
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, manager.rotationSpeed * delta);
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
    
    public void HandleTacticalChase(float delta)
    {
        if (manager.playerTarget == null) return;
        
        Vector3 targetDirection = manager.playerTarget.position - transform.position;
        targetDirection.y = 0;
        
        float distanceToTarget = targetDirection.magnitude;
        
        if (distanceToTarget <= manager.stoppingDistance)
        {
            manager.RequestAttack();
            return;
        }

        Vector3 desiredMovement = transform.forward * manager.movementSpeed;
        
        float dotProduct = Vector3.Dot(transform.right, targetDirection.normalized);
        
        if (Mathf.Abs(dotProduct) > 0.3f && distanceToTarget > manager.stoppingDistance) 
        {
            float strafeDirection = Mathf.Sign(dotProduct) * -1f; 
            
            desiredMovement = (transform.right * strafeDirection * manager.strafeSpeed) + 
                              (transform.forward * (manager.movementSpeed * 0.5f));
            
            manager.controller.Move(desiredMovement * delta);
            return; 
        }

        manager.controller.Move(desiredMovement * delta);
    }
}