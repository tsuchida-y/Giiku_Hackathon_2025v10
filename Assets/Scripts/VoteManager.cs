using UnityEngine;
public class VoteManager : MonoBehaviour
{
    // Unityエディタから設定できるようにpublicにする
    public PostData postData;

    // 赤ボタンがクリックされたときに呼ばれる関数
    public void OnRedButtonClick()
    {
        if (postData != null)
        {
            postData.RedVotes++;
            Debug.Log($"赤に投票されました。現在の投票数: {postData.RedVotes}");
        }
    }

    // 緑ボタンがクリックされたときに呼ばれる関数
    public void OnGreenButtonClick()
    {
        if (postData != null)
        {
            postData.GreenVotes++;
            Debug.Log($"緑に投票されました。現在の投票数: {postData.GreenVotes}");
        }
    }

    // 青ボタンがクリックされたときに呼ばれる関数
    public void OnBlueButtonClick()
    {
        if (postData != null)
        {
            postData.BlueVotes++;
            Debug.Log($"青に投票されました。現在の投票数: {postData.BlueVotes}");
        }
    }
}