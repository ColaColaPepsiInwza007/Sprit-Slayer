using UnityEngine;

// (*** 🚀 State 3: กลิ้ง (อัปเดต 4: ใช้ Logic แบบ FreeLook ตลอด!) 🚀 ***)

public class PlayerRollState : PlayerBaseState
{
    private float rollTimer; 

    public override void Enter(PlayerManager player)
    {
        if (!player.isGrounded)
        {
            player.SwitchState(player.idleState); 
            return;
        }
        if (!player.stats.HasEnoughStamina(player.rollCost))
        {
            player.SwitchState(player.idleState); 
            return;
        }
        
        player.isRolling = true; 
        player.stats.UseStamina(player.rollCost); 
        player.lockOn.SetRollDamping(true);
        player.animator.applyRootMotion = true;
        
        
        // (*** 🚀 FIX: ใช้ Logic "FreeLook" ตลอดเวลา (ตามคำขอ!) 🚀 ***)
        
        Vector2 moveInput = player.inputHandler.moveInput; 
        float moveAmount = moveInput.magnitude;
        Vector3 rollDirection; 

        if (moveAmount > 0.1f) 
        {
            // (Case 1: กด WASD ... กลิ้งตาม "ทิศกล้อง" เสมอ)
            rollDirection = (player.cameraMainTransform.forward * moveInput.y) + (player.cameraMainTransform.right * moveInput.x);
        } 
        else 
        {
            // (Case 2: ไม่กด WASD ... (Backstep))
            rollDirection = -player.transform.forward;
        }
            
        rollDirection.y = 0; 
        if (rollDirection.magnitude > 0.01f) {
            // (*** ❗️ นี่คือหัวใจ: "หมุน" ตัวละครไปตามทิศที่จะกลิ้ง ❗️ ***)
            player.transform.rotation = Quaternion.LookRotation(rollDirection.normalized);
        }
        
        // (*** (เราไม่สน MoveX/MoveY แล้ว เพราะเราใช้ "อนิเมชั่นเดียว") ***)
        // player.animHandler.SetRollDirection(animInput); // (ลบทิ้ง)
        
        player.animHandler.TriggerRoll(); // (เล่นท่ากลิ้ง (ท่าเดียว) ของคุณ)
        
        rollTimer = 0f;
    }

    public override void Tick(PlayerManager player)
    {
        rollTimer += Time.deltaTime;
        if (rollTimer > 1.5f) // (กันค้าง)
        {
            player.SwitchState(player.idleState);
            return;
        }
    }
    
    public override void Exit(PlayerManager player)
    {
        player.isRolling = false;
        player.animator.applyRootMotion = false; 
        player.lockOn.SetRollDamping(false);
        
        // (*** ❗️ พอกลิ้งเสร็จ... State จะเด้งกลับไป Idle/Move ❗️ ***)
        // (*** และ State พวกนั้นจะ "สั่ง" ให้ตัวละครหันกลับไป LockOn เอง ***)
    }
}