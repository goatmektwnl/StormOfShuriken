using System.Collections;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public GameObject bossPrefab;
    public Vector3 spawnPosition = new Vector3(15f, 0f, 0f); // 화면 오른쪽 바깥

    private bool bossSpawned = false;

    // 💡 GameManager나 다른 곳에서 보스 소환 조건을 체크할 때 호출합니다.
    public void SpawnBoss()
    {
        if (!bossSpawned && bossPrefab != null)
        {
            bossSpawned = true;
            Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("최종 보스가 전장에 나타났습니다!");
        }
    }
}
