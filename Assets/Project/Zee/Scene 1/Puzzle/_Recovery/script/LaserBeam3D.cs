using System.Collections.Generic;
using UnityEngine;

public class LaserBeam3D : MonoBehaviour
{
    [Header("Beam")]
    public Color beamColor = Color.red;
    public float maxDistancePerSegment = 100f;
    public int maxBounces = 50;
    public LayerMask hitLayers = ~0;
    public float lineWidth = 0.02f;

    [Header("Emission")]
    public Transform firePoint;                       // ???????????
    public Vector3 initialDirection = Vector3.forward;

    [Header("Raycast")]
    public QueryTriggerInteraction triggerHits = QueryTriggerInteraction.Collide;

    // line renderer pool
    private readonly List<LineRenderer> linePool = new List<LineRenderer>();
    private int usedThisFrame;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void LateUpdate()
    {
        usedThisFrame = 0;

        Vector3 origin = firePoint ? firePoint.position : transform.position;
        Vector3 dir = firePoint ? firePoint.forward : initialDirection.normalized;

        TraceAndDraw(origin, dir, beamColor, 1f, 0);

        // disable unused lines
        for (int i = usedThisFrame; i < linePool.Count; i++)
            linePool[i].gameObject.SetActive(false);
    }

    private void TraceAndDraw(Vector3 origin, Vector3 direction, Color color, float intensity, int depth)
    {
        if (depth > maxBounces || intensity <= 0.01f) return;

        Vector3 currOrigin = origin;
        Vector3 currDir = direction.normalized;

        for (int step = depth; step < maxBounces; step++)
        {
            if (!Physics.Raycast(currOrigin, currDir, out RaycastHit hit, maxDistancePerSegment, hitLayers, triggerHits))
            {
                DrawSegment(currOrigin, currOrigin + currDir * maxDistancePerSegment, color);
                break;
            }

            DrawSegment(currOrigin, hit.point, color);

            // ---- Splitter (??? 2 ???) ----
            var splitter = hit.collider.GetComponentInParent<LaserSplitter3D>();
            if (splitter != null)
            {
                splitter.GetOutputs(out Vector3 posA, out Vector3 dirA, out Vector3 posB, out Vector3 dirB);

                float childIntensity = intensity * splitter.splitIntensityFactor;
                TraceAndDraw(posA + dirA * 0.001f, dirA, color, childIntensity, step + 1);
                TraceAndDraw(posB + dirB * 0.001f, dirB, color, childIntensity, step + 1);
                break; // ???????????????
            }

            // ---- Mirror (??????) ----
            Transform t = hit.collider.transform;
            bool isMirror = t.CompareTag("Mirror") || (t.parent && t.parent.CompareTag("Mirror"));
            if (isMirror)
            {
                Vector3 normal = hit.normal;
                currOrigin = hit.point + normal * 0.001f;
                currDir = Vector3.Reflect(currDir, normal).normalized;
                continue;
            }

            // ---- Target (?????? ???????) ----
            var recv = hit.collider.GetComponentInParent<TargetHoldReceiver>();
            if (recv != null || t.CompareTag("Target") || (t.parent && t.parent.CompareTag("Target")))
            {
                if (recv != null)
                    recv.OnLaserHit(hit.point, currDir, color, intensity);
                // ??????????? ?????????
                break;
            }

            // ????????????????????/??? ? ??
            break;
        }
    }

    private void DrawSegment(Vector3 a, Vector3 b, Color c)
    {
        LineRenderer lr = GetLine();
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        lr.startColor = c;
        lr.endColor = c;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.numCapVertices = 2;
    }

    private LineRenderer GetLine()
    {
        if (usedThisFrame < linePool.Count)
        {
            var lr = linePool[usedThisFrame];
            lr.gameObject.SetActive(true);
            usedThisFrame++;
            return lr;
        }
        else
        {
            var go = new GameObject("LaserSegment3D");
            var lr = go.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.useWorldSpace = true;
            linePool.Add(lr);
            usedThisFrame++;
            return lr;
        }
    }
}

// ??????: ???????????????????????? ?
public interface ILaserReceiver3D
{
    void OnLaserHit(Vector3 hitPoint, Vector3 inDirection, Color color, float intensity);
}
