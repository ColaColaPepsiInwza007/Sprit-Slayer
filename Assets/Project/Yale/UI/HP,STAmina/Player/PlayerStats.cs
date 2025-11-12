using UnityEngine;
using UnityEngine.UI; 

// (*** 🚀 ไฟล์อัปเดต! (ย้าย Logic 'Stamina Regen' มาไว้ที่นี่) 🚀 ***)

public class PlayerStats : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject hpBarObject; 
    [SerializeField] private GameObject staminaBarObject; 

    private Slider hpBar;
    private Slider staminaBar;

    [Header("Flags")]
    public bool isInvincible = false;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxStamina = 100f;
    public float currentStamina;

    [Header("Stamina Regen")]
    [SerializeField] private float staminaRegenRate = 20f;   
    [SerializeField] private float staminaRegenDelay = 1.5f;
    private float staminaRegenTimer = 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        
        if (hpBarObject != null)
        {
            hpBar = hpBarObject.GetComponentInChildren<Slider>();
            if (hpBar != null)
            {
                hpBar.maxValue = maxHealth;
                hpBar.value = currentHealth;
            }
        }
        if (staminaBarObject != null)
        {
            staminaBar = staminaBarObject.GetComponentInChildren<Slider>();
            if (staminaBar != null)
            {
                staminaBar.maxValue = maxStamina;
                staminaBar.value = currentStamina;
            }
        }
    }

    public void HandleStaminaRegen(float delta)
    {
        staminaRegenTimer += delta; 
            
        if (staminaRegenTimer >= staminaRegenDelay && currentStamina < maxStamina)
        {
            // (*** ❗️ บรรทัดนี้จะ "หาย" แดง... ❗️ ***)
            RegenerateStamina(staminaRegenRate * delta); 
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible)
        {
            Debug.Log("PLAYER: I-Frame Dodge!");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 
        Debug.Log("PLAYER: โดนตี! เหลือเลือด " + currentHealth);

        if (hpBar != null)
        {
            hpBar.value = currentHealth; 
        }
        
        if (currentHealth <= 0) { /* Die */ }
    }

    public void UseStamina(float cost)
    {
        currentStamina -= cost;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); 
        
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina; 
        }
        
        staminaRegenTimer = 0f;
    }

    // (*** ❗️❗️❗️ ...เพราะ "ฟังก์ชันนี้" มันอยู่ที่นี่ครับ! ❗️❗️❗️ ***)
    public void RegenerateStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Min(currentStamina, maxStamina); 
        
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina; 
        }
    }
    // (*** ❗️❗️❗️ -------------------------------- ❗️❗️❗️ ***)


    public bool HasEnoughStamina(float cost)
    {
        return (currentStamina >= cost);
    }
}