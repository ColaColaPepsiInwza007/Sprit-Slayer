using UnityEngine;

// ❗️ 1. สร้างคลาสเล็กๆ นี้เพื่อเก็บข้อมูลเสียง
// (เราใส่ [System.Serializable] เพื่อให้มันโชว์ใน Inspector)
[System.Serializable]
public class Sound
{
    public string name;         // ชื่อสำหรับเรียกใช้ (เช่น "BackgroundMusic")
    public AudioClip clip;      // ไฟล์เสียง (ลากใส่)

    [Range(0f, 1f)]
    public float volume = 1f;   // ความดัง
    [Range(0.1f, 3f)]
    public float pitch = 1f;    // ความเร็วเสียง

    public bool loop = false;   // ให้เล่นวนหรือไม่

    [HideInInspector] // ซ่อนไว้ ไม่ต้องยุ่งใน Inspector
    public AudioSource source;
}