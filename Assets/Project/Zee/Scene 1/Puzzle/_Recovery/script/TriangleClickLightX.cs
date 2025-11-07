using UnityEngine;

public class TriangleClickLightX : MonoBehaviour
{
    public enum Mode { TwoState, ThreeState, FourState, CustomAngles }

    [Header("โหมดการหมุนแบบ LightX")]
    public Mode mode = Mode.ThreeState;

    [Header("มุมรอบแกน Y (ใช้เมื่อเลือก CustomAngles)")]
    public float[] customYAngles = new float[] { 0f, 120f, 240f };

    [Header("ตัวเลือก")]
    public Transform target;       // ว่างไว้ = ใช้ตัวนี้ (root ของสามเหลี่ยม)
    public bool keepPosition = true;  // กันอาการ “ไหล” ให้อยู่จุดเดิม
    public bool snapExact = true;     // ล็อกมุมให้เป๊ะหลังหมุน

    int index = 0;

    void Awake()
    {
        if (!target) target = transform;
        Apply();
    }

    void OnMouseDown() // ต้องมี Collider ที่คลิกโดน
    {
        index = (index + 1) % Count();
        Apply();
    }

    int Count()
    {
        switch (mode)
        {
            case Mode.TwoState: return 2;           // 0°, 180°
            case Mode.ThreeState: return 3;           // 0°, 120°, 240°
            case Mode.FourState: return 4;           // 0°, 90°, 180°, 270°
            case Mode.CustomAngles:
            default: return Mathf.Max(1, customYAngles.Length);
        }
    }

    float AngleAt(int i)
    {
        switch (mode)
        {
            case Mode.TwoState: return (i % 2) * 180f;
            case Mode.ThreeState: return (i % 3) * 120f;
            case Mode.FourState: return (i % 4) * 90f;
            case Mode.CustomAngles:
            default: return customYAngles[Mathf.Clamp(i, 0, customYAngles.Length - 1)];
        }
    }

    void Apply()
    {
        Vector3 keepPos = target.position;

        // ตั้งมุมรอบแกน Y ตามสถานะ
        var e = target.eulerAngles;
        e.y = AngleAt(index);
        target.eulerAngles = e;

        if (keepPosition) target.position = keepPos;

        if (snapExact) // กัน floating error
        {
            e = target.eulerAngles;
            e.y = AngleAt(index);
            target.eulerAngles = e;
        }
    }
}
