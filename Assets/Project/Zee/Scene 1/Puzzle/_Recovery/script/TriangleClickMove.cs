using UnityEngine;

public class TriangleClickMove : MonoBehaviour
{
    [Header("เป้าหมายที่ต้องหมุน (เว้นว่างไว้ = ตัวนี้)")]
    public Transform target;

    [Header("มุมรอบแกน Y ที่จะหมุนไปตามลำดับ (องศา)")]
    public float[] yAngles = new float[] { 0f, 120f, 240f };

    [Header("ตำแหน่งที่จะขยับไป (เว้นว่างไว้ถ้าไม่อยากให้ขยับ)")]
    public Transform[] movePoints;

    [Header("ตัวเลือก")]
    public bool smoothMove = true;  // ให้เคลื่อนไหวลื่น ๆ
    public float moveSpeed = 5f;    // ความเร็วเคลื่อนที่
    public bool snapExact = true;   // ล็อกมุมให้อยู่เป๊ะ
    public bool resetToFirst = false; // ให้กลับจุดแรกหลังครบทั้งหมด

    int index = 0;
    bool moving = false;

    void Awake()
    {
        if (!target) target = transform;
        ApplyImmediate();
    }

    void OnMouseDown()
    {
        if (moving) return;

        index++;
        if (index >= yAngles.Length || (movePoints.Length > 0 && index >= movePoints.Length))
        {
            index = resetToFirst ? 0 : (yAngles.Length - 1);
        }

        ApplyStep();
    }

    void ApplyStep()
    {
        // เริ่มหมุน + เคลื่อนตำแหน่ง
        Vector3 startPos = target.position;
        Vector3 endPos = (movePoints != null && movePoints.Length > 0 && index < movePoints.Length)
            ? movePoints[index].position
            : startPos;

        Quaternion startRot = target.rotation;
        Quaternion endRot = Quaternion.Euler(0f, yAngles[index], 0f);

        if (smoothMove)
        {
            StartCoroutine(MoveAndRotateSmooth(startPos, endPos, startRot, endRot));
        }
        else
        {
            target.position = endPos;
            target.rotation = endRot;
        }
    }

    System.Collections.IEnumerator MoveAndRotateSmooth(Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot)
    {
        moving = true;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            target.position = Vector3.Lerp(startPos, endPos, t);
            target.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        target.position = endPos;
        target.rotation = endRot;

        if (snapExact)
        {
            var e = target.eulerAngles;
            e.y = Mathf.Round(e.y / 1f) * 1f;
            target.eulerAngles = e;
        }

        moving = false;
    }

    void ApplyImmediate()
    {
        if (yAngles.Length > 0)
        {
            var e = target.eulerAngles;
            e.y = yAngles[0];
            target.eulerAngles = e;
        }
        if (movePoints.Length > 0)
            target.position = movePoints[0].position;
    }
}
