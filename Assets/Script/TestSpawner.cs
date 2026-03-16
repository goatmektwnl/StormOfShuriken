using UnityEngine;

public class TestSpawner : MonoBehaviour
{
    public GameObject kunaiPrefab;
    public float spawnInterval = 1.0f;
    public float yRange = 4f;
    public float kunaiSpeed = 8f;

    void Start()
    {
        // 1초 뒤부터 소환 시도 (하지만 내부 조건에서 걸러질 거예요)
        InvokeRepeating("SpawnKunai", 1f, spawnInterval);
    }

    void SpawnKunai()
    {
        // [수정된 핵심 로직]
        // 1. 매니저가 없거나(null), 2. 게임이 아직 시작 안 했거나, 3. 게임이 끝났다면? -> 소환 금지!
        if (GameManager.instance == null || !GameManager.instance.isGameStarted || GameManager.instance.isGameOver)
        {
            return;
        }

        // --- 여기서부터는 게임이 진짜 시작되었을 때만 실행됩니다 ---
        float randomY = Random.Range(-yRange, yRange);
        Vector3 spawnPos = new Vector3(12f, 0f, 0f); // y값에 randomY를 넣어줬어요!

        GameObject newKunai = Instantiate(kunaiPrefab, spawnPos, Quaternion.identity);

        Kunai kunaiScript = newKunai.GetComponent<Kunai>();
        if (kunaiScript != null)
        {
            kunaiScript.speed = kunaiSpeed;
        }
    }

    // Update의 빈 체크문은 이제 삭제해도 됩니다!
}