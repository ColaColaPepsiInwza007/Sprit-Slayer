using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    private bool isSprinting = false; 
    private bool isLockOnSprinting = false; 

    public override void Enter(PlayerManager player) 
    {
        player.animator.applyRootMotion = false; 
        isSprinting = false;
        isLockOnSprinting = false;
    }

    public override void Tick(PlayerManager player)
    {
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

        isSprinting = player.inputHandler.sprintInput; 
        isLockOnSprinting = (player.lockedTarget != null && isSprinting && player.inputHandler.moveInput.magnitude > 0.1f);
        
        player.lockedTarget = player.lockOn.HandleLockOn(
            Time.deltaTime, 
            player.inputHandler.moveInput, 
            player.isRolling, 
            isLockOnSprinting 
        );
        
        player.movement.HandleGravity();
        player.movement.HandleStamina(Time.deltaTime, isSprinting, player.isRolling, player.inputHandler.moveInput.magnitude);
        player.movement.HandleMovement(
            Time.deltaTime, player.inputHandler.moveInput, isSprinting, 
            player.lockedTarget, player.cameraMainTransform, isLockOnSprinting 
        );
        player.movement.ApplyVelocity();
        player.animHandler.UpdateMovementParameters(
            player.inputHandler.moveInput, isSprinting, player.lockedTarget, isLockOnSprinting
        );

        if (player.inputHandler.moveInput.magnitude < 0.1f)
        {
            player.SwitchState(player.idleState); 
            return;
        }
    }
    
    public override void Exit(PlayerManager player) { }
}