using Firebase.Firestore;
using Firebase.Extensions;
using System.Threading.Tasks;
using UnityEngine;

public class FirestoreManager : MonoBehaviour
{
    // Firestoreデータベースのインスタンス
    private FirebaseFirestore db;
    // シングルトンパターン（インスタンスを1つに限定）のためのプロパティ
    public static FirestoreManager Instance { get; private set; }

    // スクリプトが最初にアクティブになったときに呼ばれる
    void Awake()
    {
        // インスタンスがまだ存在しない場合
        if (Instance == null)
        {
            // このスクリプトのインスタンスを唯一のものとして設定
            Instance = this;
            // Firebase Firestoreのデフォルトインスタンスを取得
            db = FirebaseFirestore.DefaultInstance;
        }
        else
        {
            // すでに存在する場合は、重複を避けるために破棄
            Destroy(gameObject);
        }
    }

    // 投稿IDを基に、Firebaseからデータを非同期で取得するメソッド
    // asyncとTaskを使うことで、通信が終わるまで他の処理をブロックせずに待機できる
    public async Task<PostData> GetPostDataAsync(string postId)
    {
        // "messages"コレクション内の、指定されたpostIdを持つドキュメントへの参照を取得
        DocumentReference docRef = db.Collection("messages").Document(postId);
        // ドキュメントの最新のスナップショットを非同期で取得
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        // スナップショットが存在するか（ドキュメントが見つかったか）をチェック
        if (snapshot.Exists)
        {
            // データをキーと値のペアを持つDictionaryに変換
            var data = snapshot.ToDictionary();

            // PostDataクラスのインスタンスを作成
            PostData postData = new PostData
            {
                // ドキュメントIDをPostIDとして設定
                PostID = snapshot.Id,                 // Dictionaryに"message"キーが存在するかチェックし、値を取得
                // 存在しない場合は空文字列を返す
                Message = data.ContainsKey("message") ? data["message"].ToString() : "",
                // 同様に"name"キーの値を取得
                UserName = data.ContainsKey("name") ? data["name"].ToString() : "",
                // "LikeCount"はFirestoreにないため、暫定的に0を設定
                LikeCount = 0,
                // "timestamp"キーの値を取得
                Timestamp = data.ContainsKey("timestamp") ? data["timestamp"].ToString() : ""
            };

            // ログに成功メッセージを出力
            Debug.Log($"Successfully fetched post data for postId: {postId}");
            // 作成したPostDataオブジェクトを返す
            return postData;
        }
        else
        {
            // ドキュメントが見つからない場合はエラーログを出力
            Debug.LogError($"Post not found for postId: {postId}");
            // nullを返す
            return null;
        }
    }
}