using UnityEngine;
using UnityEngine.EventSystems;

public class LookAround : MonoBehaviour
{
    public float sensitivity = 2.0f;
    public float touchSensitivity = 10.0f;  // タッチ操作の感度を上げました
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    
    private Vector2 touchStartPosition;
    private Vector2 touchEndPosition;
    private bool isTouching = false;

    void Start()
    {
        // PCの場合のみカーソルをロック
        #if !UNITY_ANDROID && !UNITY_IOS
        Cursor.lockState = CursorLockMode.Locked;
        #endif
    }

    void Update()
    {
        // PCの場合はマウス操作
        #if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseLook();
        #endif

        // モバイルの場合はタッチ操作
        #if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        HandleTouchLook();
        #endif
        
        // 共通の視点更新処理
        UpdateCameraRotation();
    }
    
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
        
        rotationY += mouseX;
        rotationX -= mouseY;
    }
    
    void HandleTouchLook()
    {
        // UIを操作中の場合はカメラを動かさない
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
            
        // タッチ入力がある場合
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isTouching = true;
                    touchStartPosition = touch.position;
                    break;
                    
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isTouching)
                    {
                        touchEndPosition = touch.position;
                        
                        // スワイプ量を計算（deltaTimeを削除してより直接的な応答に）
                        float deltaX = (touchEndPosition.x - touchStartPosition.x) * touchSensitivity* 0.1f;
                        float deltaY = (touchEndPosition.y - touchStartPosition.y) * touchSensitivity* 0.1f;
                        
                        // 方向を修正（deltaXは正、deltaYは逆方向に）
                        rotationY -= deltaX;
                        rotationX += deltaY; // マイナスからプラスに変更
                        
                        // スワイプの開始位置を更新
                        touchStartPosition = touch.position;
                    }
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isTouching = false;
                    break;
            }
        }
    }
    
    void UpdateCameraRotation()
    {
        // 上下の視点移動を制限
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        
        // カメラの回転を更新
        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
    }
}