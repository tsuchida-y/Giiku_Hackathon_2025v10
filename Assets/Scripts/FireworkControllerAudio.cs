/*using UnityEngine;

public class FireworkControllerAudio : MonoBehaviour
{
    public enum FireworkTier { Small, Medium, Large }

    [Header("Particles (任意)")]
    public ParticleSystem launchTrail;     // 出現直後に再生（任意）
    public ParticleSystem explosionBurst;  // 出現直後に再生（任意）

    [Header("Audio")]
    public FireworkAudioSimple audioPlayer;       // 同じGOの FireworkAudioSimple を割当
    public FireworkTier tier = FireworkTier.Small;

    [Header("Options")]
    public bool playOnSpawn = true;       // 生成された瞬間に自動で鳴らすか
    public bool verboseLog = false;       // デバッグログ

    void Awake()
    {
        // 参照の取りこぼし保険
        if (!audioPlayer) audioPlayer = GetComponent<FireworkAudioSimple>();
        // 最低限のAudioSourceが無い場合は追加（FireworkAudioSimple側が設定を行う）
        if (!GetComponent<AudioSource>()) gameObject.AddComponent<AudioSource>();
    }

    void OnEnable()
    {
        if (playOnSpawn) PlayNow();
    }

    /// <summary>
    /// 手動トリガ用：生成直後以外に鳴らしたい場合はこれを外部から呼ぶ
    /// </summary>
    public void PlayNow()
    {
        // 視覚エフェクト（必要な方だけ割当・再生）
        launchTrail?.Play();
        explosionBurst?.Play();

        // 合体音をその場で1回だけ再生
        if (!audioPlayer) audioPlayer = GetComponent<FireworkAudioSimple>();
        if (audioPlayer)
        {
            FireTier t = tier == FireworkTier.Small ? FireTier.Small
                        : tier == FireworkTier.Medium ? FireTier.Medium
                        : FireTier.Large;

            if (verboseLog) Debug.Log($"[FW] PlayNow on {name}, tier={t}");
            audioPlayer.PlayForTier(t);
        }
        else
        {
            Debug.LogWarning($"[FW] FireworkAudioSimple not found on {name}");
        }
    }

    // 高さ（段階）を外部から変更したい場合
    public void SetTier(FireworkTier newTier) => tier = newTier;
}
*/
