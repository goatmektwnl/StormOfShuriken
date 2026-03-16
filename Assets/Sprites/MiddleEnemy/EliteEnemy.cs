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
    public GameObject[] buffItemPrefabs; // 폭발쿠나이, 수리검 등

    // 💡 [새로 추가된 부분] 쉴드 전용 드랍 세팅!
    public GameObject shieldItemPrefab;
    [Range(0, 100)] public float shieldDropChance = 20f; // 정확히 20% 확률!

    [Header("피격 효과 설정")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

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

            if (transform.position.x <= enterXPosition)
            {
                isAdvancing = false;
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
        if (isAdvancing || isDead) return;

        if (other.CompareTag("Kunai"))
        {
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
        yield return new WaitForSeconds(0.1f);
        if (sr != null) sr.color = Color.white;
    }

    // --- 기존 Die() 함수를 아래 내용으로 완전히 교체하십시오! ---
    void Die()
    {
        if (isDead) return; // 중복 사망 방지
        isDead = true;

        // 1. 점수 및 킬 카운트 보고 (파괴신님의 설계 유지)
        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(500); // 엘리트니까 점수를 500점으로 높여드렸습니다!
            GameManager.instance.AddKill();
        }

        // 2. 💡 [핵심] 사망 연출 코루틴을 여기서 시동 겁니다!!
        // 이 한 줄이 들어가는 순간, 밑에 어둡던 루틴들이 밝게 타오를 것입니다!
        StartCoroutine(HitAndDieRoutine());
    }

    // --- 아래 IEnumerator와 DropItem은 그대로 두셔도 이제 불이 들어올 것입니다! ---
    IEnumerator HitAndDieRoutine()
    {
        // 물리 충돌 끄기 (시체가 플레이어를 밀어내지 않도록)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 핏빛 피격 색상 적용 및 폭발 연출
        if (sr != null) sr.color = hitColor;
        if (deathMotionPrefab != null) Instantiate(deathMotionPrefab, transform.position, Quaternion.identity);

        // 💡 [아이템 드롭!] 몹이 사라지기 전에 드롭 함수를 강제 실행합니다!
        DropItem();

        // 0.1초 동안 사망 연출을 보여준 뒤...
        yield return new WaitForSeconds(0.1f);

        // 드디어 삭제!
        Destroy(gameObject);
    }

    void DropItem()
    {
        // 일반 아이템 드롭 (10% 확률)
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

        // 엘리트 전용 쉴드 아이템 드롭 (설정된 확률)
        if (shieldItemPrefab != null && Random.Range(0f, 100f) <= shieldDropChance)
        {
            Instantiate(shieldItemPrefab, transform.position, Quaternion.identity);
        }
    }
}
