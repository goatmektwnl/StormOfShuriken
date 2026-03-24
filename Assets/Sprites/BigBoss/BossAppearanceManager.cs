using System.Collections;
using UnityEngine;

public class BossAppearanceManager : MonoBehaviour
{
    [Header("소멸 연출 설정")]
    // 💡 분신 아이템에서 쓰던 그 "펑!" 터지는 애니메이션 프리팹을 인스펙터에서 넣어주세요!
    public GameObject disappearEffectPrefab;

    public void MakeEnemiesFlee()
    {
        Debug.Log("📢 [보스 등장] 모든 스포너를 정지하고 필드를 청소합니다.");

        // 1. [스포너 전원 차단]
        EliteSpawner[] eliteSpawners = FindObjectsByType<EliteSpawner>(FindObjectsSortMode.None);
        foreach (EliteSpawner spawner in eliteSpawners) { spawner.StopSpawning(); }

        EnemySpawner[] enemySpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        foreach (EnemySpawner spawner in enemySpawners) { spawner.StopSpawning(); }


        // 2. [이미 나온 몹들 "펑!" 터뜨리기]
        // "Enemy" 태그를 가진 모든 게임 오브젝트를 찾습니다.
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        Debug.Log("👀 감지된 적의 수: " + allEnemies.Length);

        foreach (GameObject enemyObj in allEnemies)
        {
            if (enemyObj == null) continue;

            // A. 이펙트 소환 (분신 아이템 애니메이션 프리팹)
            if (disappearEffectPrefab != null)
            {
                // 적의 현재 위치에 이펙트를 생성합니다.
                Instantiate(disappearEffectPrefab, enemyObj.transform.position, Quaternion.identity);
            }

            // B. 적 본체 즉시 삭제
            Destroy(enemyObj);
        }
    }
}