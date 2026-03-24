using System.Collections;
using UnityEngine;

public class EliteSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject elitePrefab;
    public float minSpawnTime = 10f;
    public float maxSpawnTime = 15f;

    [Header("Y축(위아래) 랜덤 스폰 범위")]
    public float minY = -4f;
    public float maxY = 4f;

    // 💡 [핵심 추가] 기계의 전원을 켜고 끄는 스위치입니다!
    private bool isSpawning = true;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // 무한 반복(true) 대신, 스위치(isSpawning)가 켜져 있을 때만 돌아가게 합니다.
        while (isSpawning)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            // 기다리는 동안 보스가 등장해서 전원이 꺼졌을 수도 있으니 한 번 더 체크!
            if (isSpawning && elitePrefab != null)
            {
                float randomY = Random.Range(minY, maxY);
                Vector3 spawnPosition = new Vector3(transform.position.x, randomY, transform.position.z);
                Instantiate(elitePrefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    // 💡 [핵심 추가] 보스가 등장하면 대피 매니저가 이 함수를 눌러서 전원을 강제로 뽑아버립니다.
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines(); // 현재 진행 중인 스폰 대기 시간도 즉시 취소!
    }
}