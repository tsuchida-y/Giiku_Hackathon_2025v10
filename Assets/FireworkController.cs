using UnityEngine;

public class FireworkController : MonoBehaviour
{
    // 打ち上げと爆発のパーティクルシステムをインスペクターから設定
    public ParticleSystem launchParticles;
    public ParticleSystem explosionParticles;

    // いいね数に応じて変化を調整するための係数
    public float heightMultiplier = 0.5f; // いいね1つあたりの高さ上昇量
    public float sizeMultiplier = 0.1f;   // いいね1つあたりの爆発サイズ上昇量

    // この関数をSpawnerから呼び出す
    public void Initialize(int likeCount)
    {
        // --- 高さの計算 ---
        var launchMain = launchParticles.main;
        // 元の速度に、いいね数に応じた値を加算する
        float newSpeed = launchMain.startSpeed.constant + (likeCount * heightMultiplier);
        launchMain.startSpeed = newSpeed;

        // --- 大きさの計算 ---
        var explosionMain = explosionParticles.main;
        // 元のサイズに、いいね数に応じた値を加算する
        float newSize = explosionMain.startSize.constant + (likeCount * sizeMultiplier);
        explosionMain.startSize = newSize;
    }
}