using System.Collections;//test用のコードに必要
using System.Collections.Generic;
using UnityEngine;
using System.Linq; //Linqを使うために追加
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Threading.Tasks;

public class FestivalManager : MonoBehaviour
{
    public FireworkSpawner spawner;

    // Firestore参照
    private FirebaseFirestore db;
    
    // 自動花火生成の設定
    [Header("自動花火生成設定")]
    public bool enableAutoFireworks = true;  // 自動生成の有効/無効
    public float fireworkInterval = 5.0f;    // 花火生成間隔（秒）
    public int numberOfFireworksPerInterval = 2; // 一度に生成する花火の数
    public float minFireworkDistance = 10.0f; // 花火同士の最小距離

    private Dictionary<string, FireworkController> activeFireworks = new Dictionary<string, FireworkController>();
    private List<PostData> allPosts = new List<PostData>();
    
    // Firebase投稿データのキャッシュ
    private List<PostData> firebasePosts = new List<PostData>();

    // ★★★ 追加 ★★★
    // このクラスが起動したときに一度だけ呼ばれる
    void Start()
    {
        // Firebaseを初期化
        InitializeFirebase();
        
        // 5分おきに古い花火を掃除する、見回りパトロールを開始する
        StartCoroutine(CleanupCoroutine());
        
        // 自動花火生成が有効な場合、定期的に花火を生成するコルーチンを開始
        if (enableAutoFireworks)
        {
            StartCoroutine(AutoFireworkGenerationCoroutine());
        }
    }
    
    // Firebaseの初期化処理
    private async void InitializeFirebase()
    {
        // Firebase依存関係を確認
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Firestoreのインスタンスを取得
                db = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firebaseの初期化が完了しました");
                
                // 初回のデータ取得
                FetchFirestorePostsAsync();
            }
            else
            {
                Debug.LogError($"Firebase依存関係の解決に失敗しました: {dependencyStatus}");
            }
        });
    }
    
    // Firestoreから投稿データを取得する非同期メソッド
    private async void FetchFirestorePostsAsync()
    {
        if (db == null)
        {
            Debug.LogError("Firestoreが初期化されていません");
            return;
        }
        
        try
        {
            // messagesコレクションからすべてのデータを取得
            QuerySnapshot snapshot = await db.Collection("messages")
                .OrderByDescending("timestamp")
                .GetSnapshotAsync();
                
            // 取得したデータを変換してキャッシュに保存
            firebasePosts.Clear();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Dictionary<string, object> postData = document.ToDictionary();
                
                // 必要なデータを取得
                string postId = document.Id;
                
                // PostDataオブジェクトを作成
                PostData post = new PostData
                {
                    PostID = postId,
                    Timestamp = System.DateTime.UtcNow // 適切な変換が必要
                };
                
                // ユーザー名を取得
                if (postData.TryGetValue("name", out object nameObj))
                {
                    post.UserName = nameObj?.ToString() ?? "匿名";
                }
                else
                {
                    post.UserName = "匿名";
                }
                
                // 投稿内容を取得
                if (postData.TryGetValue("message", out object messageObj))
                {
                    post.Message = messageObj?.ToString() ?? "";
                }
                else
                {
                    post.Message = "";
                }
                
                // いいね数を取得
                if (postData.TryGetValue("likes", out object likesObj))
                {
                    post.LikeCount = System.Convert.ToInt32(likesObj);
                }
                
                // RGB投票を取得
                if (postData.TryGetValue("redVotes", out object redObj))
                {
                    post.RedVotes = System.Convert.ToInt32(redObj);
                }
                if (postData.TryGetValue("greenVotes", out object greenObj))
                {
                    post.GreenVotes = System.Convert.ToInt32(greenObj);
                }
                if (postData.TryGetValue("blueVotes", out object blueObj))
                {
                    post.BlueVotes = System.Convert.ToInt32(blueObj);
                }
                
                // デバッグ用に投稿内容を表示
                Debug.Log($"投稿データ取得: ID={post.PostID}, Name={post.UserName}, Message={post.Message?.Substring(0, Mathf.Min(20, post.Message?.Length ?? 0))}..., Likes={post.LikeCount}");
                
                // その他の必要なデータがあれば取得
                
                // リストに追加
                firebasePosts.Add(post);
            }
            
            Debug.Log($"Firestoreから{firebasePosts.Count}件の投稿を取得しました");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Firestoreからのデータ取得に失敗しました: {ex.Message}");
        }
    }
    
    // 定期的に花火を生成するコルーチン
    private IEnumerator AutoFireworkGenerationCoroutine()
    {
        while (true)
        {
            // 設定された間隔で待機
            yield return new WaitForSeconds(fireworkInterval);
            
            // Firestoreからデータを再取得
            FetchFirestorePostsAsync();
            
            // 少し待ってデータ取得が完了するのを待つ
            yield return new WaitForSeconds(0.5f);
            
            // ランダムに投稿を選択して花火を生成
            GenerateRandomFireworks();
        }
    }
    
    // ランダムに投稿を選択して花火を生成するメソッド
    private void GenerateRandomFireworks()
    {
        // 投稿がない場合は処理をスキップ
        if (firebasePosts.Count == 0)
        {
            Debug.Log("生成可能な投稿データがありません");
            return;
        }
        
        // シャッフルしたリストを作成
        List<PostData> shuffledPosts = new List<PostData>(firebasePosts);
        for (int i = 0; i < shuffledPosts.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledPosts.Count);
            PostData temp = shuffledPosts[i];
            shuffledPosts[i] = shuffledPosts[randomIndex];
            shuffledPosts[randomIndex] = temp;
        }
        
        // 指定された数の花火を生成（ただし、利用可能な投稿数を超えないように）
        int count = Mathf.Min(numberOfFireworksPerInterval, shuffledPosts.Count);
        for (int i = 0; i < count; i++)
        {
            // 投稿から花火を生成
            OnNewPostReceived(shuffledPosts[i]);
        }
    }
    
    // デバッグ用の機能（Firestoreが設定されていない場合に使用）
    void Update()
    {
        // デバッグモード：スペースキーを押すとランダムな花火が生成される
        if (Input.GetKeyDown(KeyCode.Space) && firebasePosts.Count == 0)
        {
            GenerateDebugFireworks();
        }
        
        // デバッグモード：Tキーでテスト用のデータをキャッシュに追加
        if (Input.GetKeyDown(KeyCode.T) && firebasePosts.Count == 0)
        {
            AddDebugPostData();
        }
    }
    
    // デバッグ用：ランダムな色の花火を生成
    private void GenerateDebugFireworks()
    {
        for (int i = 0; i < numberOfFireworksPerInterval; i++)
        {
            // ランダムな色の比率を生成
            int red = Random.Range(1, 10);
            int green = Random.Range(1, 10);
            int blue = Random.Range(1, 10);
            
            PostData debugPost = new PostData
            {
                PostID = "debug_" + System.Guid.NewGuid().ToString(),
                Timestamp = System.DateTime.UtcNow,
                LikeCount = Random.Range(0, 20),
                RedVotes = red,
                GreenVotes = green,
                BlueVotes = blue
            };
            
            OnNewPostReceived(debugPost);
        }
        
        Debug.Log("デバッグモード：ランダムな花火を生成しました");
    }
    
    // デバッグ用：テストデータを追加
    private void AddDebugPostData()
    {
        firebasePosts.Clear();
        
        // 10個のテストデータを作成
        for (int i = 0; i < 10; i++)
        {
            int red = Random.Range(1, 10);
            int green = Random.Range(1, 10);
            int blue = Random.Range(1, 10);
            
            PostData debugPost = new PostData
            {
                PostID = "debug_" + i,
                Timestamp = System.DateTime.UtcNow,
                LikeCount = Random.Range(0, 20),
                RedVotes = red,
                GreenVotes = green,
                BlueVotes = blue
            };
            
            firebasePosts.Add(debugPost);
        }
        
        Debug.Log("デバッグモード：10個のテストデータを追加しました");
    }
    // 新しい投稿があった時に呼び出される
    public void OnNewPostReceived(PostData newPost)
    {
        if ((System.DateTime.UtcNow - newPost.Timestamp).TotalHours >= 24) return;
        if (!activeFireworks.ContainsKey(newPost.PostID))
        {
            FireworkController newFirework = spawner.SpawnFirework(newPost, this);
            if (newFirework != null)
            {
                activeFireworks.Add(newPost.PostID, newFirework);
                allPosts.Add(newPost);
            }
        }
    }

    // 最新のいいね数を返す
    public int GetLikesForPost(string postID)
    {
        PostData post = allPosts.Find(p => p.PostID == postID);
        if (post != null)
        {
            post.LikeCount += Random.Range(0, 5);
            return post.LikeCount;
        }
        return 0;
    }

    // 定期的に古い投稿（花火）を削除するためのコルーチン
    private IEnumerator CleanupCoroutine()
    {
        // 無限ループ
        while (true)
        {
            // 5分待機（300秒）
            yield return new WaitForSeconds(300f);

            Debug.Log("古い花火がないか見回り");

            // 削除対象の投稿IDを一時的に保存するリスト
            List<string> postsToRemove = new List<string>();

            // 現在アクティブな全ての投稿をチェック
            foreach (var fireworkEntry in activeFireworks)
            {
                PostData post = fireworkEntry.Value.GetPostData();
                // 投稿から24時間以上経過しているかチェック
                if ((System.DateTime.UtcNow - post.Timestamp).TotalHours >= 24)
                {
                    // 経過していたら、削除リストに追加
                    postsToRemove.Add(post.PostID);
                }
            }

            // 削除対象のものを、リストから実際に削除していく
            foreach (string postID in postsToRemove)
            {
                Debug.Log(postID + " は24時間以上経過したため、削除します。");
                activeFireworks[postID].Despawn(); // 花火オブジェクトに消滅を命令
                activeFireworks.Remove(postID); // 辞書から削除
            }
            // allPostsリストからも削除
            allPosts.RemoveAll(p => postsToRemove.Contains(p.PostID));
        }
    }
}