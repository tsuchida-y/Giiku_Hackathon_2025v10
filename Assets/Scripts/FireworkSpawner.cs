using UnityEngine;
using System.Collections.Generic;

public class FireworkSpawner : MonoBehaviour
{
    public GameObject fireworkPrefab;
    public float spawnRadius = 50f;
    public float minDistanceBetweenFireworks = 10f; // 花火同士の最小距離

    // 直近で生成された花火の位置を保持するリスト
    private List<Vector3> recentSpawnPositions = new List<Vector3>();
    private const int maxRecentPositions = 10; // 記憶する位置の数

    // FestivalManagerから呼び出される関数
    public FireworkController SpawnFirework(PostData data, FestivalManager manager)
    {
        // ランダムな打ち上げ位置を計算（既存の花火と重ならないように）
        Vector3 spawnPosition = CalculateSpawnPosition();

        // 花火を生成
        GameObject instance = Instantiate(fireworkPrefab, spawnPosition, Quaternion.identity);
        FireworkController controller = instance.GetComponent<FireworkController>();

        // 生成した花火の初期設定を行う
        if (controller != null)
        {
            // 色の設定を追加
            SetFireworkColor(controller, data);

            // 花火の初期化
            controller.Initialize(data, manager);

            // 位置を記録
            AddRecentPosition(spawnPosition);
        }
        return controller;
    }

    // 既存の花火と重ならない位置を計算
    private Vector3 CalculateSpawnPosition()
    {
        Vector3 position;
        int attempts = 0;
        const int maxAttempts = 10; // 最大試行回数

        do
        {
            // ランダムな角度と距離を生成
            float angle = Random.Range(0, 360f);
            float distance = Random.Range(spawnRadius * 0.3f, spawnRadius);

            position = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );

            attempts++;

            // 十分な試行回数を超えた場合は現在の位置を使用
            if (attempts >= maxAttempts)
                break;

        } while (!IsPositionValid(position));

        return position;
    }

    // 位置が既存の花火と十分に離れているか確認
    private bool IsPositionValid(Vector3 position)
    {
        foreach (Vector3 existingPosition in recentSpawnPositions)
        {
            if (Vector3.Distance(position, existingPosition) < minDistanceBetweenFireworks)
                return false;
        }
        return true;
    }

    // 最近の位置リストに追加
    private void AddRecentPosition(Vector3 position)
    {
        recentSpawnPositions.Add(position);

        // リストのサイズを制限
        if (recentSpawnPositions.Count > maxRecentPositions)
        {
            recentSpawnPositions.RemoveAt(0);
        }
    }

    // // RGB値に基づいて花火の色を設定
    // private void SetFireworkColor(FireworkController controller, PostData data)
    // {
    //     // RGB値を取得
    //     int redVotes = Mathf.Max(1, data.RedVotes);    // 0除算を防ぐため最小値を1に
    //     int greenVotes = Mathf.Max(1, data.GreenVotes);
    //     int blueVotes = Mathf.Max(1, data.BlueVotes);

    //     // 合計値を計算
    //     float total = redVotes + greenVotes + blueVotes;

    //     // 正規化して0〜1の範囲にする
    //     float r = redVotes / total;
    //     float g = greenVotes / total;
    //     float b = blueVotes / total;

    //     // 色を少し明るくするための調整（オプション）
    //     float brightness = 1.0f;
    //     r = Mathf.Min(1.0f, r * brightness);
    //     g = Mathf.Min(1.0f, g * brightness);
    //     b = Mathf.Min(1.0f, b * brightness);

    //     // 色を設定
    //     Color fireworkColor = new Color(r, g, b);

    //     // FireworkControllerに色を設定
    //     controller.SetFireworkColor(fireworkColor);
    // }


    // RGB値に基づいて花火の色を設定（彩度強化版）
    private void SetFireworkColor(FireworkController controller, PostData data)
    {
        // RGB値取得（ゼロ除算防止）
        int redVotes = Mathf.Max(1, data.RedVotes);
        int greenVotes = Mathf.Max(1, data.GreenVotes);
        int blueVotes = Mathf.Max(1, data.BlueVotes);

        // 正規化
        float total = redVotes + greenVotes + blueVotes;
        float r = redVotes / total;
        float g = greenVotes / total;
        float b = blueVotes / total;

        // --- HSV変換で彩度を強化 ---
        Color.RGBToHSV(new Color(r, g, b), out float h, out float s, out float v);

        s = Mathf.Clamp01(s * 0.5f);   // 彩度を50%アップ
        v = Mathf.Clamp01(v * 2.0f);   // 明るさも少し上げる

        Color fireworkColor = Color.HSVToRGB(h, s, v);

        // FireworkControllerに色を設定
        controller.SetFireworkColor(fireworkColor);
    }

}