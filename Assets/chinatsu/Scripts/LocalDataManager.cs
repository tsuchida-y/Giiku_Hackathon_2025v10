using UnityEngine;
using System.IO;

public class LocalDataManager : MonoBehaviour
{
    public static LocalDataManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public PostList LoadPosts()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "posts.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            PostList postList = JsonUtility.FromJson<PostList>(json);
            return postList;
        }
        else
        {
            Debug.LogError("JSONファイルが見つかりません: " + filePath);
            return new PostList { posts = new PostData[0] };
        }
    }
}