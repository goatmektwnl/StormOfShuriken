using UnityEngine;

public class ItemBoxSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject itemBoxPrefab;
    public float spawnInterval = 15f;

    [Header("스폰 높이 (Y축 랜덤 범위)")]
    public float minY = -3.5f;
    public float maxY = 3.5f;

    private float timer = 0f;
    // 💡 isSpawning 변수 삭제 (항상 생성)

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnBox();
            timer = 0f;
        }
    }

    void SpawnBox()
    {
        if (itemBoxPrefab != null)
        {
            float randomY = Random.Range(minY, maxY);
            Vector3 spawnPos = new Vector3(transform.position.x, randomY, 0f);
            Instantiate(itemBoxPrefab, spawnPos, Quaternion.identity);
        }
    }
    // 💡 StopSpawning 함수 삭제 (보스가 멈추라고 해도 무시함)
}