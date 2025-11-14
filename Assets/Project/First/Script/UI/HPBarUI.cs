using UnityEngine;
using UnityEngine.UI; // ต้องใช้ Library UI

public class HPBarUI : MonoBehaviour
{
    [Header("Health Bar References")] //Image
    public Slider healthSlider; 
    
    // Optional: Text Component
    public Text healthText; 

    // ฟังก์ชันนี้ถูกเรียกโดย BossManager.cs
    public void UpdateHealthBar(float currentHP, float maxHP)
    {
        // 1. คำนวณเปอร์เซ็นต์ของเลือด
        float fillAmount = currentHP / maxHP;

        // 2. ป้องกัน NullReferenceException และกำหนดค่า Slider
        if (healthSlider != null)
        {
            // กำหนดค่า Value ให้กับ Slider (ค่าจะอยู่ระหว่าง 0 ถึง 1)
            // หมายเหตุ: ต้องตั้งค่า Min Value ของ Slider ใน Inspector เป็น 0 และ Max Value เป็น 1
            healthSlider.value = fillAmount;
        } 
        else 
        {
            Debug.LogError("HPBarUI: healthSlider is NOT assigned! Cannot update health bar.");
        }
        
        // 3. อัปเดตตัวเลข HP (ถ้ามี)
        if (healthText != null)
        {
            healthText.text = currentHP.ToString("F0") + " / " + maxHP.ToString("F0"); 
        }
    }
}