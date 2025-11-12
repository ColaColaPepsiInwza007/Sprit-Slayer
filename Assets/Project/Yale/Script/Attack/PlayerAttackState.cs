using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    public override void Enter(PlayerManager player)
    {
        player.lastAttackStartTime = Time.time;
        player.isAttacking = true;
        player.canCombo = false;
        player.canRollCancel = false;
        player.animator.applyRootMotion = true;
        player.animator.ResetTrigger("AttackExit");

        if (player.attackToPlayNext == null) { player.currentAttackData = player.startingLightAttack; }
        else { player.currentAttackData = player.attackToPlayNext; }
        player.attackToPlayNext = null; 

        if (player.lockedTarget != null)
        {
            Vector3 targetDirection = player.lockedTarget.position - player.transform.position;
            targetDirection.y = 0;
            if (targetDirection.sqrMagnitude > 0.001f) { player.transform.rotation = Quaternion.LookRotation(targetDirection.normalized); }
        }
        else
        {
            Vector2 moveInput = player.inputHandler.moveInput;
            if (moveInput.magnitude > 0.1f)
            {
                Vector3 moveDirection = (player.cameraMainTransform.forward * moveInput.y) + (player.cameraMainTransform.right * moveInput.x);
                moveDirection.y = 0;
                if (moveDirection.sqrMagnitude > 0.001f) { player.transform.rotation = Quaternion.LookRotation(moveDirection.normalized); }
            }
        }
        
        if (player.currentAttackData != null)
        {
            if (!player.stats.HasEnoughStamina(player.currentAttackData.staminaCost))
            {
                player.SwitchState(player.idleState); 
                return;
            }
            player.stats.UseStamina(player.currentAttackData.staminaCost); 
            player.animator.CrossFade(player.currentAttackData.animationName, player.currentAttackData.transitionTime);
        }
        else
        {
            Debug.LogWarning("ไม่มี AttackData! กลับไป Idle");
            player.SwitchState(player.idleState);
            return;
        }
    }

    public override void Tick(PlayerManager player)
    {
        // (*** ❗️❗️❗️ "แก้ไข" 2 บรรทัดนี้ ❗️❗️❗️ ***)
        if (player.canRollCancel && player.inputHandler.rollBufferTimer > 0 && player.isGrounded)
        {
            player.inputHandler.ConsumeRollBuffer(); 
            
            player.isAttacking = false;
            player.canCombo = false;
            player.canRollCancel = false;
            player.attackToPlayNext = null;
            player.animator.SetTrigger("AttackExit"); 
            
            player.SwitchState(player.rollState);
            return;
        }

        // (*** ❗️❗️❗️ "แก้ไข" 2 บรรทัดนี้ ❗️❗️❗️ ***)
        if (player.canCombo) 
        {
            if (player.attackToPlayNext != null) 
            {
                if (player.inputHandler.attackBufferTimer > 0) 
                {
                    player.inputHandler.ConsumeAttackBuffer(); 
                    player.animator.SetTrigger("AttackExit"); 
                    player.SwitchState(player.attackState); 
                    return;
                }
            }
        }
    }
    
    public override void Exit(PlayerManager player)
    {
        player.animator.applyRootMotion = false; 
    }
}