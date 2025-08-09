using System.Collections;
using UnityEngine;

public class FireworkController : MonoBehaviour
{
    [Header("クリック判定用")]
    [SerializeField] Transform hitTarget; // Inspector で HitTarget を割り当て
    SphereCollider hitCol;


    // 花火の3段階の成長レベルを定義
    public enum FireworkTier { Small, Medium, Large }

    [Header("必須コンポーネント")]
    public ParticleSystem launchTrail;
    public ParticleSystem explosionBurst;

    [Header("成長レベルごとの設定")]
    public float smallHeight = 5f;
    public float smallSize = 1f;
    public float smallLifetimeMinutes = 60f;

    public float mediumHeight = 10f;
    public float mediumSize = 5f;
    public float mediumLifetimeMinutes = 120f; // 60分 + 追加60分

    public float largeHeight = 15f;
    public float largeSize = 10f;
    public float largeLifetimeMinutes = 180f; // 60分 + 追加120分

    public float smallSpeed = 5f;//花火の球の半径
    public float mediumSpeed = 10f;
    public float largeSpeed = 15f;

    // 内部で使う変数
    private PostData currentPostData;
    private FestivalManager festivalManager;
    private float currentHeight; // ← いまの打上げ高さを保存

    // Spawnerから呼び出される、一生の始まりの合図
    public void Initialize(PostData data, FestivalManager manager)
    {
        this.currentPostData = data;
        this.festivalManager = manager;
        StartCoroutine(LifecycleCoroutine());
    }
    /// <summary>
    /// Managerが、この花火の投稿データを参照するために使う
    /// </summary>
    public PostData GetPostData()
    {
        return currentPostData;
    }

    /// <summary>
    /// Managerから、寿命を待たずに消滅するように命令される
    /// </summary>
    public void Despawn()
    {
        StopAllCoroutines(); // 進行中のライフサイクルを停止
        Destroy(gameObject); // オブジェクトを即座に削除
    }

    // 花火の一生を管理するメインのコルーチン
    private IEnumerator LifecycleCoroutine()
    {
        // --- 1: 最初の打ち上げ (必ずSmallサイズで開始) 
        UpdateAppearance(FireworkTier.Small);
        yield return StartCoroutine(LaunchAndBloom());

        // --- 2: 最初の60分間の寿命
        // (ここでは簡略化のため、60分待つ処理のみ。10分ごとの減衰もここに入れる)
        yield return new WaitForSeconds(smallLifetimeMinutes * 60f); // 分を秒に変換して待機

        // --- 3: 60分後のいいね数チェック
        int latestLikeCount = festivalManager.GetLikesForPost(currentPostData.PostID);
        currentPostData.LikeCount = latestLikeCount;

        // ---4: 運命の分岐
        FireworkTier nextTier;
        if (latestLikeCount >= 10)
        {
            nextTier = FireworkTier.Large;
        }
        else if (latestLikeCount >= 5)
        {
            nextTier = FireworkTier.Medium;
        }
        else
        {
            // 【消滅ルート】
            Destroy(gameObject, 10f); // 10秒かけて消える
            yield break; // コルーチンを終了
        }

        // 【成長ルート】再打ち上げと、その後の寿命
        UpdateAppearance(nextTier);
        yield return StartCoroutine(LaunchAndBloom()); // 新しい見た目で再打ち上げ＆開花

        float newLifetime = 0;
        if (nextTier == FireworkTier.Medium)
        {
            newLifetime = mediumLifetimeMinutes - smallLifetimeMinutes;
        }
        else if (nextTier == FireworkTier.Large)
        {
            newLifetime = largeLifetimeMinutes - smallLifetimeMinutes;
        }

        if (newLifetime > 0)
        {
            // (ここに追加の寿命と減衰の処理を記述)
            yield return new WaitForSeconds(newLifetime * 60f);
        }

        Destroy(gameObject, 10f);
    }


    void Awake()
    {
        if (hitTarget) hitCol = hitTarget.GetComponent<SphereCollider>();
        if (hitCol) hitCol.enabled = false; // 初期は無効
    }

    // 打ち上げと開花の一連の流れ + 当たり判定
    private IEnumerator LaunchAndBloom()
    {
        // 上昇演出
        launchTrail.Play();
        yield return new WaitForSeconds(launchTrail.main.duration);

        // 頂点（現在の高さ）に移動して判定ON
        Vector3 apexPos = transform.position + Vector3.up * currentHeight;

        if (explosionBurst) explosionBurst.transform.position = apexPos;
        if (hitCol)
        {
            hitTarget.position = apexPos;
            // ティアに合わせて半径調整してもOK（例：サイズに比例）
            // hitCol.radius = Mathf.Max(1.5f, currentHeight * 0.12f);
            hitCol.enabled = true;
        }

        // 爆発演出
        explosionBurst.Play();

        yield return new WaitWhile(() => explosionBurst.IsAlive(true));

        // 余韻で 0.2〜0.5秒だけ猶予（端末差の吸収）
        yield return new WaitForSeconds(0.3f);
        
        // 爆発終了+少しだけ余韻
        // yield return new WaitForSeconds(explosionBurst.main.duration + 0.5f);

        if (hitCol) hitCol.enabled = false;
    }

    // 見た目更新＋高さ記録
    private void UpdateAppearance(FireworkTier tier)
    {
        var launchMain = launchTrail.main;
        var explosionMain = explosionBurst.main;

        if (tier == FireworkTier.Small)
        {
            currentHeight = smallHeight;
            launchMain.startSpeed = smallHeight;
            explosionMain.startSize = smallSize;
            explosionMain.startSpeed = smallSpeed;
        }
        else if (tier == FireworkTier.Medium)
        {
            currentHeight = mediumHeight;
            launchMain.startSpeed = mediumHeight;
            explosionMain.startSize = mediumSize;
            explosionMain.startSpeed = mediumSpeed;
        }
        else // Large
        {
            currentHeight = largeHeight;
            launchMain.startSpeed = largeHeight;
            explosionMain.startSize = largeSize;
            explosionMain.startSpeed = largeSpeed;
        }
    }

}


