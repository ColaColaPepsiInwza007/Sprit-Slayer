using UnityEngine;

public class BossAnimationEvents : MonoBehaviour
{
    private BossManager manager;

    private void Awake()
    {
        manager = GetComponent<BossManager>();
    }

    /// <summary>
    /// ฟังก์ชันนี้ถูกเรียกใช้โดย Animation Event เมื่อการโจมตีจบลง
    /// </summary>
    public void AnimationAttackFinished()
    {
        if (manager.currentState == BossManager.BossState.Attack)
        {
            manager.currentState = BossManager.BossState.Chase;
            // DEBUG: ยืนยันการออกจาก State
            Debug.Log("Boss: Attack Finished (Event Fired), Back to CHASE.");
        }
    }
}