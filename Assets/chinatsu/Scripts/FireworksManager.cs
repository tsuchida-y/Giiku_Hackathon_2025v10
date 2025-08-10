/*
using UnityEngine;

// 花火の生成と管理を行うクラス
public class FireworksManager : MonoBehaviour
{
    // Inspectorから花火のプレハブをアタッチする
    public GameObject fireworkPrefab;

    void Start()
    {
        // TODO: ここに投稿データを読み込む処理を記述する
        // 現時点ではダミーデータを作成して動作を確認できます
        // 例: postData = LocalDataManager.Instance.LoadPosts();

        // 投稿データに基づいて花火を生成するロジック
          foreach (var post in postData.posts)
          {
              // 花火プレハブをインスタンス化
              GameObject firework = Instantiate(fireworkPrefab, GetRandomPosition(), Quaternion.identity);

              // FireworkTriggerに投稿IDをセット
              var fireworkTrigger = firework.GetComponent<FireworkTrigger>();
              if(fireworkTrigger != null)
              {
                fireworkTrigger.postId = post.id;
              }

              // いいね数に応じて花火のサイズを変更
              var particleSystem = firework.GetComponentInChildren<ParticleSystem>();
              if (particleSystem != null)
              {
                  var mainModule = particleSystem.main;
                  float size = 1f + (post.likes * 0.1f);
                  mainModule.startSize = new ParticleSystem.MinMaxCurve(size, size + 0.5f);
              }
          }
    }

    // 花火を生成するランダムな位置を返すメソッド
    Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-20, 20), Random.Range(0, 15), Random.Range(-20, 20));
    }
}
*/