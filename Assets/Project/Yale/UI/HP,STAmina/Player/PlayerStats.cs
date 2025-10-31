using UnityEngine;
using UnityEngine.UI; 

public class PlayerStats : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxStamina = 100f;
    public float currentStamina;

    // <<< NEW >>>
    [Header("State")]
    public bool isInvincible = false; // นี่คือสวิตช์ i-frame

    [Header("UI References")]
    public Slider healthBar;
    public Slider staminaBar;

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        UpdateHealthBar();
        UpdateStaminaBar();
    }

    // --- ฟังก์ชันสำหรับอัปเดต UI ---
    
    public void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    public void UpdateStaminaBar()
    {
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }
    }

    // <<< NEW >>>
    // --- ฟังก์ชันรับดาเมจ (เปิดใช้งาน) ---
    public void TakeDamage(float amount)
    {
        // นี่คือหัวใจของ i-frame!!!
        if (isInvincible)
        {
            Debug.Log("DODGED! (i-frame active)");
            return; // ถ้าอมตะอยู่ ให้เมินดาเมจนี้ไปเลย
        }

        Debug.Log("Player took " + amount + " damage!");
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // ไม่ให้เลือดต่ำกว่า 0
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            // (ใส่โลจิกตอนตายตรงนี้)
            Debug.Log("Player is DEAD");
        }
    }
}