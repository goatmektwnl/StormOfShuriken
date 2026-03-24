using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 3f; // 3초마다 1마리씩 생성

    // 적이 생성될 무작위 Y축(높이)의 범위 (에디터에서 화면 크기에 맞게 조절)
    public float minY = -4f;
    public float maxY = 4f;

    void Start()
    {
        // 1초 뒤부터 spawnInterval 간격으로 SpawnEnemy 함수 반복 실행
        InvokeRepeating("SpawnEnemy", 1f, spawnInterval);
    }

    void SpawnEnemy()
    {
        // 무작위 높이 뽑기
        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(transform.position.x, randomY, 0f);

        // 적 생성
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}
