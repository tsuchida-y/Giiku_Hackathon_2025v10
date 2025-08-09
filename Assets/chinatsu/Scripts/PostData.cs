
using System;

[Serializable]
public class PostData
{
    public string id;
    public string owner;
    public string text;
    public long createdAtUnix;
    public int likes;
}

[Serializable]
public class PostList
{
    public PostData[] posts;
}