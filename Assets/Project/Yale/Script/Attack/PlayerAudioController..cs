using UnityEngine;
using System.Collections.Generic;

// ใส่บน GameObject ของ Player (ติดกับ PlayerManager)
[RequireComponent(typeof(AudioSource))]
public class PlayerAudioController : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Attack Audio Clips")]
    // ❗️ List สำหรับใส่เสียง Whoosh/Swing (LA1, LA2, LA3, LA4...)
    public List<AudioClip> swingVFXClips; 

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // ถ้าไม่มี AudioSource ให้เพิ่มเข้าไป
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    // ❗️ ฟังก์ชันนี้จะถูกเรียกจาก Animation Event ❗️
    // Parameter 'clipIndex' จะเป็นตัวบอกว่ากำลังตีท่าที่เท่าไหร่
    public void PlaySwingVFX(int clipIndex)
    {
        if (clipIndex < 0 || clipIndex >= swingVFXClips.Count)
        {
            Debug.LogWarning($"PlayerAudioController: ไม่พบเสียง Swing VFX สำหรับ Index {clipIndex}.");
            return;
        }

        // เล่นเสียงแค่ครั้งเดียว (PlayOneShot)
        audioSource.PlayOneShot(swingVFXClips[clipIndex]);
    }
}