using UnityEngine;
using UnityEngine.SceneManagement; // ❗️ ต้องเพิ่มบรรทัดนี้

public class MainMenuManager : MonoBehaviour
{
    // 1. สร้างช่องสำหรับลาก Panel Settings มาใส่
    [SerializeField] private GameObject settingsPanel;

    // --- ฟังก์ชันสำหรับปุ่ม Play ---
    public void PlayGame()
    {
        Debug.Log("Starting Game...");
        
        // ❗️ เปลี่ยน "YourGameSceneName" เป็นชื่อ Scene เกมของคุณ
        SceneManager.LoadScene("First"); 
    }

    // --- ฟังก์ชันสำหรับปุ่ม Settings ---
    public void OpenSettings()
    {
        Debug.Log("Opening Settings...");
        settingsPanel.SetActive(true); // เปิดหน้าต่าง Settings
    }

    public void CloseSettings()
    {
        Debug.Log("Closing Settings...");
        settingsPanel.SetActive(false); // ปิดหน้าต่าง Settings
    }

    // --- ฟังก์ชันสำหรับปุ่ม Leave/Quit ---
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();

        // (โค้ดพิเศษสำหรับให้ปุ่ม Quit ทำงานใน Editor ด้วย)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}