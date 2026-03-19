using UnityEngine;

public class ItemBoxSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject itemBoxPrefab; // 💡 날려보낼 상자 프리팹

    [Tooltip("몇 초마다 상자가 등장할지 결정합니다.")]
    public float spawnInterval = 15f; // 15초마다 등장

    [Header("스폰 높이 (Y축 랜덤 범위)")]
    public float minY = -3.5f;
    public float maxY = 3.5f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnBox();
            timer = 0f; // 타이머 초기화
        }
    }

    void SpawnBox()
    {
        if (itemBoxPrefab != null)
        {
            // 💡 Y축 높이만 랜덤으로 뽑고, X축은 이 스포너가 있는 위치(화면 우측 밖)로 설정합니다.
            float randomY = Random.Range(minY, maxY);
            Vector3 spawnPos = new Vector3(transform.position.x, randomY, 0f);

            Instantiate(itemBoxPrefab, spawnPos, Quaternion.identity);
        }
    }
}