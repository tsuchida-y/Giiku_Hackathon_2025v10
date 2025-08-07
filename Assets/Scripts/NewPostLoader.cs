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
    }

    public void OnBackButtonClick()
    {
        Debug.Log("Back");  
    }


}
