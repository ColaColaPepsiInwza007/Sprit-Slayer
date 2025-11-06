using UnityEngine;

public class BossAnimator : MonoBehaviour
{
    private BossManager manager;
    public Animator animator; 

    [Header("Animation Speed")]
    [SerializeField] private float animationSpeedMultiplier = 1.0f; 

    private void Awake()
    {
        manager = GetComponent<BossManager>();
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("BossAnimator: Animator component is missing on the Boss GameObject.");
        }
        
        if (animator != null)
        {
             animator.speed = animationSpeedMultiplier;
        }
    }

    public void UpdateMovement(float moveAmount)
    {
        if (animator == null) return;
        animator.SetFloat("MoveY", moveAmount, 0.1f, Time.deltaTime);
    }
    
    public void TriggerAttack(int attackIndex) 
    {
        if (animator == null) return;
        
        animator.SetInteger("AttackIndex", attackIndex); 
        animator.SetTrigger("Attack");
    }
    
    public void ResetAttackTrigger()
    {
        if (animator == null) return;
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("ComboExit"); 
        Debug.Log("Animator: Triggers Reset.");
    }
    
    public void SetAnimationSpeed(float multiplier)
    {
        if (animator == null) return;
        animator.speed = multiplier;
        animationSpeedMultiplier = multiplier; 
    }
}