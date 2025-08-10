using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class DataPostiine : MonoBehaviour
{
    // 投票ボタン
    public Button happyButton;    // 赤（Happy）
    public Button gladButton;     // 緑（Glad）
    public Button sadButton;      // 青（Sad）
    
    // 投票数表示テキスト
    public TMP_Text happyText;    // 赤（Happy）の投票数
    public TMP_Text gladText;     // 緑（Glad）の投票数
    public TMP_Text blueText;     // 青（Sad）の投票数
    
    // カラープレビュー（オプション）
    public Image colorPreview;
    
    // 現在の投稿ID（どの投稿に対するリアクションか）
    private string currentPostId;
    
    // Firebase Firestoreへの参照
    private FirebaseFirestore db;
    
    // 現在の投票数
    private int redVotes = 0;
    private int greenVotes = 0;
    private int blueVotes = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Firebase Firestoreの初期化
        db = FirebaseFirestore.DefaultInstance;
        
        // 現在の投稿IDを取得（例えばSelectedPost.Currentから）
        if (SelectedPost.Current != null)
        {
            currentPostId = SelectedPost.Current.PostID;
            
            // 初期値を設定
            redVotes = SelectedPost.Current.RedVotes;
            greenVotes = SelectedPost.Current.GreenVotes;
            blueVotes = SelectedPost.Current.BlueVotes;
            
            // テキスト表示を更新
            UpdateVoteTexts();
            
            // カラープレビューを更新
            UpdateColorPreview();
        }
        else
        {
            Debug.LogError("投稿データが見つかりません");
            return;
        }
        
        // ボタンのクリックイベントを設定
        SetupButtonListeners();
    }
    
    // ボタンのクリックイベントを設定
    private void SetupButtonListeners()
    {
        // ボタンのnullチェック
        if (happyButton == null || gladButton == null || sadButton == null)
        {
            Debug.LogError("リアクションボタンがInspectorで設定されていません");
            return;
        }
        
        // 既存のリスナーをクリア
        happyButton.onClick.RemoveAllListeners();
        gladButton.onClick.RemoveAllListeners();
        sadButton.onClick.RemoveAllListeners();
        
        // 新しいリスナーを追加
        happyButton.onClick.AddListener(() => UpdateVoteCount("redVotes"));
        gladButton.onClick.AddListener(() => UpdateVoteCount("greenVotes"));
        sadButton.onClick.AddListener(() => UpdateVoteCount("blueVotes"));
    }
    
    // テキスト表示を更新
    private void UpdateVoteTexts()
    {
        if (happyText != null) happyText.text = redVotes.ToString();
        if (gladText != null) gladText.text = greenVotes.ToString();
        if (blueText != null) blueText.text = blueVotes.ToString();
    }
    
    // カラープレビューの更新（PostDetailLoaderと同じ計算方法）
    private void UpdateColorPreview()
    {
        if (colorPreview == null) return;
        
        // 投票数の合計を計算
        float totalVotes = redVotes + greenVotes + blueVotes;
        if (totalVotes <= 0) totalVotes = 3; // デフォルト値として3を使用（各色1ずつ）
        
        // 投票数の比率を計算（明示的にfloatにキャスト）
        float r = (float)redVotes / totalVotes;
        float g = (float)greenVotes / totalVotes;
        float b = (float)blueVotes / totalVotes;
        
        // HSV変換で彩度と明度を補正（PostDetailLoaderと同じ方法）
        Color.RGBToHSV(new Color(r, g, b), out float h, out float s, out float v);
        s = Mathf.Clamp01(s * 0.5f); // 彩度を調整
        v = Mathf.Clamp01(v * 2.0f); // 明度を調整
        
        // デバッグログを追加して値を確認
        Debug.Log($"色の更新: R={r:F3}, G={g:F3}, B={b:F3}, HSV={h:F3},{s:F3},{v:F3}, 合計={totalVotes}票 (赤={redVotes}, 緑={greenVotes}, 青={blueVotes})");
        
        // HSVからRGBに変換して色を設定 - PostDetailLoaderと同じ方法
        colorPreview.color = Color.HSVToRGB(h, s, v);
    }
    
    // 投票数の更新処理
    private void UpdateVoteCount(string voteType)
    {
        // 投稿IDのチェック
        if (string.IsNullOrEmpty(currentPostId))
        {
            Debug.LogError("投稿IDが設定されていません");
            return;
        }
        
        // Firestoreの初期化チェック
        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
            if (db == null)
            {
                Debug.LogError("Firestoreがまだ初期化されていません");
                return;
            }
        }
        
        // ドキュメント参照の取得
        DocumentReference docRef = db.Collection("messages").Document(currentPostId);
        
        // トランザクションを使用して投票数を更新
        db.RunTransactionAsync(transaction =>
        {
            return transaction.GetSnapshotAsync(docRef)
                .ContinueWith(task =>
                {
                    DocumentSnapshot snapshot = task.Result;
                    Dictionary<string, object> data = snapshot.ToDictionary();
                    
                    // 現在の投票数を取得して+1する
                    long currentVotes = 0;
                    if (data.ContainsKey(voteType))
                    {
                        currentVotes = System.Convert.ToInt64(data[voteType]);
                    }
                    
                    // 投票数を更新
                    Dictionary<string, object> updates = new Dictionary<string, object>
                    {
                        { voteType, currentVotes + 1 }
                    };
                    
                    // ドキュメントを更新
                    transaction.Update(docRef, updates);
                    
                    return true;
                });
        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log($"{voteType}の投票を追加しました！");
                
                // 最新のデータを取得して表示を更新
                FetchLatestVoteCount();
                
                // ローカルでの表示も更新（即時フィードバック用）
                UpdateLocalVotes(voteType);
            }
            else
            {
                Debug.LogError($"{voteType}の投票の追加に失敗しました: " + task.Exception);
            }
        });
    }
    
    // ローカルでの投票数表示を更新
    private void UpdateLocalVotes(string voteType)
    {
        bool updated = false;
        
        switch (voteType)
        {
            case "redVotes":
                redVotes++;
                if (happyText != null) happyText.text = redVotes.ToString();
                updated = true;
                break;
                
            case "greenVotes":
                greenVotes++;
                if (gladText != null) gladText.text = greenVotes.ToString();
                updated = true;
                break;
                
            case "blueVotes":
                blueVotes++;
                if (blueText != null) blueText.text = blueVotes.ToString();
                updated = true;
                break;
        }
        
        // 変更があった場合のみカラープレビューも更新
        if (updated)
        {
            Debug.Log($"投票数更新後: 赤={redVotes}, 緑={greenVotes}, 青={blueVotes}");
            UpdateColorPreview();
        }
    }
    
    // Firestoreから最新の投票数を取得
    private void FetchLatestVoteCount()
    {
        if (string.IsNullOrEmpty(currentPostId) || db == null) return;
        
        DocumentReference docRef = db.Collection("messages").Document(currentPostId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => 
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
            {
                Dictionary<string, object> data = task.Result.ToDictionary();
                
                // 投票数を取得（デフォルト値は0）
                int previousRedVotes = redVotes;
                int previousGreenVotes = greenVotes;
                int previousBlueVotes = blueVotes;
                
                // データを整数に変換
                if (data.TryGetValue("redVotes", out object redObj))
                {
                    redVotes = System.Convert.ToInt32(redObj);
                }
                else
                {
                    redVotes = 0;
                }
                
                if (data.TryGetValue("greenVotes", out object greenObj))
                {
                    greenVotes = System.Convert.ToInt32(greenObj);
                }
                else
                {
                    greenVotes = 0;
                }
                
                if (data.TryGetValue("blueVotes", out object blueObj))
                {
                    blueVotes = System.Convert.ToInt32(blueObj);
                }
                else
                {
                    blueVotes = 0;
                }
                
                // 値が変わった場合のみ更新
                bool updated = (previousRedVotes != redVotes || 
                               previousGreenVotes != greenVotes || 
                               previousBlueVotes != blueVotes);
                
                if (updated)
                {
                    // UI更新
                    UpdateVoteTexts();
                    UpdateColorPreview();
                    
                    Debug.Log($"Firestoreから最新データを取得: 赤={redVotes}, 緑={greenVotes}, 青={blueVotes}");
                }
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"Firestoreからのデータ取得に失敗: {task.Exception}");
            }
        });
    }
}
