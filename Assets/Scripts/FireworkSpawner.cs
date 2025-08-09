using UnityEngine;

public class FireworkSpawner : MonoBehaviour
{
    public GameObject fireworkPrefab;
    public float spawnRadius = 50f;

    // FestivalManagerから呼び出される関数
    public FireworkController SpawnFirework(PostData data, FestivalManager manager)
    {
        // ランダムな打ち上げ位置を計算
        float angle = Random.Range(0, 360f);
        Vector3 spawnPosition = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius, // spawnRadiusは半径
            0,
            Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius
        );

        // 花火を生成
        GameObject instance = Instantiate(fireworkPrefab, spawnPosition, Quaternion.identity);
        FireworkController controller = instance.GetComponent<FireworkController>();
        
        // 生成した花火の初期設定を行う
        if(controller != null)
        {
            // ★エラー修正点：引数にmanagerを追加
            controller.Initialize(data, manager);
        }
        return controller;
    }
}