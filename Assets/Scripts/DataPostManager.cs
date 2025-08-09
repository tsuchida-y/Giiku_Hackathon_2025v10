using UnityEngine;
using TMPro; // TextMeshProを使うために必要
using UnityEngine.UI;
using Firebase.Firestore;
using System.Collections.Generic;
using Firebase.Extensions; // Firebaseの非同期処理をメインスレッドで実行するために必要

public class DataPostManager : MonoBehaviour
{
    // UnityエディタからInputFieldとButtonをここに設定します
    public TMP_InputField nameInputField;    // 名前入力用のInputField
    public TMP_InputField messageInputField; // 投稿内容入力用のInputField
    public Button saveButton;                // 投稿内容保存用のButton

    private FirebaseFirestore db;// FirebaseFirestoreにアクセスするための変数

    void Start()
    {
        // Firebase Firestoreのインスタンスを初期化
        db = FirebaseFirestore.DefaultInstance;
        
        // Firebaseを使うのに必要な環境（依存関係）が揃っているかを非同期でチェック
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            Firebase.DependencyStatus dependencyStatus = task.Result;

            
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Firebaseが使用可能な場合
                Debug.Log("Firebase初期化成功！");
                
                // Firebaseアプリが既に初期化されているか確認
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
                if (app == null)
                {
                    Debug.Log("Firebase App を作成します");
                    Firebase.FirebaseApp.Create();
                }
                
                // もう一度Firestoreを初期化
                db = FirebaseFirestore.DefaultInstance;
            }
            else
            {
                Debug.LogError($"Firebase初期化に失敗: {dependencyStatus}");
            }
        });

        //ボタンを何度も重複して押したときに、同じ処理が何回も実行されるのを防ぐ
        saveButton.onClick.RemoveAllListeners();

        // ボタンがクリックされたらSaveDataメソッドを呼び出すように設定
        saveButton.onClick.AddListener(() => SaveData());
    }
    
    public void SaveData()
    {
        // Firestoreの初期化チェック
        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
            if (db == null)
            {
                Debug.LogError("Firestoreがまだ初期化されていません。");
                return;
            }
        }

        string nameText = nameInputField.text;
        string messageText = messageInputField.text;

        // 名前または投稿内容が空の場合は処理をしない
        if (string.IsNullOrEmpty(nameText) || string.IsNullOrEmpty(messageText))
        {
            Debug.LogWarning("名前または投稿内容が空です。");
            return;
        }

        // Firestoreに保存するデータを作成
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "name", nameText },
            { "message", messageText },
            { "timestamp", Timestamp.GetCurrentTimestamp() },
            { "likes", 0 } // いいね数の初期値を0に設定
        };

        // "messages"というコレクションに新しいドキュメントを追加
        db.Collection("messages").AddAsync(data).ContinueWithOnMainThread(task => {
            //保存が成功してたら入力欄をクリア
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("データの保存に成功しました！");
                // 保存後に入力欄をクリア
                nameInputField.text = "";
                messageInputField.text = ""; 
                
                // 投稿一覧画面を更新（存在する場合）
                UpdatePostList();
            }
            else
            {
                Debug.LogError("データの保存に失敗しました: " + task.Exception);
            }
        });
    }
    
    // 投稿一覧画面を更新する
    void UpdatePostList()
    {
        // DataDisplayListControllerを探してリロードを呼び出す
        DataDisplayListController[] displayControllers = FindObjectsOfType<DataDisplayListController>();
        
        Debug.Log($"{displayControllers.Length}個の表示コントローラーを見つけました");
        
        foreach (var controller in displayControllers)
        {
            // 直接メソッドを呼び出す
            controller.LoadFromFirestore();
            Debug.Log("投稿一覧を更新しました");
        }
    }
}