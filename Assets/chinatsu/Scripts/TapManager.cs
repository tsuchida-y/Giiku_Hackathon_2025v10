using UnityEngine;
using UnityEngine.SceneManagement; // シーン管理のためのライブラリ
using System.Threading.Tasks;

// タップを管理するクラス
public class TapManager : MonoBehaviour
{
    // Update関数は毎フレーム呼ばれる
    void Update()
    {
        // マウスの左ボタンが押された瞬間（またはモバイルで指が触れた瞬間）を判定
        if (Input.GetMouseButtonDown(0))
        {
            // メインカメラから、タップした画面座標へ向かうRay（光線）を生成
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Rayを飛ばし、何かのコライダーに当たったかどうかを判定
            if (Physics.Raycast(ray, out hit))
            {
                // Rayが当たったオブジェクトからFireworkTriggerコンポーネントを取得
                FireworkTrigger trigger = hit.collider.GetComponent<FireworkTrigger>();

                // もしコンポーネントが見つかれば、それは花火のコライダー
                if (trigger != null)
                {
                    // FireworkTriggerから投稿IDを取得
                    string postId = trigger.PostID;

                    // 非同期でFirebaseから投稿データを取得するメソッドを呼び出す
                    GetAndDisplayPostData(postId);
                }
            }
        }
    }

    // 投稿データを非同期で取得し、表示するプライベートメソッド
    // asyncキーワードを付けて、このメソッド内でawaitが使えるようにする
    private async void GetAndDisplayPostData(string postId)
    {
        // FirestoreManagerのインスタンスがnullでないか（シーンに存在するか）を確認
        if (FirestoreManager.Instance != null)
        {
            // FirestoreManagerのGetPostDataAsyncを呼び出し、投稿データを非同期で待つ
            PostData post = await FirestoreManager.Instance.GetPostDataAsync(postId);

            // 投稿データがnullでなければ
            if (post != null)
            {
                // 1. 取得した投稿データを静的クラスに一時的に保存
                PostDataHolder.currentPost = post;

                // 2. 投稿詳細画面のシーンに遷移
                SceneManager.LoadScene("detailpostScene");
            }
        }
        else
        {
            // FirestoreManagerが見つからない場合はエラーログを表示
            Debug.LogError("FirestoreManagerのインスタンスが見つかりません。");
        }
    }
}