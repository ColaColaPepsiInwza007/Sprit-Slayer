using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public override void Enter(PlayerManager player)
    {
        player.animator.applyRootMotion = false; 
        player.animator.SetTrigger("AttackExit"); 
    }

    public override void Tick(PlayerManager player)
    {
        player.movement.ClearHorizontalVelocity(); 
        player.movement.HandleGravity(); 
        player.movement.ApplyVelocity(); 

        player.lockedTarget = player.lockOn.HandleLockOn(Time.deltaTime, player.inputHandler.moveInput, false, false);
        player.animHandler.UpdateMovementParameters(Vector2.zero, false, player.lockedTarget, false);


        if (player.inputHandler.jumpInput && player.isGrounded && player.jumpCooldownTimer <= 0)
        {
            if (player.stats.HasEnoughStamina(player.jumpStaminaCost))
            {
                player.stats.UseStamina(player.jumpStaminaCost);
                player.jumpCooldownTimer = player.jumpCooldown; 
                player.movement.HandleJump();
            }
        }

        // (*** ❗️❗️❗️ "แก้ไข" 2 บรรทัดนี้ ❗️❗️❗️ ***)
        if (player.inputHandler.rollBufferTimer > 0 && player.isGrounded)
        {
            player.inputHandler.ConsumeRollBuffer(); 
            player.SwitchState(player.rollState); 
            return;
        }

        // (*** ❗️❗️❗️ "แก้ไข" 2 บรรทัดนี้ ❗️❗️❗️ ***)
        if (player.inputHandler.attackBufferTimer > 0 && player.isGrounded && player.isWeaponDrawn)
        {
            player.inputHandler.ConsumeAttackBuffer(); 
            player.SwitchState(player.attackState); 
            return;
        }

        if (player.inputHandler.moveInput.magnitude > 0.1f)
        {
            player.SwitchState(player.moveState); 
            return;
        }
    }
    
    public override void Exit(PlayerManager player) { }
}