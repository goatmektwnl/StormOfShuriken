using UnityEngine;

public class CrowSpawner : MonoBehaviour
{
    [Header("까마귀 스폰 설정")]
    public GameObject crowPrefab;
    public float minSpawnInterval = 3f;
    public float maxSpawnInterval = 6f;

    [Header("스폰 Y축 범위")]
    public float minY = -4f;
    public float maxY = 4f;

    private float timer;
    private bool isSpawning = true; // 보스가 나타나면 끌 스위치

    void Start()
    {
        SetNextTimer();
    }

    void Update()
    {
        if (!isSpawning) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnCrow();
            SetNextTimer();
        }
    }

    void SetNextTimer()
    {
        timer = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void SpawnCrow()
    {
        if (crowPrefab != null)
        {
            // 이 스포너가 있는 X 위치에서, 랜덤한 Y 높이로 까마귀 소환!
            Vector3 spawnPos = new Vector3(transform.position.x, Random.Range(minY, maxY), 0f);
            Instantiate(crowPrefab, spawnPos, Quaternion.identity);
        }
    }

    // 💡 보스가 등장할 때 매니저가 이 함수를 부르면 스폰이 멈춥니다.
    public void StopSpawning()
    {
        isSpawning = false;
    }
}