using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("기본 스폰 설정")]
    public GameObject enemyPrefab;
    public float minY = -4f;
    public float maxY = 4f;
    public float checkRadius = 1.5f;
    public int maxAttempts = 10;

    [Header("🔥 난이도 자동 상승 설정")]
    public float initialSpawnInterval = 3f;
    public float minimumSpawnInterval = 0.5f;
    public float decreasePerSecond = 0.02f;

    private float currentSpawnInterval;
    private float timer = 0f;

    // 💡 [핵심 추가] 보스가 오면 스폰을 멈추기 위한 스위치입니다.
    private bool isSpawning = true;

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
    }

    void Update()
    {
        // 💡 [수정] 스위치가 꺼져있다면 아래의 모든 스폰 로직을 실행하지 않고 건너뜁니다!
        if (!isSpawning) return;

        // 1. 난이도 자동 상승 로직
        if (currentSpawnInterval > minimumSpawnInterval)
        {
            currentSpawnInterval -= decreasePerSecond * Time.deltaTime;
            if (currentSpawnInterval < minimumSpawnInterval)
            {
                currentSpawnInterval = minimumSpawnInterval;
            }
        }

        // 2. 타이머 스톱워치
        timer += Time.deltaTime;

        // 3. 스폰 실행
        if (timer >= currentSpawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    // 💡 [핵심 추가] 대피 매니저가 호출할 전원 차단 함수입니다.
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("🚫 [일반몹 스포너] 전원이 차단되었습니다. 더 이상 몹을 생산하지 않습니다.");
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector3 spawnPosition = Vector3.zero;
        bool canSpawn = false;
        int attempts = 0;

        while (!canSpawn && attempts < maxAttempts)
        {
            float randomY = Random.Range(minY, maxY);
            spawnPosition = new Vector3(transform.position.x, randomY, 0f);

            Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPosition, checkRadius);
            bool isOverlapping = false;

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    isOverlapping = true;
                    break;
                }
            }

            if (!isOverlapping) canSpawn = true;
            attempts++;
        }

        if (canSpawn)
        {
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }
}