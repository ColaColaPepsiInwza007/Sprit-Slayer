using UnityEngine;
using UnityEngine.UI; // ต้องเรียกใช้ Library นี้
// ต้องนำเข้า System.Collections.Generic ด้วย ถ้าต้องการใช้ List
// using System.Collections.Generic; 

public class HPBarUI : MonoBehaviour
{
    // อ้างอิงถึง Image Component ของแถบเลือด (HP_Fill)
    public Image fillImage;

    // ฟังก์ชันสำหรับอัปเดตแถบเลือด (เรียกใช้ทุกครั้งที่เลือดบอสเปลี่ยน)
    // currentHP คือ เลือดปัจจุบันของบอส
    // maxHP คือ เลือดสูงสุดของบอส
    public void UpdateHealthBar(float currentHP, float maxHP)
    {
        // คำนวณเปอร์เซ็นต์ของเลือด (ค่าระหว่าง 0.0 ถึง 1.0)
        float fillAmount = currentHP / maxHP;

        // กำหนดค่า Fill Amount ให้กับ Image Component
        fillImage.fillAmount = fillAmount;
    }
}