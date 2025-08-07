using UnityEngine;
using TMPro;
public class PostDetailLoader : MonoBehaviour
{

    public TextMeshProUGUI PostText, PostOwner;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PostOwner.text = "ここに投稿主が入ります";
        PostText.text = "ここに投稿内容が入ります";
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

    public void OnBackButtonClick()
    {
        Debug.Log("Back");
    }
}
