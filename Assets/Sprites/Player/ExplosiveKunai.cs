using UnityEngine;

public class ExplosiveKunai : MonoBehaviour
{
    public float speed = 10f;             // 날아가는 속도
    public float explosionRadius = 3f;    // 폭발 반경
    public GameObject explosionPrefab;    // 폭발 이펙트 프리팹

    // 💡 [추가] 폭발 데미지를 조절할 수 있게 밖으로 뺐습니다.
    public int explosionDamage = 1;

    void Start()
    {
        Destroy(gameObject, 4f); // 4초 뒤 자동 파괴
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 보스 태그("Boss")를 따로 쓰실 수도 있으니 조건에 슬쩍 추가해 둡니다.
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Enemy") || hit.CompareTag("Boss"))
                {
                    if (explosionPrefab != null) Instantiate(explosionPrefab, hit.transform.position, Quaternion.identity);

                    // 1. 기존 잡몹 데미지 처리
                    Enemy enemyScript = hit.GetComponent<Enemy>();
                    if (enemyScript != null) enemyScript.TakeHit();

                    // 2. 구형 엘리트 몹 데미지 처리
                    EliteEnemy eliteScript = hit.GetComponent<EliteEnemy>();
                    if (eliteScript != null) eliteScript.TakeHit();

                    // 💡 3. [신규] 까마귀 요괴 데미지 처리!
                    CrowMonster crowScript = hit.GetComponent<CrowMonster>();
                    if (crowScript != null) crowScript.TakeDamage(explosionDamage);

                    // 💡 4. [신규] 신형 엘리트 검사 데미지 처리!
                    EliteSwordEnemy swordEliteScript = hit.GetComponent<EliteSwordEnemy>();
                    if (swordEliteScript != null) swordEliteScript.TakeDamage(explosionDamage);

                    // 💡 5. [신규] 2스테이지 보스 데미지 처리!
                    Stage2Boss bossScript = hit.GetComponent<Stage2Boss>();
                    if (bossScript != null) bossScript.TakeDamage(explosionDamage);
                }
            }
            Destroy(gameObject); // 쿠나이는 임무를 마치고 파괴
        }
    }

    // 에디터에서 폭발 범위를 빨간 원으로 미리 보여주는 기능
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}