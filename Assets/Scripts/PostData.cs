[System.Serializable]
public class PostData
{
    public string PostID;
    public string Message;
    public string UserName;
public int LikeCount
{
    get { return RedVotes + GreenVotes + BlueVotes; }
}
    public System.DateTime Timestamp;

    public int RedVotes;
    public int GreenVotes;
    public int BlueVotes;
}
