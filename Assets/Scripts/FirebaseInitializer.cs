using UnityEngine;
using Firebase;
using Firebase.Extensions;
using System.Threading.Tasks;
using System;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseInitializer Instance { get; private set; }
    
    public bool IsInitialized { get; private set; } = false;
    public bool HasError { get; private set; } = false;
    public string ErrorMessage { get; private set; } = "";

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void InitializeFirebase()
    {
        Debug.Log("Firebaseの初期化を開始します...");
        
        // Firebase初期化チェック
        await CheckAndFixFirebaseDependencies();
        
        if (!HasError)
        {
            Debug.Log("Firebaseの初期化が完了しました");
            IsInitialized = true;
        }
        else
        {
            Debug.LogError($"Firebaseの初期化に失敗しました: {ErrorMessage}");
        }
    }

    private async Task CheckAndFixFirebaseDependencies()
    {
        try
        {
            // Firebase Unity SDKが初期化に必要な全ての依存関係を解決する
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            
            if (dependencyStatus == DependencyStatus.Available)
            {
                // 依存関係が解決され、Firebaseは初期化できる状態
                Debug.Log("Firebaseの依存関係が解決されました");
                
                // Firebaseアプリが既に初期化されているか確認
                var app = FirebaseApp.DefaultInstance;
                if (app == null)
                {
                    Debug.LogWarning("FirebaseApp.DefaultInstanceがnullです。手動で初期化します。");
                    FirebaseApp.Create();
                }
            }
            else
            {
                // 依存関係の解決に失敗
                HasError = true;
                ErrorMessage = $"Firebase依存関係の解決に失敗しました: {dependencyStatus}";
                Debug.LogError(ErrorMessage);
            }
        }
        catch (Exception e)
        {
            HasError = true;
            ErrorMessage = $"Firebase初期化中に例外が発生しました: {e.Message}";
            Debug.LogException(e);
        }
    }
}
