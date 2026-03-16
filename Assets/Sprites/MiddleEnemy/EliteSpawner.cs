using System.Collections;
using UnityEngine;

public class EliteSpawner : MonoBehaviour
{
    public GameObject elitePrefab; // 방금 만든 엘리트 몹 프리팹
    public float minSpawnTime = 10f;
    public float maxSpawnTime = 15f;

    void Start()
    {
        // 게임 시작 시 타이머 작동!
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 💡 10초에서 15초 사이의 랜덤한 시간을 뽑아서 대기합니다.
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            // 대기 시간이 끝나면 엘리트 몹 출격!
            if (elitePrefab != null)
            {
                Instantiate(elitePrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
