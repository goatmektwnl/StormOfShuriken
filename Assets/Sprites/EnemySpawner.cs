using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;
    public float minY = -4f;
    public float maxY = 4f;

    [Header("Spawn Settings")]
    // 💡 적이 차지하는 공간의 크기 (이 반경 안에는 다른 적이 못 옵니다)
    public float checkRadius = 1.5f;

    // 무한 반복 방지용 (자리가 꽉 찼을 때 최대 몇 번 다시 뽑을지)
    public int maxAttempts = 10;

    void Start()
    {
        InvokeRepeating("SpawnEnemy", 1f, spawnInterval);
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition = Vector3.zero;
        bool canSpawn = false;
        int attempts = 0;

        // 💡 핵심: 안전한 빈자리를 찾을 때까지 최대 10번(maxAttempts) 반복해서 자리를 다시 뽑습니다.
        while (!canSpawn && attempts < maxAttempts)
        {
            float randomY = Random.Range(minY, maxY);
            spawnPosition = new Vector3(transform.position.x, randomY, 0f);

            // 방금 뽑은 자리에 반경(checkRadius)만큼 가상의 원을 그려서 부딪히는 모든 것을 가져옵니다.
            Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPosition, checkRadius);

            bool isOverlapping = false;

            // 부딪힌 것들 중에 "Enemy" 태그를 가진 녀석이 하나라도 있는지 검사합니다.
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    isOverlapping = true; // 앗, 다른 적이 이미 있다!
                    break;
                }
            }

            // 그 자리에 다른 적이 없다면? 스폰 가능 상태로 변경하여 반복문을 탈출합니다.
            if (!isOverlapping)
            {
                canSpawn = true;
            }

            attempts++; // 시도 횟수 증가
        }

        // 반복문이 끝난 후, 안전한 자리를 찾았다면 적을 생성합니다!
        if (canSpawn)
        {
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            // 만약 화면이 적들로 꽉 차서 10번 넘게 빈자리를 못 찾았다면, 이번 턴은 그냥 생성하지 않고 넘깁니다.
            Debug.Log("스폰 자리가 꽉 차서 생성을 한 턴 쉬어갑니다.");
        }
    }
}
