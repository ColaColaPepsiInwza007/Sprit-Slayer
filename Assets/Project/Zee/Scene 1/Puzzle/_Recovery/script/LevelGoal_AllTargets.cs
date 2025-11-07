using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class LevelGoal_AllTargets : MonoBehaviour
{
    [Tooltip("????????? = ?? TargetHoldReceiver ???????????????")]
    public TargetHoldReceiver[] targets;

    [Tooltip("?????????????? ?: ???????????????????????????????????????????????")]
    public float confirmSeconds = 0.0f;

    public UnityEvent onLevelComplete;

    private float _confirmTimer;
    private bool _fired;

    void Start()
    {
        if (targets == null || targets.Length == 0)
            targets = FindObjectsOfType<TargetHoldReceiver>(includeInactive: false);
    }

    void Update()
    {
        if (_fired || targets == null || targets.Length == 0) return;

        bool allHeld = targets.All(t => t && t.IsHeld);
        if (allHeld)
        {
            _confirmTimer += Time.deltaTime;
            if (_confirmTimer >= confirmSeconds)
            {
                _fired = true;
                onLevelComplete?.Invoke();
                Debug.Log("[GOAL] Level Complete!");
            }
        }
        else
        {
            _confirmTimer = 0f;
        }
    }
}
