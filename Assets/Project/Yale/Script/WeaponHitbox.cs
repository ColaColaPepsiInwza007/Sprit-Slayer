using UnityEngine;
using System.Collections.Generic; 

public class WeaponHitbox : MonoBehaviour
{
    public Collider weaponCollider; 
    private List<Collider> targetsHit;

    private void Awake()
    {
        if (weaponCollider == null)
        {
            weaponCollider = GetComponent<Collider>();
        }
        weaponCollider.enabled = false; 
        targetsHit = new List<Collider>();
    }

    public void OpenHitbox() 
    { 
        targetsHit.Clear(); 
        weaponCollider.enabled = true; 
    }
    
    public void CloseHitbox() 
    { 
        weaponCollider.enabled = false; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) 
        {
            if (targetsHit.Contains(other))
            {
                return; 
            }
            
            targetsHit.Add(other);
            
            Debug.Log("ฟันโดน: " + other.name);
            // (เดี๋ยวอนาคตเราจะเรียก... other.GetComponent<EnemyStats>().TakeDamage(10);)
        }
    }
}