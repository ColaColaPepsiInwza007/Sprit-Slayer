using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelGoal_AllTargets : MonoBehaviour
{
    public TargetHoldReceiver[] targets;
    public float confirmSeconds = 0.0f;
    public UnityEvent onLevelComplete;

    [Header("Load scene อัตโนมัติ (ออปชัน)")]
    public bool autoLoadNextScene = true;          // true = โหลดฉากถัดไปตาม Build Index
    public string loadSceneByName = "";            // ถ้าไม่ว่าง จะโหลดตามชื่อนี้แทน

    float _t; bool _fired;

    void Start()
    {
        if (targets == null || targets.Length == 0)
            targets = FindObjectsOfType<TargetHoldReceiver>(false);
    }

    void Update()
    {
        if (_fired || targets == null || targets.Length == 0) return;

        bool allHeld = targets.All(t => t && t.IsHeld);
        if (allHeld)
        {
            _t += Time.deltaTime;
            if (_t >= confirmSeconds)
            {
                _fired = true;
                onLevelComplete?.Invoke();

                if (autoLoadNextScene || !string.IsNullOrEmpty(loadSceneByName))
                {
                    if (!string.IsNullOrEmpty(loadSceneByName))
                        SceneManager.LoadScene(loadSceneByName);
                    else
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
            }
        }
        else _t = 0f;
    }
}
