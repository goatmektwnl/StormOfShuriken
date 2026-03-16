using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("이동 및 공격 설정")]
    public float moveSpeed = 3f;
    public float stopXPosition = 6f;
    public GameObject enemyBulletPrefab;
    public float fireRate = 0.5f;

    [Header("사망 및 전리품")]
    public GameObject deathMotionPrefab;
    public GameObject[] buffItemPrefabs; // 아이템 여러 개 넣는 주머니
    [Range(0, 100)]
    public float dropChance = 10f;

    private bool isAdvancing = true;
    private bool hasAttacked = false;
    private bool isMovingLeftAgain = false;
    private bool isDead = false; // 💡 두 번 죽는 것 방지!

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDead) return; // 죽었으면 행동 정지!

        if (isAdvancing && !hasAttacked && !isMovingLeftAgain)
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;

            if (transform.position.x <= stopXPosition)
            {
                isAdvancing = false;
                StartCoroutine(FireThreeBullets());
            }
        }
        else if (isMovingLeftAgain)
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        }

        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator FireThreeBullets()
    {
        hasAttacked = true;

        for (int i = 0; i < 3; i++)
        {
            if (enemyBulletPrefab != null) Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(fireRate);
        }

        isMovingLeftAgain = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Kunai"))
        {
            TakeHit();
            Destroy(other.gameObject);
        }
    }

    public void TakeHit()
    {
        if (isDead) return;
        isDead = true; // 💡 자물쇠 쾅!

        // 게임 매니저에 점수와 킬 수 보고!
        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(100);
            GameManager.instance.AddKill();
        }

        DropItem(); // 아이템 떨구기

        // 💡 피격 효과를 보여주고 장렬히 산화하는 코루틴 실행!
        StartCoroutine(HitAndDieRoutine());
    }

    // 💡 피격 시 빨갛게 번쩍이고 죽는 마법의 코루틴
    IEnumerator HitAndDieRoutine()
    {
        // 1. 더 이상 맞지 않도록 무적(콜라이더 끄기) 처리
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 2. 색깔을 시뻘겋게!
        if (sr != null) sr.color = Color.red;

        // 3. 사망 폭발 이펙트 소환
        if (deathMotionPrefab != null) Instantiate(deathMotionPrefab, transform.position, Quaternion.identity);

        // 4. 0.1초 동안 빨간 상태 유지하며 비명 지르는 시간 제공!
        yield return new WaitForSeconds(0.1f);

        // 5. 완벽한 소멸
        Destroy(gameObject);
    }
    void Die()
    {
        // 점수도 올리고!
        if (GameManager.instance != null) GameManager.instance.AddScore(100);

        // 💡 [핵심] 킬 카운트도 무조건 올려야 합니다!! (이게 없으면 영원히 0킬입니다!)
        if (GameManager.instance != null) GameManager.instance.AddKill();

        Destroy(gameObject); // 적 몹 파괴
    }


    void DropItem()
    {
        if (buffItemPrefabs != null && buffItemPrefabs.Length > 0)
        {
            foreach (GameObject item in buffItemPrefabs)
            {
                if (item != null)
                {
                    int randomDice = Random.Range(1, 11);
                    if (randomDice == 1) // 10% 확률
                    {
                        Instantiate(item, transform.position, Quaternion.identity);
                    }
                }
            }
        }
    }
}



