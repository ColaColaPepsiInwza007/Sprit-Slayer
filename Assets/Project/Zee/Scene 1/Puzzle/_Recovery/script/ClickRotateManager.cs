using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;   // <= ?????? ; ??????
#endif

public class ClickRotateManager : MonoBehaviour
{
    public LayerMask mirrorMask;   // ????????????????????
    public float step = 45f;

    void Update()
    {
        // ??????????? UI ?????????
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        bool pressed;
        Vector2 mpos;

#if ENABLE_INPUT_SYSTEM
        pressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        mpos    = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
        pressed = Input.GetMouseButtonDown(0);
        mpos = Input.mousePosition;
#endif

        if (!pressed) return;

        var ray = Camera.main.ScreenPointToRay(mpos);
        if (Physics.Raycast(ray, out var hit, 2000f, mirrorMask, QueryTriggerInteraction.Ignore))
        {
            // ???????????????????? Tag = "Mirror"
            Transform t = hit.transform;
            while (t != null && !t.CompareTag("Mirror")) t = t.parent;
            if (t == null) return;

            // ???? 45° ??? "?????????????" ?????????????? pivot ???? ?
            Vector3 keepPos = t.position;
            Quaternion keepRot = t.rotation;
            t.rotation = Quaternion.AngleAxis(step, Vector3.up) * keepRot;
            t.position = keepPos;
        }
    }
}
