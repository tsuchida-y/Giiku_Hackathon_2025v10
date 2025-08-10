using UnityEngine;

public class GrassTiler : MonoBehaviour
{
    [Header("複製する元のPrefab")]
    public GameObject grassPrefab; // Plane + 芝生スクリプト付き

    [Header("タイルのサイズ")]
    public float tileSize = 10f; // Planeの横幅(Scaleも考慮)

    [Header("配置する範囲（横×縦）")]
    public int tilesX = 3; // 横方向の枚数
    public int tilesZ = 3; // 縦方向の枚数

    void Start()
    {
        if (grassPrefab == null)
        {
            Debug.LogError("grassPrefab が設定されていません。");
            return;
        }

        // 中心を基準に並べる
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                // 配置位置を計算
                float posX = (x - tilesX / 2) * tileSize;
                float posZ = (z - tilesZ / 2) * tileSize;

                Instantiate(grassPrefab, new Vector3(posX, 0, posZ), Quaternion.identity, transform);
            }
        }
    }
}
