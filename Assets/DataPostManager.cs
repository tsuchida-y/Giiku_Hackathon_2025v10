using UnityEngine;
using TMPro; // TextMeshProを使うために必要
using UnityEngine.UI;
using Firebase.Firestore;
using System.Collections.Generic;
using Firebase.Extensions; // ← この行を追加

public class DataManager : MonoBehaviour
{
    // UnityエディタからInputFieldとButtonをここに設定します
    public TMP_InputField nameInputField;    // 名前入力用のInputField
    public TMP_InputField messageInputField; // 投稿内容入力用のInputField
    public Button saveButton;

    private FirebaseFirestore db;

    void Start()
    {
        // Firebase Firestoreのインスタンスを初期化
        db = FirebaseFirestore.DefaultInstance;

        // ボタンがクリックされたらSaveDataメソッドを呼び出すように設定
        saveButton.onClick.AddListener(() => SaveData());
    }

    public void SaveData()
    {
        string nameText = nameInputField.text;
        string messageText = messageInputField.text;

        // 名前または投稿内容が空の場合は処理をしない
        if (string.IsNullOrEmpty(nameText) || string.IsNullOrEmpty(messageText))
        {
            Debug.LogWarning("名前または投稿内容が空です。");
            return;
        }

        // Firestoreに保存するデータを作成
        //ここで名前、投稿内容、現在のタイムスタンプを含むデータを作成
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "name", nameText },
            { "message", messageText },
            { "timestamp", Timestamp.GetCurrentTimestamp() }
        };

        // "messages"というコレクションに新しいドキュメントを追加
        db.Collection("messages").AddAsync(data).ContinueWithOnMainThread(task => {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("データの保存に成功しました！");
                // 保存後に入力欄をクリア
                nameInputField.text = "";
                messageInputField.text = ""; 
            }
            else
            {
                Debug.LogError("データの保存に失敗しました: " + task.Exception);
            }
        });
    }
}