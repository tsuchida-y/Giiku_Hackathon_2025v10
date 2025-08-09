using UnityEngine;
using TMPro;
public class PostDetailLoader : MonoBehaviour
{
    public TextMeshProUGUI PostText, PostOwner;
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
        PostOwner.text = post.UserName ?? "匿名";
        PostText.text = post.Message ?? "";
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
