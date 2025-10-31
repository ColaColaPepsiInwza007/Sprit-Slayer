using Unity.Cinemachine; // <--- V.3
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// (เวอร์ชัน "กล้อง FreeLook" - ตัวละครล็อคเป้า แต่กล้องเป็นอิสระ)
public class PlayerLockOn : MonoBehaviour
{
    private PlayerManager manager;

    [Header("Camera Setup")]
    // (เราไม่จำเป็นต้องยุ่งกับ Animator หรือ VCam อีกต่อไป)
    // [SerializeField] private Animator cinemachineAnimator; 
    // [SerializeField] private CinemachineCamera lockOnCamera; 

    [Header("Lock-On Settings")]
    [SerializeField] private LayerMask enemyLayer; 
    [SerializeField] private float maxLockOnDistance = 20f; 
    [SerializeField] private float minLockOnDot = 0.5f; 
    [SerializeField] private Transform playerTargetIcon; 

    [Header("Target Switching")]
    [SerializeField] private float switchTargetDeadzone = 0.8f; 
    [SerializeField] private float switchTargetCooldown = 0.2f; 
    private float lastSwitchTime;
    private bool canSwitch;

    // (เราไม่จำเป็นต้องใช้ Damping หรือ TargetGroup อีก)
    // private Cinemachine3rdPersonFollow thirdPersonFollow;
    // private float defaultDampingX;
    // private float defaultDampingY;

    private List<Transform> availableTargets = new List<Transform>();
    private int currentTargetIndex = -1;

    private void Awake()
    {
        manager = GetComponent<PlayerManager>();
        
        // (ส่วน Awake() ที่ยุ่งกับกล้อง ไม่จำเป็นต้องใช้แล้ว)
    }

    public void TryToggleLockOn()
    {
        if (manager.lockedTarget == null)
        {
            FindAndLockTarget();
        }
        else
        {
            UnlockTarget();
        }
    }

    // (ฟังก์ชันนี้ยังต้องทำงานเหมือนเดิมเป๊ะ!)
    // (เพื่อ "หมุนตัวละคร" และ "เช็คระยะ" ศัตรู)
    public Transform HandleLockOn(float delta, Vector2 moveInput, bool isRolling, bool isLockOnSprinting)
    {
        if (manager.lockedTarget == null) return null;
        if (isRolling) return manager.lockedTarget; 

        if (!isLockOnSprinting)
        {
            Vector3 targetDir = manager.lockedTarget.position - transform.position;
            targetDir.y = 0;
            manager.movement.HandleLockOnRotation(targetDir.normalized, delta);
        }

        float distance = Vector3.Distance(transform.position, manager.lockedTarget.position);
        if (distance > maxLockOnDistance)
        {
            UnlockTarget();
            return null;
        }

        // (ฟีเจอร์ "สลับเป้าหมาย" ยังคงทำงานได้!)
        HandleTargetSwitching(moveInput);

        if (playerTargetIcon != null)
        {
            playerTargetIcon.position = manager.lockedTarget.position;
        }

        return manager.lockedTarget;
    }

    private void FindAndLockTarget()
    {
        availableTargets.Clear();
        Transform bestTarget = null;
        float highestDot = 0; 

        Collider[] hits = Physics.OverlapSphere(transform.position, maxLockOnDistance, enemyLayer);

        foreach (var hit in hits)
        {
            Vector3 dirToEnemy = hit.transform.position - manager.cameraMainTransform.position;
            Vector3 camForward = manager.cameraMainTransform.forward;
            dirToEnemy.Normalize();

            float dot = Vector3.Dot(camForward, dirToEnemy);

            if (dot < minLockOnDot) continue;

            if (Physics.Raycast(manager.cameraMainTransform.position, dirToEnemy, out RaycastHit rayHit, maxLockOnDistance))
            {
                if (rayHit.transform != hit.transform) continue; 
            }

            availableTargets.Add(hit.transform);

            if (dot > highestDot)
            {
                highestDot = dot;
                bestTarget = hit.transform;
            }
        }

        if (bestTarget != null)
        {
            SortTargetsLeftToRight();
            currentTargetIndex = availableTargets.IndexOf(bestTarget);
            LockOnTo(bestTarget);
        }
    }

    // (ฟังก์ชันนี้ไม่จำเป็นต้องใช้แล้ว เพราะเราไม่ได้สลับกล้อง)
    public void SetRollDamping(bool isDamping)
    {
        // if (thirdPersonFollow == null) return;
        // ... (ไม่ต้องทำอะไร) ...
    }

    private void HandleTargetSwitching(Vector2 moveInput)
    {
        if (availableTargets.Count <= 1) return; 

        float horizontalInput = moveInput.x;

        if (Time.time < lastSwitchTime + switchTargetCooldown) return;

        if (Mathf.Abs(horizontalInput) < switchTargetDeadzone)
        {
            canSwitch = true;
            return;
        }

        if (!canSwitch) return; 

        int switchDirection = (horizontalInput > 0) ? 1 : -1;
        currentTargetIndex += switchDirection;

        currentTargetIndex = Mathf.Clamp(currentTargetIndex, 0, availableTargets.Count - 1);
        
        if (availableTargets[currentTargetIndex] == manager.lockedTarget)
        {
             return; 
        }

        // (สลับเป้าหมายจริงๆ)
        LockOnTo(availableTargets[currentTargetIndex]);
        lastSwitchTime = Time.time;
        canSwitch = false; 
    }

    private void LockOnTo(Transform target)
    {
        // (*** โค้ดที่อัปเดต (คลีน) ***)
        
        // 1. บอก Manager (เพื่อให้ตัวละครเปลี่ยนโหมด)
        manager.lockedTarget = target;
        
        // 2. (ปิดโค้ดส่วนที่ Crash)
        // cinemachineAnimator.Play("TargetLock"); 

        if (playerTargetIcon != null) playerTargetIcon.gameObject.SetActive(true);
    }

    private void UnlockTarget()
    {
        // (*** โค้ดที่อัปเดต (คลีน) ***)
        
        // 1. บอก Manager
        manager.lockedTarget = null;
        
        // 2. (ปิดโค้ดส่วนที่ Crash)
        // cinemachineAnimator.Play("FreeLook"); 

        availableTargets.Clear();
        currentTargetIndex = -1;

        if (playerTargetIcon != null) playerTargetIcon.gameObject.SetActive(false);
    }

    private void SortTargetsLeftToRight()
    {
        Vector3 camRight = manager.cameraMainTransform.right;
        availableTargets.Sort((a, b) => {
            Vector3 dirA = (a.position - transform.position).normalized;
            Vector3 dirB = (b.position - transform.position).normalized;
            float dotA = Vector3.Dot(camRight, dirA);
            float dotB = Vector3.Dot(camRight, dirB);
            return dotA.CompareTo(dotB); 
        });
    }
}