using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;   // ??????????? New Input System
#endif

public class MirrorDragRotate : MonoBehaviour
{
    public LayerMask mirrorMask;       // ?????????????????????? Mirror (??? Default ???????????????????)
    public float rotateSpeed = 120f;   // ?????????????

    Transform active;

    void Update()
    {
        // ??????????????????????? "?????"
        if (MouseDown())
        {
            Ray ray = Camera.main.ScreenPointToRay(MousePos());
            if (Physics.Raycast(ray, out var hit, 1000f, mirrorMask))
                active = hit.transform;
        }

        // ??????????????????? => ????
        if (MouseHeld() && active)
        {
            float dx = MouseDeltaX();
            active.Rotate(Vector3.up, dx * rotateSpeed * Time.deltaTime, Space.World);
        }

        // ?????????? => ?????????
        if (MouseUp()) active = null;
    }

    // ---- Helpers ?????????????? Input ????/???? ----
    bool MouseDown()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    bool MouseHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.isPressed;
#else
        return Input.GetMouseButton(0);
#endif
    }

    bool MouseUp()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
#else
        return Input.GetMouseButtonUp(0);
#endif
    }

    float MouseDeltaX()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.delta.ReadValue().x : 0f;
#else
        return Input.GetAxis("Mouse X");
#endif
    }

    Vector2 MousePos()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }
}
