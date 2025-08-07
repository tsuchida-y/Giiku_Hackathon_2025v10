using UnityEngine;

public class FireworkSpawner : MonoBehaviour
{
    // インスペクターから設定する項目
    public GameObject fireworkPrefab;   // 生成する花火のプレハブ
    public float spawnRadius = 50f;     // 花火が生成される中心からの半径
    public float spawnInterval = 1.5f;    // 花火が生成される間隔（秒）

    // Start is called before the first frame update
    void Start()
    {
        // 花火の生成を無限に繰り返す命令を開始する
        StartCoroutine(SpawnFireworks());
    }

    // 花火を生成し続けるコルーチン
    System.Collections.IEnumerator SpawnFireworks()
    {
        // 無限ループ
        while (true)
        {
            // 次の花火を生成する位置を計算
            // spawnRadiusの円周上のランダムな点を取得
            float angle = Random.Range(0, 360f);
            Vector3 spawnPosition = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius, // X座標
                0,                                              // Y座標（高さ）
                Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius  // Z座標
            );

            // 花火を生成する
            Instantiate(fireworkPrefab, spawnPosition, Quaternion.identity);

            // spawnIntervalで指定した秒数だけ待つ
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}