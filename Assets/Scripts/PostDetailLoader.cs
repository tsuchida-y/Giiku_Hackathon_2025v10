using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PostDetailLoader : MonoBehaviour
{
    public TextMeshProUGUI PostText, PostOwner;
    
    // RGB投票数表示用テキスト
    public TextMeshProUGUI RedVotesText, GreenVotesText, BlueVotesText;
    
    // 色を視覚的に表示するイメージ（オプション）
    public Image ColorPreview;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var post = SelectedPost.Current;
        if (post == null)
        {
            Debug.LogWarning("SelectedPost.Current is null. Back to list.");
            // 直接シーンを開いた等のケース。保険で一覧へ戻す
            UnityEngine.SceneManagement.SceneManager.LoadScene("postlistScene");
            return;
        }
        
        if (!PostOwner || !PostText)
        {
            Debug.LogError("PostOwner / PostText が未割り当てです（Inspectorで割り当ててください）");
            return;
        }
        
        // 基本情報の表示
        PostOwner.text = post.UserName ?? "匿名";
        PostText.text = post.Message ?? "";
        
        // デバッグ出力（データの確認）
        Debug.Log($"PostDetailLoader - 表示する投稿データ: UserName={post.UserName}, Message={post.Message}, PostID={post.PostID}");
        
        // データが正しく設定されているか検証
        if (string.IsNullOrEmpty(post.UserName)) Debug.LogWarning("UserNameが空です");
        if (string.IsNullOrEmpty(post.Message)) Debug.LogWarning("Messageが空です");
        
        // RGB投票数の表示
        if (RedVotesText) RedVotesText.text = post.RedVotes.ToString();
        if (GreenVotesText) GreenVotesText.text = post.GreenVotes.ToString();
        if (BlueVotesText) BlueVotesText.text = post.BlueVotes.ToString();
        
        // 色のプレビュー表示（オプション）
        if (ColorPreview)
        {
            float totalVotes = post.RedVotes + post.GreenVotes + post.BlueVotes;
            if (totalVotes <= 0) totalVotes = 3; // デフォルト値として3を使用（各色1ずつ）
            
            float r = post.RedVotes / totalVotes;
            float g = post.GreenVotes / totalVotes;
            float b = post.BlueVotes / totalVotes;
            
            ColorPreview.color = new Color(r, g, b);
        }
        
        // デバッグログ
        Debug.Log($"投稿詳細を表示: {post.UserName}, ID: {post.PostID}, 投票: R={post.RedVotes} G={post.GreenVotes} B={post.BlueVotes}");
    }

    public void OnEditButtonClick()
    {
        string currentText = PostText.text;
        string currentOwner = PostOwner.text;

        // 遷移先に値を渡す（後述の方法を使う）
        PostEditData.TempPostText = currentText;
        PostEditData.TempPostOwner = currentOwner;

        // 新規投稿画面へ遷移
        UnityEngine.SceneManagement.SceneManager.LoadScene("newpostScene");
    }

    public void OnDeleteButtonClick()
    {
        Debug.Log("delete");
    }

    public void OnHomeButtonClick()
    {
        Debug.Log("Home");
        UnityEngine.SceneManagement.SceneManager.LoadScene("FireSpwer");
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
