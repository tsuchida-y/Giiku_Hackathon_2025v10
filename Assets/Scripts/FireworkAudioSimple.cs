/*using UnityEngine;

public enum FireTier { Small, Medium, Large }

[RequireComponent(typeof(AudioSource))]
public class FireworkAudioSimple : MonoBehaviour
{
    public AudioClip smallClip;
    public AudioClip mediumClip;
    public AudioClip largeClip;

    [Range(0,1)] public float spatialBlend = 0f; // まずは2Dで確実に鳴らす
    public float maxDistance = 40f;

    AudioSource src;

    void Awake() {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = spatialBlend;
        src.dopplerLevel = 0f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.maxDistance = maxDistance;
    }

    public void PlayForTier(FireTier tier) {
        AudioClip clip = tier switch {
            FireTier.Small  => smallClip,
            FireTier.Medium => mediumClip,
            FireTier.Large  => largeClip,
            _ => null
        };

        var listener = FindObjectOfType<AudioListener>();
        if (listener) {
            float d = Vector3.Distance(transform.position, listener.transform.position);
            Debug.Log($"[SFX] clip={(clip ? clip.name : "NULL")} dist={d:F1} blend={src.spatialBlend} max={src.maxDistance}");
        }

        if (clip != null) src.PlayOneShot(clip);
    }

    [ContextMenu("Test Play Small")]  void TestSmall()  => PlayForTier(FireTier.Small);
    [ContextMenu("Test Play Medium")] void TestMedium() => PlayForTier(FireTier.Medium);
    [ContextMenu("Test Play Large")]  void TestLarge()  => PlayForTier(FireTier.Large);
}
*/