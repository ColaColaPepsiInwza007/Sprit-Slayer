using UnityEngine;
using UnityEngine.Playables;   // สำหรับ PlayableDirector

public class CutsceneCameraSwitcher : MonoBehaviour
{
    public PlayableDirector director;

    [Header("กล้องตัวละคร (VCam ผู้เล่น)")]

    // ใส่ GameObject ของ FreeLook_VCam หรือ VCam ที่ใช้เป็นกล้องเล่นจริง
    public GameObject playerVCam;

    [Header("กล้องคัทซีน")]

    // ใส่ GameObject ของ CinemachineCamera (ตัวคัทซีน)
    public GameObject cutsceneVCam;

    void OnEnable()
    {
        if (director != null)
        {
            director.played += OnCutsceneStart;
            director.stopped += OnCutsceneEnd;
        }
    }

    void OnDisable()
    {
        if (director != null)
        {
            director.played -= OnCutsceneStart;
            director.stopped -= OnCutsceneEnd;
        }
    }

    void OnCutsceneStart(PlayableDirector d)
    {
        // ปิดกล้องตัวละคร เปิดกล้องคัทซีน
        if (playerVCam != null) playerVCam.SetActive(false);
        if (cutsceneVCam != null) cutsceneVCam.SetActive(true);
    }

    void OnCutsceneEnd(PlayableDirector d)
    {
        // จบคัทซีน: เปิดกล้องตัวละครกลับมา ปิดกล้องคัทซีน
        if (cutsceneVCam != null) cutsceneVCam.SetActive(false);
        if (playerVCam != null) playerVCam.SetActive(true);
    }
}
