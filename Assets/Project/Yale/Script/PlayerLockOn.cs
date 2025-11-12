using Unity.Cinemachine; 
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerLockOn : MonoBehaviour
{
    private PlayerManager manager;

    [Header("Camera Setup")]
    [Header("Lock-On Settings")]
    [SerializeField] private LayerMask enemyLayer; 
    [SerializeField] private LayerMask playerLayer; 
    [SerializeField] private float maxLockOnDistance = 20f; 
    [SerializeField] private float minLockOnDot = 0.5f; 
    [SerializeField] private Transform playerTargetIcon; 
    [Header("Target Switching")]
    [SerializeField] private float switchTargetDeadzone = 0.8f; 
    [SerializeField] private float switchTargetCooldown = 0.2f; 
    private float lastSwitchTime;
    private bool canSwitch;

    private List<Transform> availableTargets = new List<Transform>();
    private int currentTargetIndex = -1;
    
    private LayerMask raycastMask; 

    private void Awake()
    {
        manager = GetComponent<PlayerManager>();
        raycastMask = ~playerLayer; 
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

    public Transform HandleLockOn(float delta, Vector2 moveInput, bool isRolling, bool isLockOnSprinting)
    {
        if (manager.lockedTarget == null) return null;

        // (*** 🚀 FIX 1 (LockOn Delay): "เพิ่ม" เช็ค 'rollBufferTimer'! 🚀 ***)
        // (ถ้า (กลิ้ง) OR (ตี) OR (กำลังจะกลิ้ง) OR (กำลังวิ่งปลดล็อค)... "หยุดหมุนตัว")
        
        // (*** ❗️❗️❗️ แก้ไขบรรทัดนี้ ❗️❗️❗️ ***)
        if (isRolling || manager.isAttacking || isLockOnSprinting || manager.inputHandler.rollBufferTimer > 0) 
        {
            return manager.lockedTarget; 
        }

        // (*** (โค้ดที่เหลือ... คือ Logic การหมุนตัว/สลับเป้า/เช็คระยะ ... เหมือนเดิมเป๊ะ) ***)
        
        Vector3 targetDir = manager.lockedTarget.position - transform.position;
        targetDir.y = 0;
        manager.movement.HandleLockOnRotation(targetDir.normalized, delta);

        float distance = Vector3.Distance(transform.position, manager.lockedTarget.position);
        if (distance > maxLockOnDistance)
        {
            UnlockTarget();
            return null;
        }

        HandleTargetSwitching(moveInput);

        if (playerTargetIcon != null)
        {
            if (manager.lockedTarget != null) 
            {
                playerTargetIcon.position = manager.lockedTarget.GetComponent<Collider>().bounds.center;
            }
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
            Vector3 targetCenter = hit.bounds.center; 
            Vector3 dirToEnemy = targetCenter - manager.cameraMainTransform.position; 
            Vector3 camForward = manager.cameraMainTransform.forward;
            dirToEnemy.Normalize();
            float dot = Vector3.Dot(camForward, dirToEnemy);
            if (dot < minLockOnDot) continue;
            if (Physics.Raycast(manager.cameraMainTransform.position, dirToEnemy, out RaycastHit rayHit, maxLockOnDistance, raycastMask))
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

    public void SetRollDamping(bool isDamping) { }

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
        LockOnTo(availableTargets[currentTargetIndex]);
        lastSwitchTime = Time.time;
        canSwitch = false; 
    }

    private void LockOnTo(Transform target)
    {
        manager.lockedTarget = target;
        if (playerTargetIcon != null) playerTargetIcon.gameObject.SetActive(true);
    }
    
    public void UnlockTarget() 
    {
        manager.lockedTarget = null;
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