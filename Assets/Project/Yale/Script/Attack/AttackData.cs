using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "Player/Moveset/Attack")]
public class AttackData : ScriptableObject
{
    // (1. ข้อมูลอนิเมชั่น)
    public string animationName; // (เช่น "LA1" หรือ "metarig_LightAttack2Action")
    public float transitionTime = 0.1f; // (ความเร็วในการ CrossFade)

    // (2. ข้อมูล Game Feel)
    public float staminaCost = 10f;
    public float damageMultiplier = 1.0f;
    public float poiseDamage = 10f;

    // (3. *** นี่คือ "หัวใจ" ของ Chain Animation! ***)
    public AttackData nextLightAttack; // (ถ้ากด "ตีเบา" ต่อ... จะไปท่าไหน?)
    public AttackData nextHeavyAttack; // (ถ้ากด "ตีหนัก" ต่อ... จะไปท่าไหน?)
    public AttackData rollAttack;      // (ถ้า "กลิ้ง" มา... จะไปท่าไหน?)
    public AttackData backstepAttack;  // (ถ้า "ถอย" มา... จะไปท่าไหน?)
    
    // (4. จังหวะ Event (แทนการนั่งจิ้มเอง))
    public float openComboWindowFrame; // (เช่น 0.6 วินาที)
    public float openHitboxFrame;
    public float closeHitboxFrame;
    public float finishAttackFrame;
}