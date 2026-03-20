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
    public float initialSpawnInterval = 3f;     // 처음 시작할 때의 스폰 간격 (3초)
    public float minimumSpawnInterval = 0.5f;   // 아무리 빨라져도 이 수치 밑으로는 안 떨어짐 (최대 난이도)
    public float decreasePerSecond = 0.02f;     // 1초마다 스폰 간격을 얼만큼씩 줄일 것인가? (0.02면 50초 뒤에 1초가 줄어듭니다)

    // 내부적으로 계산에 사용할 변수들
    private float currentSpawnInterval;
    private float timer = 0f;

    void Start()
    {
        // 시작할 때는 초기 간격(3초)으로 세팅합니다.
        // 💡 실시간으로 변동을 줘야 하므로 기존의 InvokeRepeating은 삭제했습니다.
        currentSpawnInterval = initialSpawnInterval;
    }

    void Update()
    {
        // 1. 시간이 지날수록 스폰 간격을 서서히 깎아냅니다.
        if (currentSpawnInterval > minimumSpawnInterval)
        {
            // Time.deltaTime을 곱해주어 프레임과 상관없이 1초에 decreasePerSecond 만큼만 정확히 깎습니다.
            currentSpawnInterval -= decreasePerSecond * Time.deltaTime;

            // 만약 깎다가 최소치(0.5초)보다 더 작아지면 0.5초로 고정시킵니다. (안 그러면 0초가 되어 컴퓨터가 멈춥니다)
            if (currentSpawnInterval < minimumSpawnInterval)
            {
                currentSpawnInterval = minimumSpawnInterval;
            }
        }

        // 2. 타이머 스톱워치를 돌립니다.
        timer += Time.deltaTime;

        // 3. 타이머가 현재 스폰 간격(currentSpawnInterval)을 채웠다면?
        if (timer >= currentSpawnInterval)
        {
            SpawnEnemy(); // 적 생성!
            timer = 0f;   // 스톱워치 다시 0으로 초기화
        }
    }

    void SpawnEnemy()
    {
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

            if (!isOverlapping)
            {
                canSpawn = true;
            }

            attempts++;
        }

        if (canSpawn)
        {
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }
}