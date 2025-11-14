using UnityEngine;
using UnityEngine.UI; // ❗️ ต้องมีสำหรับ UI

public class BossStanceBarUI : MonoBehaviour
{
    // ใช้ Slider หรือ Image ก็ได้ (ขึ้นอยู่กับว่าคุณใช้อะไร)
    // ถ้าใช้ Slider:
    public Slider stanceSlider;

    // ถ้าใช้ Image (แบบ Fill Amount):
    // public Image stanceFillImage; 

    private void Awake()
    {
        // ถ้าใช้ Slider
        if (stanceSlider == null)
        {
            stanceSlider = GetComponentInChildren<Slider>();
        }
        
        // ถ้าใช้ Image
        // if (stanceFillImage == null)
        // {
        //     // หาก Image อยู่ใน Child
        //     stanceFillImage = GetComponentInChildren<Image>(); 
        // }
    }

    // ฟังก์ชันสำหรับอัปเดต UI Bar
    public void UpdateStanceBar(float currentStance, float maxStance)
    {
        float stanceRatio = currentStance / maxStance;

        // ถ้าใช้ Slider
        if (stanceSlider != null)
        {
            stanceSlider.value = stanceRatio;
        }

        // ถ้าใช้ Image
        // if (stanceFillImage != null)
        // {
        //     stanceFillImage.fillAmount = stanceRatio;
        // }
    }
}