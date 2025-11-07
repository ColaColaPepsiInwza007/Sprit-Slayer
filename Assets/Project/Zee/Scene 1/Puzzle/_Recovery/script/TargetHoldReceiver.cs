using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TargetHoldReceiver : MonoBehaviour, ILaserReceiver3D
{
    [Header("??????????????????????? (0 = ??????????????????)")]
    public float requiredHoldSeconds = 1.0f;

    [Header("????????????????????")]
    public bool instantReset = true;   // true = ???????????????????
    public float decayPerSecond = 3f;  // ???????? instantReset=false

    [Header("Visual (??????)")]
    public Renderer rend;
    public Color idleColor = Color.gray;
    public Color onColor = Color.cyan;

    public float Hold => _hold;
    public bool IsHeld => _hold >= requiredHoldSeconds;

    private float _hold = 0f;
    private bool _hitThisFrame;

    void Awake()
    {
        if (!rend) rend = GetComponentInChildren<Renderer>();
        if (rend) UpdateVisual();
    }

    void Update()
    {
        if (_hitThisFrame)
        {
            _hold = Mathf.Min(requiredHoldSeconds, _hold + Time.deltaTime);
        }
        else
        {
            if (instantReset) _hold = 0f;
            else _hold = Mathf.Max(0f, _hold - decayPerSecond * Time.deltaTime);
        }
        _hitThisFrame = false;
        UpdateVisual();
    }

    public void OnLaserHit(Vector3 hitPoint, Vector3 inDirection, Color color, float intensity)
    {
        _hitThisFrame = true;
    }

    private void UpdateVisual()
    {
        if (!rend) return;
        float t = requiredHoldSeconds <= 0f ? 1f : Mathf.Clamp01(_hold / requiredHoldSeconds);
        Color c = Color.Lerp(idleColor, onColor, t);
        rend.material.color = c;
        if (rend.material.HasProperty("_EmissionColor"))
            rend.material.SetColor("_EmissionColor", c * (0.25f + 0.75f * t));
    }
}
