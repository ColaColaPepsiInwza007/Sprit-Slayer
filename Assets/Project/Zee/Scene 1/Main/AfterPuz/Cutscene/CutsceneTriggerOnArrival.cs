using UnityEngine;
using UnityEngine.Playables; // ถ้าใช้ Timeline
using System.Collections;

public class CutsceneTriggerOnArrival : MonoBehaviour
{
    [Header("อ้างอิง (อย่างใดอย่างหนึ่ง)")]
    public PlayableDirector timeline;  // ใช้ Timeline ก็ใส่ตัวนี้
    public Animator doorAnimator;      // หรือใช้ Animator ธรรมดา (Trigger: Open)

    [Header("ตั้งค่า")]
    public string playerTag = "Player";
    public bool autoStartIfPlayerInside = true; // เล่นเองถ้าเกิดมาทับ Trigger
    public MonoBehaviour[] controlsToDisable;   // สคริปต์ควบคุม Player ที่ต้องปิดชั่วคราว

    bool played = false;
    Collider triggerCol;

    void Awake()
    {
        triggerCol = GetComponent<Collider>();
        triggerCol.isTrigger = true;
    }

    void Start()
    {
        if (autoStartIfPlayerInside)
            StartCoroutine(CheckPlayerInsideAtStart());
    }

    IEnumerator CheckPlayerInsideAtStart()
    {
        // รอ 1 เฟรมให้ Player spawn เสร็จก่อน
        yield return null;

        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) yield break;

        // ถ้าตำแหน่งผู้เล่นเริ่มอยู่ใน Trigger ให้เล่นคัทซีนเลย
        if (triggerCol.bounds.Contains(player.transform.position))
            StartCutscene();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!played && other.CompareTag(playerTag))
            StartCutscene();
    }

    void StartCutscene()
    {
        if (played) return;
        played = true;

        // ปิดการควบคุมผู้เล่นชั่วคราว
        foreach (var c in controlsToDisable) if (c) c.enabled = false;

        // เล่นคัทซีนแบบที่ใช้
        if (timeline)
        {
            timeline.stopped += _ => RestoreControl();
            timeline.Play();
        }
        else if (doorAnimator)
        {
            doorAnimator.SetTrigger("Open");
            // เปิดคอนโทรลคืนหลังจบแอนิเมชันได้ด้วย Animation Event หรือหน่วงเวลาเอา:
            Invoke(nameof(RestoreControl), 2f); // ปรับเวลาให้พอดีกับคลิป
        }
        else
        {
            // ไม่มีอะไรให้เล่น ก็คืนคอนโทรลทันที (กันพลาด)
            RestoreControl();
        }
    }

    void RestoreControl()
    {
        foreach (var c in controlsToDisable) if (c) c.enabled = true;
    }
}
