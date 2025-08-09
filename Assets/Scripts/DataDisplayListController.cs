using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Firebase.Firestore;
using Firebase.Extensions; // Firebaseの非同期処理をメインスレッドで実行するために必要
using System.Linq;

public class DataDisplayListController : MonoBehaviour
{
    [Header("References")]
    public Transform content;        // Scroll View / Viewport / Content
    public GameObject postItemPrefab;

    private FirebaseFirestore db;
    private List<Post> _posts = new();

    [Serializable]
    public class Post
    {
        public string id;
        public string owner;
        public string text;
        public long createdAtUnix;
        public int likes;
    }

    // JsonUtility用のラッパークラス（JSONデータから直接読み込む場合のみ使用）
    // Firestoreを使用する場合は不要だが、将来的にJSONからの読み込みに戻す可能性を考慮して残しておく
    [Serializable]
    class PostListWrapper { public Post[] posts; }

    void Start()
    {
        // Firebase Firestoreのインスタンスを初期化（シンプルに）
        db = FirebaseFirestore.DefaultInstance;

        // Firebase依存性を確認して修正
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            Firebase.DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Firebaseが使用可能な場合
                Debug.Log("Firebase初期化成功！(DataDisplayListController)");

                // Firebaseアプリが既に初期化されているか確認
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
                if (app == null)
                {
                    Debug.Log("Firebase App を作成します");
                    Firebase.FirebaseApp.Create();
                }

                // もう一度Firestoreを初期化
                db = FirebaseFirestore.DefaultInstance;

                // データを読み込む
                LoadFromFirestore();
            }
            else
            {
                Debug.LogError($"Firebase初期化に失敗: {dependencyStatus}");
            }
        });
    }

    // LoadFromFirestoreメソッドをpublicに変更
    public void LoadFromFirestore()
    {
        // Firestoreの初期化チェック
        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
            if (db == null)
            {
                Debug.LogError("Firestoreがまだ初期化されていません。");
                return;
            }
        }

        Debug.Log("Firestoreからデータを読み込みます...");

        // "messages"コレクションからデータを取得
        db.Collection("messages")
            .OrderByDescending("timestamp") // 新しい投稿順に並べる
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    _posts.Clear();
                    QuerySnapshot snapshot = task.Result;

                    Debug.Log($"Firestoreから{snapshot.Documents.Count()}件のデータを取得しました");

                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        Dictionary<string, object> postData = document.ToDictionary();

                        // Firestoreから取得したデータをPostオブジェクトに変換
                        Post post = new Post
                        {
                            id = document.Id,
                            owner = postData.ContainsKey("name") ? postData["name"].ToString() : "匿名",
                            text = postData.ContainsKey("message") ? postData["message"].ToString() : "",
                            createdAtUnix = postData.ContainsKey("timestamp")
                                ? GetUnixTimestamp((Timestamp)postData["timestamp"])
                                : GetCurrentUnixTimestamp(),
                            likes = postData.ContainsKey("likes") ? Convert.ToInt32(postData["likes"]) : 0
                        };

                        _posts.Add(post);
                    }

                    // 投稿リストを更新
                    BuildList();
                }
                else
                {
                    Debug.LogError("投稿データの取得に失敗しました: " + task.Exception);
                }
            });
    }

    void BuildList()
    {
        Debug.Log($"投稿リストを構築します: {_posts.Count()}件のデータ");

        // 既存の子オブジェクトをすべて削除
        foreach (Transform child in content) Destroy(child.gameObject);

        if (_posts.Count == 0)
        {
            Debug.Log("表示する投稿がありません");
            return;
        }

        // 各投稿に対してUIを生成
        foreach (var p in _posts)
        {
            var go = Instantiate(postItemPrefab, content);
            Debug.Log($"投稿アイテムを生成: {p.owner} - {p.text.Substring(0, Mathf.Min(20, p.text.Length))}...");
            Bind(go, p);
        }
    }

    void Bind(GameObject go, Post p)
    {
        // プレハブ内の子オブジェクト名をすべて出力（階層も含めて）
        Debug.Log("プレハブ内の子オブジェクト:");
        foreach (Transform child in go.transform)
        {
            Debug.Log($"- {child.name}");

            // さらに子オブジェクトがあれば表示
            foreach (Transform grandchild in child.transform)
            {
                Debug.Log($"  └── {grandchild.name}");

                // さらに子オブジェクトがあれば表示
                foreach (Transform greatGrandchild in grandchild.transform)
                {
                    Debug.Log($"      └── {greatGrandchild.name}");
                }
            }
        }
        var ownerText = go.transform.Find("OwnerText")?.GetComponent<TMP_Text>();
        var bodyText = go.transform.Find("BodyText")?.GetComponent<TMP_Text>();
        var timeText = go.transform.Find("TimeText")?.GetComponent<TMP_Text>();
        // ここを「ルートButton」に付け替える（GetComponentInChildrenではなくGetComponent）
        var rootBtn = go.GetComponent<Button>();

        // UIコンポーネントが見つからない場合のデバッグログ
        if (ownerText == null) Debug.LogError("OwnerTextが見つかりません");
        if (bodyText == null) Debug.LogError("BodyTextが見つかりません");
        if (timeText == null) Debug.LogError("TimeTextが見つかりません");
        if (rootBtn == null) Debug.LogError("Buttonが見つかりません");


        if (ownerText) ownerText.text = p.owner;
        if (bodyText)
        {
            int limit = 70;
            bodyText.text = (p.text.Length > limit) ? p.text.Substring(0, limit) + "…" : p.text;
        }
        if (timeText) timeText.text = UnixToJstString(p.createdAtUnix);

        if (rootBtn == null)
        {
            Debug.LogError("PostItem ルートに Button を追加してください（Image + Button）。");
        }
        else
        {
            rootBtn.onClick.RemoveAllListeners();
            rootBtn.onClick.AddListener(() =>
            {
                SelectedPost.Current = ToPostData(p);   // ← 変換してから渡す
                UnityEngine.SceneManagement.SceneManager.LoadScene("detailpostScene");
            });
        }
        // いいねボタンは今のままでOK（子に別ボタンがある場合、最前面の子ボタンが優先処理されます）
        // いいねボタン、テキストの設定（両方ともReactionPanelの子オブジェクト）
        var likeButton = go.transform.Find("ReactionPanel/LikeButton")?.GetComponent<Button>();
        var likeText = go.transform.Find("ReactionPanel/LikeText")?.GetComponent<TMP_Text>();


        // UIコンポーネントが見つからない場合のデバッグログ
        if (likeButton == null) Debug.LogError("ReactionPanel/LikeButtonが見つかりません");
        if (likeText == null) Debug.LogError("LikeTextが見つかりません");

        if (likeButton && likeText)
        {
            Debug.Log($"いいね数を設定します: {p.likes} いいね");
            likeText.text = p.likes.ToString() + " いいね";

            likeButton.onClick.RemoveAllListeners();
            likeButton.onClick.AddListener(() =>
            {
                LikePost(p.id);
            });
        }
    }

private PostData ToPostData(Post p)
{
    return new PostData
    {
        PostID = p.id,
        UserName = p.owner ?? "匿名",
        Message = p.text ?? "",
        // この行がエラーの原因なので削除します
        // LikeCount = p.likes, 
        Timestamp = DateTimeOffset.FromUnixTimeSeconds(p.createdAtUnix).UtcDateTime
    };
}

    // void Bind(GameObject go, Post p)
    // {
    //     // プレハブ内の子オブジェクト名をすべて出力（階層も含めて）
    //     Debug.Log("プレハブ内の子オブジェクト:");
    //     foreach (Transform child in go.transform)
    //     {
    //         Debug.Log($"- {child.name}");

    //         // さらに子オブジェクトがあれば表示
    //         foreach (Transform grandchild in child.transform)
    //         {
    //             Debug.Log($"  └── {grandchild.name}");

    //             // さらに子オブジェクトがあれば表示
    //             foreach (Transform greatGrandchild in grandchild.transform)
    //             {
    //                 Debug.Log($"      └── {greatGrandchild.name}");
    //             }
    //         }
    //     }

    //     var ownerText = go.transform.Find("OwnerText")?.GetComponent<TMP_Text>();
    //     var bodyText = go.transform.Find("BodyText")?.GetComponent<TMP_Text>();
    //     var timeText = go.transform.Find("TimeText")?.GetComponent<TMP_Text>();
    //     var button = go.GetComponentInChildren<Button>(true);

    //     // UIコンポーネントが見つからない場合のデバッグログ
    //     if (ownerText == null) Debug.LogError("OwnerTextが見つかりません");
    //     if (bodyText == null) Debug.LogError("BodyTextが見つかりません");
    //     if (timeText == null) Debug.LogError("TimeTextが見つかりません");
    //     if (button == null) Debug.LogError("Buttonが見つかりません");

    //     if (ownerText) ownerText.text = p.owner;
    //     if (bodyText)
    //     {
    //         int limit = 70; // 最大文字数（全角・半角問わず）
    //         if (p.text.Length > limit)
    //             bodyText.text = p.text.Substring(0, limit) + "…";
    //         else
    //             bodyText.text = p.text;
    //     }
    //     if (timeText) timeText.text = UnixToJstString(p.createdAtUnix);

    //     if (button)
    //     {
    //         button.onClick.RemoveAllListeners();
    //         button.onClick.AddListener(() =>
    //         {
    //             PostRoute.SelectedPostId = p.id;
    //             UnityEngine.SceneManagement.SceneManager.LoadScene("PostDetailScene");
    //         });
    //     }

    //     // いいねボタンの設定（ReactionPanelの子オブジェクト）
    //     var likeButton = go.transform.Find("ReactionPanel/LikeButton")?.GetComponent<Button>();
    //     // LikeTextはPostItemの直接の子オブジェクト
    //     var likeText = go.transform.Find("ReactionPanel/LikeText")?.GetComponent<TMP_Text>();

    //     // UIコンポーネントが見つからない場合のデバッグログ
    //     if (likeButton == null) Debug.LogError("ReactionPanel/LikeButtonが見つかりません");
    //     if (likeText == null) Debug.LogError("LikeTextが見つかりません");

    //     if (likeButton && likeText)
    //     {
    //         Debug.Log($"いいね数を設定します: {p.likes} いいね");
    //         likeText.text = p.likes.ToString() + " いいね";

    //         likeButton.onClick.RemoveAllListeners();
    //         likeButton.onClick.AddListener(() =>
    //         {
    //             LikePost(p.id);
    //         });
    //     }
    // }

    // いいね機能の実装
    void LikePost(string postId)
    {
        // ドキュメントの参照を取得
        DocumentReference docRef = db.Collection("messages").Document(postId);

        // トランザクションを使用していいね数を更新
        db.RunTransactionAsync(transaction =>
        {
            return transaction.GetSnapshotAsync(docRef)
                .ContinueWith(task =>
                {
                    DocumentSnapshot snapshot = task.Result;
                    Dictionary<string, object> data = snapshot.ToDictionary();

                    // 現在のいいね数を取得して+1する
                    long currentLikes = 0;
                    if (data.ContainsKey("likes"))
                    {
                        currentLikes = Convert.ToInt64(data["likes"]);
                    }

                    // いいね数を更新
                    Dictionary<string, object> updates = new Dictionary<string, object>
                    {
                        { "likes", currentLikes + 1 }
                    };

                    // ドキュメントを更新
                    transaction.Update(docRef, updates);

                    return true;
                });
        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("いいねを追加しました！");
                // 投稿リストを更新
                LoadFromFirestore();
            }
            else
            {
                Debug.LogError("いいねの追加に失敗しました: " + task.Exception);
            }
        });
    }

    string UnixToJstString(long unixSec)
    {
        var dt = DateTimeOffset.FromUnixTimeSeconds(unixSec).ToLocalTime().DateTime;
        return dt.ToString("yyyy/MM/dd HH:mm");
    }

    // Firebase TimestampからUnixタイムスタンプ（秒）を取得するヘルパーメソッド
    long GetUnixTimestamp(Timestamp timestamp)
    {
        // Unix Epoch（1970年1月1日）からの経過秒数を計算
        DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime dateTime = timestamp.ToDateTime();
        TimeSpan timeSpan = dateTime - unixEpoch;
        return (long)timeSpan.TotalSeconds;
    }

    // 現在時刻のUnixタイムスタンプ（秒）を取得するヘルパーメソッド
    long GetCurrentUnixTimestamp()
    {
        // Unix Epoch（1970年1月1日）からの経過秒数を計算
        DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime now = DateTime.UtcNow;
        TimeSpan timeSpan = now - unixEpoch;
        return (long)timeSpan.TotalSeconds;
    }

    public void OnHomeButtonClick()
    {
        Debug.Log("Home");
    }

    public void OnPostListButtonClick()
    {
        Debug.Log("PostList");
        UnityEngine.SceneManagement.SceneManager.LoadScene("postlistScene");
    }

    public void OnNewPostButtonClick()
    {
        // 遷移先に値を渡す（後述の方法を使う）
        PostEditData.TempPostText = "";
        PostEditData.TempPostOwner = "";

        // 新規投稿画面へ遷移
        UnityEngine.SceneManagement.SceneManager.LoadScene("newpostScene");
    }


}
