using System.Collections;
using UnityEngine;

public class EliteSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject elitePrefab;
    public float minSpawnTime = 10f;
    public float maxSpawnTime = 15f;

    // 💡 [신규 추가] 인스펙터에서 위아래 한계선을 조절할 수 있습니다!
    [Header("Y축(위아래) 랜덤 스폰 범위")]
    public float minY = -4f; // 화면 아래쪽 한계선 (예: 바닥 부근)
    public float maxY = 4f;  // 화면 위쪽 한계선 (예: 천장 부근)

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            if (elitePrefab != null)
            {
                // 💡 [핵심] minY와 maxY 사이의 숫자 중 아무거나 하나를 무작위로 뽑습니다!
                float randomY = Random.Range(minY, maxY);

                // 스포너(투명한 빈 게임 오브젝트)가 있는 원래의 X 좌표는 그대로 유지하고,
                // Y 좌표만 방금 뽑은 랜덤값으로 바꿔서 새로운 생성 위치(Vector3)를 만듭니다.
                Vector3 spawnPosition = new Vector3(transform.position.x, randomY, transform.position.z);

                // 완성된 랜덤 위치에 엘리트 몹을 소환합니다!
                Instantiate(elitePrefab, spawnPosition, Quaternion.identity);
            }
        }
    }
}