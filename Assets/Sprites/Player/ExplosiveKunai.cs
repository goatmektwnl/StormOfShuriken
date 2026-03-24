using UnityEngine;

public class ExplosiveKunai : MonoBehaviour
{
    public float speed = 10f;             // 날아가는 속도
    public float explosionRadius = 3f;    // 폭발 반경
    public GameObject explosionPrefab;    // 폭발 이펙트 프리팹
    public int explosionDamage = 1;       // 폭발 데미지

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
        // 💡 [수정] 적(Enemy)이나 보스(Boss) 뿐만 아니라, 아이템 박스(ItemBox)와 부딪혀도 폭발 시작!
        if (other.CompareTag("Enemy") || other.CompareTag("Boss") || other.CompareTag("ItemBox"))
        {
            Explode();
        }
    }

    void Explode()
    {
        // 폭발 반경 안의 모든 물리체를 감지합니다.
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            bool targetFound = false;

            // 1. 일반 적군(잡몹, 엘리트 등) 데미지 처리
            if (hit.CompareTag("Enemy"))
            {
                hit.SendMessage("TakeDamage", explosionDamage, SendMessageOptions.DontRequireReceiver);
                hit.SendMessage("TakeHit", SendMessageOptions.DontRequireReceiver); // 구형 몹 대응
                targetFound = true;
            }

            // 2. 보스(BossEnemy) 데미지 처리
            if (hit.CompareTag("Boss"))
            {
                hit.SendMessage("TakeDamage", explosionDamage, SendMessageOptions.DontRequireReceiver);
                targetFound = true;
            }

            // 💡 3. [신규 추가] 아이템 박스 데미지 처리!
            if (hit.CompareTag("ItemBox"))
            {
                // 아이템 박스에게도 "TakeDamage" 메세지를 보내서 부서지게 만듭니다.
                // (만약 박스 스크립트의 파괴 함수 이름이 다르다면 그 이름으로 바꿔주면 됩니다!)
                hit.SendMessage("TakeDamage", explosionDamage, SendMessageOptions.DontRequireReceiver);
                targetFound = true;
            }

            // 폭발 이펙트 생성 (맞은 대상의 위치에 이펙트 생성)
            if (targetFound && explosionPrefab != null)
            {
                Instantiate(explosionPrefab, hit.transform.position, Quaternion.identity);
            }
        }

        // 폭발 후 쿠나이 본체 삭제
        Destroy(gameObject);
    }

    // 에디터에서 폭발 범위를 빨간 원으로 보여주는 기능
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}