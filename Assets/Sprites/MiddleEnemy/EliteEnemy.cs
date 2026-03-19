using System.Collections;
using UnityEngine;

public class EliteEnemy : MonoBehaviour
{
    [Header("능력치")]
    public int hp = 10;
    public float moveSpeed = 2f;

    [Header("이동 및 배회 구역")]
    public float enterXPosition = 6f;
    public float minX = -8f;
    public float maxX = 6f;
    public float minY = -4f;
    public float maxY = 4f;

    [Header("공격 설정")]
    public GameObject swordAuraPrefab;
    public float fireRate = 3f;

    [Header("사망 및 전리품")]
    public GameObject deathMotionPrefab;
    public GameObject[] buffItemPrefabs;

    public GameObject shieldItemPrefab;
    [Range(0, 100)] public float shieldDropChance = 20f;

    [Header("피격 효과 설정")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    // 💡 [핵심 추가] 처음 스폰될 때는 무조건 무적(true)으로 시작합니다!
    private bool isInvincible = true;

    private bool isAdvancing = true;
    private Vector3 targetPos;
    private Animator animator;
    private bool isAttacking = false;
    private bool isDead = false;

    private SpriteRenderer sr;

    void Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isAttacking || isDead) return;

        if (isAdvancing)
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;

            // 💡 [핵심] 정해진 위치(enterXPosition)에 도달해서 멈추는 순간!
            if (transform.position.x <= enterXPosition)
            {
                isAdvancing = false;

                // 🛑 자리를 잡았으므로 무적 방어막을 해제합니다! (이제 피 흘릴 시간입니다)
                isInvincible = false;

                SetNextWanderPosition();
                if (animator != null) animator.SetBool("isMoving", true);
                StartCoroutine(AttackRoutine());
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                SetNextWanderPosition();
            }
        }
    }

    void SetNextWanderPosition()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        targetPos = new Vector3(randomX, randomY, transform.position.z);
    }

    IEnumerator AttackRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(fireRate);
            if (animator != null && hp > 0 && !isDead) animator.SetTrigger("Attack");
        }
    }

    public void OnAttackStart() { isAttacking = true; }
    public void FireSwordAura()
    {
        if (swordAuraPrefab != null) Instantiate(swordAuraPrefab, transform.position, Quaternion.identity);
    }
    public void OnAttackEnd() { isAttacking = false; }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 죽었으면 모든 판정 무시
        if (isDead) return;

        // 플레이어의 쿠나이에 맞았을 때
        if (other.CompareTag("Kunai"))
        {
            // 💡 1. 아직 자리잡기 전 (무적 상태) 이라면?
            if (isInvincible)
            {
                // 쿠나이만 팅! 하고 튕겨내듯 파괴하고, 엘리트 몹은 데미지를 입지 않습니다.
                Destroy(other.gameObject);
                return;
            }

            // 💡 2. 자리를 잡은 후 (무적 해제 상태) 라면 정상적으로 피격!
            TakeHit();
            Destroy(other.gameObject);
        }
    }

    public void TakeHit()
    {
        if (isDead) return;

        hp--;

        if (hp > 0) StartCoroutine(HitFlashRoutine());
        else Die();
    }

    IEnumerator HitFlashRoutine()
    {
        if (sr != null) sr.color = hitColor;
        yield return new WaitForSeconds(flashDuration); // 하드코딩된 0.1f 대신 변수 사용
        if (sr != null) sr.color = Color.white;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(500);
            GameManager.instance.AddKill();
        }

        StartCoroutine(HitAndDieRoutine());
    }

    IEnumerator HitAndDieRoutine()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (sr != null) sr.color = hitColor;
        if (deathMotionPrefab != null) Instantiate(deathMotionPrefab, transform.position, Quaternion.identity);

        DropItem();

        yield return new WaitForSeconds(0.1f);

        Destroy(gameObject);
    }

    void DropItem()
    {
        if (buffItemPrefabs != null && buffItemPrefabs.Length > 0)
        {
            foreach (GameObject item in buffItemPrefabs)
            {
                if (item != null && Random.Range(1, 11) == 1)
                {
                    Instantiate(item, transform.position, Quaternion.identity);
                }
            }
        }

        if (shieldItemPrefab != null && Random.Range(0f, 100f) <= shieldDropChance)
        {
            Instantiate(shieldItemPrefab, transform.position, Quaternion.identity);
        }
    }
}