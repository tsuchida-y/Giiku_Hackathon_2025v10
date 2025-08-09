using UnityEngine;
using UnityEngine.EventSystems;

public class TapManager : MonoBehaviour
{
    public LayerMask fireworkMask;
    Camera cam;

    void Awake() { cam = Camera.main ?? FindAnyObjectByType<Camera>(); }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;
            TryPick(Input.mousePosition);
        }
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            var t = Input.touches[0];
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject(t.fingerId)) return;
            TryPick(t.position);
        }
    }

    void TryPick(Vector3 screenPos)
    {
        if (!cam) return;
        var ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit, 500f,
            (fireworkMask.value == 0) ? Physics.DefaultRaycastLayers : fireworkMask.value))
        {
            Debug.Log("Hit: " + hit.collider.name);
        }
    }
}
