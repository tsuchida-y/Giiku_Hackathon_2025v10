using System.Collections;//test用のコードに必要
using System.Collections.Generic;
using UnityEngine;
using System.Linq; //Linqを使うために追加

public class FestivalManager : MonoBehaviour
{
    public FireworkSpawner spawner;

    private Dictionary<string, FireworkController> activeFireworks = new Dictionary<string, FireworkController>();
    private List<PostData> allPosts = new List<PostData>();

    // ★★★ 追加 ★★★
    // このクラスが起動したときに一度だけ呼ばれる
    void Start()
    {
        // 5分おきに古い花火を掃除する、見回りパトロールを開始する
        StartCoroutine(CleanupCoroutine());
    }
// ★★★ 3パターンのテスト用コード ★★★

    void Update()
    {
        // 「1」キーで、いいね数 0 の「Small」花火をテスト
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("テスト：Smallの花火（いいね 0）を打ち上げます！");
            PostData testPost = new PostData
            {
                PostID = "testPost_Small_" + Random.Range(0, 10000),
                Message = "Smallのテスト投稿です",
                UserName = "TestUser",
                LikeCount = 0, // いいね数 0
                Timestamp = System.DateTime.UtcNow
            };
            OnNewPostReceived(testPost);
        }

        // 「2」キーで、いいね数 5 の「Medium」花火をテスト
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("テスト：Mediumの花火（いいね 5）を打ち上げます！");
            PostData testPost = new PostData
            {
                PostID = "testPost_Medium_" + Random.Range(0, 10000),
                Message = "Mediumのテスト投稿です",
                UserName = "TestUser",
                LikeCount = 5, // いいね数 5
                Timestamp = System.DateTime.UtcNow
            };
            OnNewPostReceived(testPost);
        }

        // 「3」キーで、いいね数 10 の「Large」花火をテスト
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("テスト：Largeの花火（いいね 10）を打ち上げます！");
            PostData testPost = new PostData
            {
                PostID = "testPost_Large_" + Random.Range(0, 10000),
                Message = "Largeのテスト投稿です",
                UserName = "TestUser",
                LikeCount = 10, // いいね数 10
                Timestamp = System.DateTime.UtcNow
            };
            OnNewPostReceived(testPost);
        }// In the Update() method of FestivalManager.cs

    // Press 'R' for a Red firework
    if (Input.GetKeyDown(KeyCode.R))
    {
        PostData testPost = new PostData
        {
            PostID = "testPost_Red_" + Random.Range(0, 10000),
            Timestamp = System.DateTime.UtcNow,
            LikeCount = 0,
            RedVotes = 10,
            GreenVotes = 1,
            BlueVotes = 1
        };
        OnNewPostReceived(testPost);
    }

    // Press 'G' for a Green firework
    if (Input.GetKeyDown(KeyCode.G))
    {
        PostData testPost = new PostData
        {
            PostID = "testPost_Green_" + Random.Range(0, 10000),
            Timestamp = System.DateTime.UtcNow,
            LikeCount = 0,
            RedVotes = 1,
            GreenVotes = 10,
            BlueVotes = 1
        };
        OnNewPostReceived(testPost);
    }
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