using UnityEngine;

public class LookAround : MonoBehaviour
{
    public float sensitivity = 2.0f;
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        // カーソルをロックして見えなくする
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // 上下に見える範囲を制限

        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
    }
}