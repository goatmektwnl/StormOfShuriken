using UnityEngine;

public class ExplosiveKunai : MonoBehaviour
{
    public float speed = 10f;             // 날아가는 속도
    public float explosionRadius = 3f;    // 💡 폭발(즉사) 반경
    public GameObject explosionPrefab;    // 폭발 이펙트 프리팹

    void Start()
    {
        Destroy(gameObject, 4f); // 4초 뒤 자동 파괴
    }

    void Update()
    {
        // 오른쪽으로 날아갑니다. (만약 위나 왼쪽으로 쏜다면 방향을 맞춰주세요)
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    if (explosionPrefab != null) Instantiate(explosionPrefab, hit.transform.position, Quaternion.identity);

                    // 1. 일반 잡몹 데미지 처리
                    Enemy enemyScript = hit.GetComponent<Enemy>();
                    if (enemyScript != null) enemyScript.TakeHit();

                    // 💡 2. 여기를 추가! 엘리트 몹 데미지 처리
                    EliteEnemy eliteScript = hit.GetComponent<EliteEnemy>();
                    if (eliteScript != null) eliteScript.TakeHit();
                }
            }
            Destroy(gameObject);
        }
    }

    // 에디터에서 폭발 범위를 빨간 원으로 미리 보여주는 기능
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
