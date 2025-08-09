using UnityEngine;
using TMPro;

public class NewPostLoader : MonoBehaviour
{
    public TMP_InputField PostInputField;
    public TMP_InputField OwnerInputField;

    void Start()
    {
        // 値が保存されていたら表示
        PostInputField.text = PostEditData.TempPostText;
        OwnerInputField.text = PostEditData.TempPostOwner;

        // 使い終わったら初期化してもよい
        PostEditData.TempPostText = "";
        PostEditData.TempPostOwner = "";
    }

    public void OnPostButtonClick()
    {
        Debug.Log("New or Update post");
        UnityEngine.SceneManagement.SceneManager.LoadScene("postlistScene");
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
