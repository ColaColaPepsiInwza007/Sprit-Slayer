using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BossManager))]
public class BossAI : MonoBehaviour
{
    private BossManager boss;
    private Transform player;
    private float thinkTimer;
    private bool isStrafing = false;

    [Header("AI Settings")]
    public float thinkIntervalMin = 0.4f;
    public float thinkIntervalMax = 0.8f;
    public float detectRange = 10f;
    public float attackRange = 2.5f;

    [Header("Behavior Probability")]
    [Range(0, 1)] public float singleAttackChance = 0.5f;
    [Range(0, 1)] public float comboAttackChance = 0.3f;
    [Range(0, 1)] public float strafeChance = 0.2f;

    [Header("Movement")]
    public float strafeDuration = 1.5f;

    private void Start()
    {
        boss = GetComponent<BossManager>();
        player = boss.playerTarget;
        thinkTimer = Random.Range(thinkIntervalMin, thinkIntervalMax);
    }

    private void Update()
    {
        if (player == null || boss.currentState == BossManager.BossState.Dead)
            return;

        thinkTimer -= Time.deltaTime;

        if (boss.currentState == BossManager.BossState.Attack || isStrafing)
            return;

        if (thinkTimer <= 0)
        {
            DecideNextAction();
            thinkTimer = Random.Range(thinkIntervalMin, thinkIntervalMax);
        }

        FacePlayer();
    }

    private void DecideNextAction()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // ผู้เล่นอยู่ไกลเกิน → ไล่
        if (distance > detectRange)
        {
            boss.currentState = BossManager.BossState.Idle;
            return;
        }

        // 🌀 ระยะกลาง → เดินวนบ้าง
        if (distance > attackRange && distance < attackRange + 2f)
        {
            if (Random.value < strafeChance)
            {
                StartCoroutine(StrafeAroundPlayer());
                Debug.Log("🤖 AI: Strafe (mid range)");
                return;
            }
        }

        // ⚔️ อยู่ในระยะโจมตี
        if (distance <= attackRange)
        {
            float roll = Random.value;
            if (roll < singleAttackChance)
            {
                StartCoroutine(PrepareAttack(1));
                Debug.Log("🤖 AI: Single Attack");
            }
            else if (roll < singleAttackChance + comboAttackChance)
            {
                StartCoroutine(PrepareAttack(3));
                Debug.Log("🤖 AI: Combo Attack");
            }
            else
            {
                StartCoroutine(StrafeAroundPlayer());
                Debug.Log("🤖 AI: Strafe (close)");
            }
        }
        else
        {
            boss.currentState = BossManager.BossState.Chase;
        }
    }

    private IEnumerator PrepareAttack(int combo)
    {
        boss.currentState = BossManager.BossState.Idle;
        yield return new WaitForSeconds(Random.Range(0.2f, 0.6f));

        boss.maxComboCount = combo;
        boss.RequestAttack();
    }

private IEnumerator StrafeAroundPlayer()
{
    isStrafing = true;
    boss.currentState = BossManager.BossState.Idle;

    float timer = 0f;
    float dir = Random.value > 0.5f ? 1f : -1f;

    while (timer < strafeDuration)
    {
        timer += Time.deltaTime;

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(lookDir),
            Time.deltaTime * boss.rotationSpeed
        );

        // 🔹 เดินวนรอบผู้เล่น (ใช้ CharacterController)
        Vector3 strafeDir = transform.right * dir;
        Vector3 move = strafeDir * boss.strafeSpeed * Time.deltaTime;

        // เพิ่มแรงโน้มถ่วงกันอาการกระตุก
        move.y -= 1f * Time.deltaTime;

        boss.controller.Move(move);

        yield return null;
    }

    isStrafing = false;
    boss.currentState = BossManager.BossState.Chase;
}

    private void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * boss.rotationSpeed);
        }
    }
}
