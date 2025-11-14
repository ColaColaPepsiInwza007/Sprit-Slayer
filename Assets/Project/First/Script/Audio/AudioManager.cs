using UnityEngine;
using System; // ❗️ ต้องมี

public class AudioManager : MonoBehaviour
{
    // ❗️ 2. ตัวแปร Singleton
    public static AudioManager instance;

    // ❗️ 3. Array ของเสียงที่เราตั้งค่าไว้ใน Sound.cs
    public Sound[] sounds;

    private void Awake()
    {
        // ❗️ 4. ตั้งค่า Singleton Pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // ถ้ามี AudioManager ตัวอื่นอยู่แล้ว ให้ทำลายตัวนี้ทิ้ง
            return;
        }

        // ทำให้ AudioManager คงอยู่ แม้จะเปลี่ยน Scene
        DontDestroyOnLoad(gameObject);

        // ❗️ 5. สร้าง AudioSource ให้กับทุกเสียง
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    // (เสริม) เล่นเสียงเพลงตอนเริ่มเกม
    private void Start()
    {
        Play("BackgroundMusic"); // (ถ้าคุณตั้งชื่อเสียงว่า "BackgroundMusic")
    }

    // ❗️ 6. ฟังก์ชันสำหรับ "เล่น" เสียง
    public void Play(string soundName)
    {
        // ค้นหาเสียงจากชื่อ
        Sound s = Array.Find(sounds, sound => sound.name == soundName);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return;
        }
        
        s.source.Play();
    }

    // ❗️ 7. ฟังก์ชันสำหรับ "หยุด" เสียง
    public void Stop(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return;
        }
        
        s.source.Stop();
    }
}