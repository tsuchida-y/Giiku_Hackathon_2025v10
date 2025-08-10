using UnityEngine;
using System.Collections;

public class FireworkAudioLooperGapless : MonoBehaviour
{
    public AudioClip[] clips;               // 花火SEを3つ（以上でもOK）
    public bool randomOrder = false;        // ランダム or ラウンドロビン
    [Range(0f, 0.2f)]
    public float crossfadeSeconds = 0.05f;  // 0〜0.1あたりが自然
    public float betweenDelay = 0f;         // 敢えて間を空けるなら>0

    AudioSource a, b; // 交互に使用
    int idx = -1;

    void Awake()
    {
        a = gameObject.AddComponent<AudioSource>();
        b = gameObject.AddComponent<AudioSource>();
        foreach (var s in new[] { a, b })
        {
            s.loop = false;
            s.playOnAwake = false;
            s.spatialBlend = 0f; // 2D再生。3Dにしたいなら 1f に
            s.volume = 1f;
        }

        if (clips != null && clips.Length > 0)
        {
            StartCoroutine(Loop());
        }
    }

    IEnumerator Loop()
    {
        var current = a;
        var next = b;

        // 最初の1発
        current.clip = GetNextClip();
        current.volume = 1f;
        current.Play();

        while (true)
        {
            // 次のクリップを準備
            next.clip = GetNextClip();
            next.volume = 0f;

            // 次の開始DSP時刻を計算（今の終わりの crossfadeSeconds 手前でスタート）
            double now = AudioSettings.dspTime;
            double remaining = current.clip.length - current.time;
            double startAt = now + Mathf.Max(0f, (float)remaining) - crossfadeSeconds + betweenDelay;

            // 予約再生
            next.PlayScheduled(startAt);

            // 予約時刻まで待機
            while (AudioSettings.dspTime < startAt)
                yield return null;

            // ここから crossfadeSeconds かけてフェード
            double fadeEnd = startAt + crossfadeSeconds;
            while (AudioSettings.dspTime < fadeEnd)
            {
                float t = (float)((AudioSettings.dspTime - startAt) / crossfadeSeconds);
                t = Mathf.Clamp01(t);
                current.volume = 1f - t;
                next.volume = t;
                yield return null;
            }
            current.volume = 0f;
            next.volume = 1f;

            // 直前のソースを「完全に役目を終えた側」として安全に停止
            if (current.isPlaying)
            {
                // 念のため、次の開始以降にフェード完了しているので停止OK
                current.Stop();
            }

            // スワップ（次が現在になる）
            var tmp = current;
            current = next;
            next = tmp;
        }
    }

    AudioClip GetNextClip()
    {
        if (clips == null || clips.Length == 0) return null;

        if (randomOrder)
        {
            int n;
            do { n = Random.Range(0, clips.Length); } while (clips.Length > 1 && n == idx);
            idx = n;
        }
        else
        {
            idx = (idx + 1) % clips.Length;
        }
        return clips[idx];
    }
}
