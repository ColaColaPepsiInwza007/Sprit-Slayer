using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class EnsureCursorOnLoad : MonoBehaviour
{
    void Start()
    {
        // ปลดล็อก + โชว์เมาส์ทุกครั้งที่เข้าฉากนี้
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // กันเผื่อฉากก่อน Pause ไว้
        Time.timeScale = 1f;
    }
}
