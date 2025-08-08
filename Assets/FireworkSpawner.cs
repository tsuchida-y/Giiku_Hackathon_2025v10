using UnityEngine;

public class FireworkSpawner : MonoBehaviour
{
    public GameObject[] fireworkPrefabs;
    public float spawnRadius = 50f;

    // ★★★ これがSNS担当者が呼び出すための「窓口」となる関数 ★★★
    public void LaunchFireworkWithLikes(int likeCount)
    {
        // 打ち上げ位置を計算
        float angle = Random.Range(0, 360f);
        Vector3 spawnPosition = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius,
            0,
            Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius
        );

        // 打ち上げる花火をランダムに選ぶ
        int randomIndex = Random.Range(0, fireworkPrefabs.Length);
        GameObject chosenPrefab = fireworkPrefabs[randomIndex];

        // 選んだ花火を生成
        GameObject fireworkInstance = Instantiate(chosenPrefab, spawnPosition, Quaternion.identity);

        // ★★★ 生成した花火に「いいね数」を伝えて初期設定させる ★★★
        FireworkController controller = fireworkInstance.GetComponent<FireworkController>();
        if (controller != null)
        {
            controller.Initialize(likeCount);
        }
    }
}