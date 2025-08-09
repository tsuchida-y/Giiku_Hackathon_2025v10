using UnityEngine;
using TMPro; // TextMeshProを使うために必要
using UnityEngine.UI;
using Firebase.Firestore;
using System.Collections.Generic;
using Firebase.Extensions; // ← この行を追加

public class DataManager : MonoBehaviour
{
    // UnityエディタからInputFieldとButtonをここに設定します
    public TMP_InputField inputField;
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
        string inputText = inputField.text;

        // テキストが空の場合は処理をしない
        if (string.IsNullOrEmpty(inputText))
        {
            Debug.LogWarning("入力が空です。");
            return;
        }

        // Firestoreに保存するデータを作成
        //ここで入力されたテキストと現在のタイムスタンプを含むデータを作成
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "message", inputText },
            { "timestamp", Timestamp.GetCurrentTimestamp() }
        };

        // "messages"というコレクションに新しいドキュメントを追加
        db.Collection("messages").AddAsync(data).ContinueWithOnMainThread(task => {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("データの保存に成功しました！");
                inputField.text = ""; // 保存後に入力欄をクリア
            }
            else
            {
                Debug.LogError("データの保存に失敗しました: " + task.Exception);
            }
        });
    }
}