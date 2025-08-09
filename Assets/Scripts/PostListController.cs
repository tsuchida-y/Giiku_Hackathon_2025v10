using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PostListController : MonoBehaviour
{
    [Header("References")]
    public Transform content;        // Scroll View / Viewport / Content
    public GameObject postItemPrefab;

    // べた書きJSON（ラッパー posts が必要）
    private string seedJson = @"
{
  ""posts"": [
    {
      ""id"": ""p1"",
      ""owner"": ""Ryo"",
      ""text"": ""初めての花火SNS投稿！"",
      ""createdAtUnix"": 1722840000,
      ""likes"": 12
    },
    {
      ""id"": ""p1"",
      ""owner"": ""Ryo"",
      ""text"": ""初めての花火SNS投稿！"",
      ""createdAtUnix"": 1722840000,
      ""likes"": 12
    },
    {
      ""id"": ""p1"",
      ""owner"": ""Ryo"",
      ""text"": ""初めての花火SNS投稿！"",
      ""createdAtUnix"": 1722840000,
      ""likes"": 12
    },
    {
      ""id"": ""p1"",
      ""owner"": ""Ryo"",
      ""text"": ""初めての花火SNS投稿！"",
      ""createdAtUnix"": 1722840000,
      ""likes"": 12
    },
    {
      ""id"": ""p1"",
      ""owner"": ""Ryo"",
      ""text"": ""初めての花火SNS投稿！"",
      ""createdAtUnix"": 1722840000,
      ""likes"": 12
    },
    {
      ""id"": ""p2"",
      ""owner"": ""Mika"",
      ""text"": ""青い花火が綺麗だった～"",
      ""createdAtUnix"": 1722926400,
      ""likes"": 7
    },
    {
      ""id"": ""p3"",
      ""owner"": ""太郎"",
      ""text"": ""海に行った！ああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああああ"",
      ""createdAtUnix"": 1722326400,
      ""likes"": 10
    }
    
  ]
}";

    private List<Post> _posts = new();

    [Serializable]
    public class Post
    {
        public string id;
        public string owner;
        public string text;
        public long createdAtUnix;
        public int likes;
    }

    [Serializable]
    class PostListWrapper { public Post[] posts; }

    void Start()
    {
        LoadFromSeedJson(); // ← べた書きから読む
        BuildList();
    }

    void LoadFromSeedJson()
    {
        var wrapper = JsonUtility.FromJson<PostListWrapper>(seedJson);
        _posts = (wrapper != null && wrapper.posts != null)
            ? new List<Post>(wrapper.posts)
            : new List<Post>();
    }

    void BuildList()
    {
        foreach (Transform child in content) Destroy(child.gameObject);
        foreach (var p in _posts)
        {
            var go = Instantiate(postItemPrefab, content);
            Bind(go, p);
        }
    }

    void Bind(GameObject go, Post p)
    {
        var ownerText = go.transform.Find("OwnerText")?.GetComponent<TMP_Text>();
        var bodyText = go.transform.Find("BodyText")?.GetComponent<TMP_Text>();
        var timeText = go.transform.Find("TimeText")?.GetComponent<TMP_Text>();
        var button = go.GetComponentInChildren<Button>(true);

        if (ownerText) ownerText.text = p.owner;
        if (bodyText)
        {
            int limit = 70; // 最大文字数（全角・半角問わず）
            if (p.text.Length > limit)
                bodyText.text = p.text.Substring(0, limit) + "…";
            else
                bodyText.text = p.text;
        }
        if (timeText) timeText.text = UnixToJstString(p.createdAtUnix);

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                PostRoute.SelectedPostId = p.id;
                UnityEngine.SceneManagement.SceneManager.LoadScene("PostDetailScene");
            });
        }
    }

    string UnixToJstString(long unixSec)
    {
        var dt = DateTimeOffset.FromUnixTimeSeconds(unixSec).ToLocalTime().DateTime;
        return dt.ToString("yyyy/MM/dd HH:mm");
    }
}

public static class PostRoute
{
    public static string SelectedPostId;
}
