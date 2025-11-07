using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;   // << สำคัญ

public class DoorPortal : MonoBehaviour
{
    public string targetScene = "NextScene";
    public GameObject promptUI;
    public bool requirePlayerTag = true;
    public string playerTag = "Player";
    bool inRange;

    void Start()
    {
        if (promptUI) promptUI.SetActive(false);
    }

    void Update()
    {
        // ใช้ New Input System เท่านั้น
        if (!inRange || Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(targetScene);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (requirePlayerTag && !other.CompareTag(playerTag)) return;
        inRange = true;
        if (promptUI) promptUI.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (requirePlayerTag && !other.CompareTag(playerTag)) return;
        inRange = false;
        if (promptUI) promptUI.SetActive(false);
    }
}
