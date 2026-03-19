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
    public GameObject[] buffItemPrefabs;
    [Range(0, 100)]
    public float dropChance = 10f;

    [Header("피격 판정 제어")]
    // 💡 [핵심 추가] 처음 스폰될 때는 무조건 무적(true)으로 시작합니다!
    private bool isInvincible = true;

    private bool isAdvancing = true;
    private bool hasAttacked = false;
    private bool isMovingLeftAgain = false;
    private bool isDead = false;

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

            // 💡 [핵심] 정해진 위치(stopXPosition)에 도달해서 멈추는 순간!
            if (transform.position.x <= stopXPosition)
            {
                isAdvancing = false;

                // 🛑 자리를 잡았으므로 무적 방어막을 해제합니다! 이제부터 타격 가능!
                isInvincible = false;

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

        // 플레이어의 쿠나이에 맞았을 때
        if (other.CompareTag("Kunai"))
        {
            // 💡 1. 아직 자리잡기 전 (무적 상태) 이라면?
            if (isInvincible)
            {
                // 쿠나이만 튕겨내듯 파괴하고, 몬스터는 데미지를 입지 않습니다!
                Destroy(other.gameObject);
                return;
            }

            // 💡 2. 자리를 잡은 후 (무적 해제 상태) 라면 정상 타격!
            TakeHit();
            Destroy(other.gameObject);
        }
    }

    public void TakeHit()
    {
        if (isDead) return;
        isDead = true; // 자물쇠 쾅!

        // 게임 매니저에 점수와 킬 수 보고!
        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(100);
            GameManager.instance.AddKill();
        }

        DropItem(); // 아이템 떨구기

        StartCoroutine(HitAndDieRoutine());
    }

    // 피격 시 빨갛게 번쩍이고 죽는 마법의 코루틴
    IEnumerator HitAndDieRoutine()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (sr != null) sr.color = Color.red;

        if (deathMotionPrefab != null) Instantiate(deathMotionPrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.1f);

        Destroy(gameObject);
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