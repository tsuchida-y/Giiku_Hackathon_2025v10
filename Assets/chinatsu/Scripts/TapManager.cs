using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TapManager : MonoBehaviour
{
    public LayerMask fireworkMask; // Firework レイヤーを指定
    public bool debugLogs = true;
    Camera cam;

    void Awake()
    {
        cam = Camera.main ?? FindAnyObjectByType<Camera>();
        if (debugLogs)
        {
            Debug.Log($"[Tap] Awake. cam={(cam? cam.name : "NULL")}");
            if (fireworkMask.value == 0) Debug.LogWarning("[Tap] fireworkMask が未設定（0）。全レイヤーに当たる/期待通り当たらない可能性");
        }
    }

    void Update()
    {
        // マウス
        if (Input.GetMouseButtonDown(0))
        {
            if (debugLogs) Debug.Log("[Tap] MouseDown");
            if (IsOverUI()) { if (debugLogs) Debug.Log("[Tap] PointerOverUI=true -> 無視"); return; }
            TryPick(Input.mousePosition);
        }

        // タッチ
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            var t = Input.touches[0];
            if (debugLogs) Debug.Log("[Tap] Touch Began");
            if (IsOverUI(t.fingerId)) { if (debugLogs) Debug.Log("[Tap] PointerOverUI(true) -> 無視"); return; }
            TryPick(t.position);
        }
    }

    bool IsOverUI(int fingerId = -1)
    {
        if (EventSystem.current == null) { if (debugLogs) Debug.LogWarning("[Tap] EventSystem がない"); return false; }
        return (fingerId >= 0) ? EventSystem.current.IsPointerOverGameObject(fingerId)
                               : EventSystem.current.IsPointerOverGameObject();
    }

    void TryPick(Vector3 screenPos)
    {
        if (!cam) { if (debugLogs) Debug.LogError("[Tap] Camera が見つからない (MainCamera タグ?)"); return; }

        var ray = cam.ScreenPointToRay(screenPos);
        Debug.DrawRay(ray.origin, ray.direction * 50f, Color.cyan, 1f);
        if (debugLogs) Debug.Log($"[Tap] Ray from {screenPos}");

        // まずはマスク付き
        if (Physics.Raycast(ray, out var hit, 500f, fireworkMask))
        {
            if (debugLogs) Debug.Log($"[Tap] Raycast(Hit w/ mask): {hit.collider.name}, layer={LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            HandleHit(hit);
            return;
        }
        else
        {
            if (debugLogs) Debug.Log("[Tap] Raycast miss (with mask).");
        }

        // マスク無しで当たるか確認（原因切り分け用）
        if (Physics.Raycast(ray, out hit, 500f))
        {
            Debug.LogWarning($"[Tap] Raycast hits something WITHOUT mask: {hit.collider.name}, layer={LayerMask.LayerToName(hit.collider.gameObject.layer)}");
        }
        else
        {
            Debug.LogWarning("[Tap] Raycast hits NOTHING (no mask). Collider 未設定の可能性大。");
        }
    }

    void HandleHit(RaycastHit hit)
    {
        var controller = hit.collider.GetComponentInParent<FireworkController>();
        if (!controller)
        {
            Debug.LogWarning("[Tap] FireworkController が取れない（レイヤは合ってるが親に付いてない？）");
            return;
        }

        // 投稿データを取得して保存
        var postData = controller.GetPostData();
        if (postData == null)
        {
            Debug.LogError("[Tap] GetPostData() が null を返しました");
            return;
        }
        
        // データの内容を検証
        Debug.Log($"[Tap] 花火から取得した投稿データ: UserName={postData.UserName}, Message={postData.Message?.Substring(0, Mathf.Min(20, postData.Message?.Length ?? 0))}..., ID={postData.PostID}");
        
        SelectedPost.Current = postData;
        SelectedPost.LastTappedFirework = controller.gameObject;
        Debug.Log($"[Tap] OK -> {controller.gameObject.name} を選択。detailpostScene へ遷移");
        SceneManager.LoadScene("detailpostScene");
    }
}
