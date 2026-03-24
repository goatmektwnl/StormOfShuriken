using UnityEngine;

public class PlayerClone : MonoBehaviour
{
    [Header("분신 공격 설정")]
    public GameObject basicKunaiPrefab; // 무조건 기본 쿠나이만 발사
    public float fireRate = 0.5f;

    [Header("피격 연출")]
    // 💡 [핵심] 여기에 1단계에서 만든 '애니메이션 연출 프리팹'을 연결합니다!
    public GameObject hitEffectPrefab;

    private float timer = 0f;

    void Start()
    {
        // 소환될 때 아무 연출 없이 조용하게 태어납니다.
        timer = Random.Range(0f, fireRate);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= fireRate)
        {
            Shoot();
            timer = 0f;
        }
    }

    void Shoot()
    {
        if (basicKunaiPrefab != null)
        {
            Instantiate(basicKunaiPrefab, transform.position, transform.rotation);
        }
    }

    // 💡 분신이 몬스터 공격에 직접 부딪혔을 때
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
        {
            // [핵심] 자폭하기 바로 직전, 그 자리에 연출 프리팹 생성!
            if (hitEffectPrefab != null)
            {
                // 소환 즉시 1단계 스크립트에 의해 애니메이션 재생 후 자동 파괴됩니다.
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            // 분신 파괴
            Destroy(gameObject);

            if (other.CompareTag("EnemyBullet"))
            {
                Destroy(other.gameObject);
            }
        }
    }
}