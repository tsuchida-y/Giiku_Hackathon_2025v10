using UnityEngine;

public class FireworkSpawner : MonoBehaviour
{
    public GameObject fireworkPrefab;   // 生成する花火のプレハブ
    public float spawnRadius = 30f;     // ★カメラからの距離（半径）
    public float spawnInterval = 1.0f;    // 花火が生成される間隔（秒）

    void Start()
    {
        StartCoroutine(SpawnFireworks());
    }

    System.Collections.IEnumerator SpawnFireworks()
    {
        while (true)
        {
            // カメラ(0,0,0)を中心とした、半径spawnRadiusの円周上のランダムな点を計算
            float angle = Random.Range(0, 360f);
            Vector3 spawnPosition = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius,
                0, // 打ち上げ開始高さ
                Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius
            );

            // 花火を生成
            Instantiate(fireworkPrefab, spawnPosition, Quaternion.identity);

            // 指定した秒数だけ待つ
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}