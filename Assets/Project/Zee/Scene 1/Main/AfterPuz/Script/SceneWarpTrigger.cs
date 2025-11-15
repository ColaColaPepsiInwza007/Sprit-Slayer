using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneWarpTrigger : MonoBehaviour
{
    [Tooltip("ชื่อฉากปลายทาง ต้องตรงกับใน Build Settings")]
    public string targetSceneName = "Scene_Puzzle";

    [Header("ถ้าต้องการให้ยืนยันด้วยปุ่ม")]
    public bool requirePressKey = false;
    public KeyCode key = KeyCode.E;

    bool playerInside = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (requirePressKey)
            {
                playerInside = true;     // รอให้กดปุ่มใน Update
                // TODO: โชว์ UI 'Press E'
            }
            else
            {
                LoadTargetScene();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            // TODO: ซ่อน UI 'Press E'
        }
    }

    void Update()
    {
        if (requirePressKey && playerInside && Input.GetKeyDown(key))
        {
            LoadTargetScene();
        }
    }

    void LoadTargetScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}
