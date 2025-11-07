using UnityEngine;

public class LaserSplitter3D : MonoBehaviour
{
    [Header("Outputs (use child transforms)")]
    public Transform OutA;
    public Transform OutB;

    [Tooltip("?????????????????????????????????? ???? 0.5 = ?????????")]
    [Range(0f, 1f)] public float splitIntensityFactor = 0.5f;

    private void OnValidate()
    {
        if (OutA == null) OutA = transform.Find("OutA");
        if (OutB == null) OutB = transform.Find("OutB");
    }

    public void GetOutputs(out Vector3 posA, out Vector3 dirA, out Vector3 posB, out Vector3 dirB)
    {
        Transform a = OutA != null ? OutA : transform;
        Transform b = OutB != null ? OutB : transform;

        posA = a.position; dirA = a.forward.normalized;
        posB = b.position; dirB = b.forward.normalized;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawOut(OutA, Color.cyan);
        DrawOut(OutB, Color.magenta);
    }
    private void DrawOut(Transform t, Color c)
    {
        if (!t) return;
        Gizmos.color = c;
        Gizmos.DrawSphere(t.position, 0.03f);
        Gizmos.DrawRay(t.position, t.forward * 0.4f);
    }
#endif
}
