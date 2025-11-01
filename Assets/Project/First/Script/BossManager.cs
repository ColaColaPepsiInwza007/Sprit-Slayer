using UnityEngine;

public class BossManager : MonoBehaviour
{
    // กำหนดให้ Boss ใช้ CharacterController เหมือน Player
    [Header("Core Components")]
    public CharacterController controller;
    public Transform playerTarget; // <--- ตัวแปรสำหรับ Player

    [Header("Boss State")]
    public float movementSpeed = 4.0f; // ความเร็วในการเดิน
    public float rotationSpeed = 10.0f; // ความเร็วในการหัน
    public float stoppingDistance = 1.5f;

    private void Awake()
    {
        // 1. ตรวจสอบว่ามี CharacterController ติดตั้งอยู่บน Object นี้
        controller = GetComponent<CharacterController>();
        
        // 2. หา Player
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
    }
    
    // ... (ส่วนอื่นๆ ของ Boss AI จะตามมาทีหลัง)
}