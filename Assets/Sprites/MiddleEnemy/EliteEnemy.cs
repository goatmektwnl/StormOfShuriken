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

    [Header("사망 및 연출")]
    // 💡 분신 아이템에서 쓰던 그 "펑!" 터지는 애니메이션 프리팹을 여기에 넣어주세요!
    public GameObject deathMotionPrefab;
    public GameObject[] buffItemPrefabs;

    public GameObject shieldItemPrefab;
    [Range(0, 100)] public float shieldDropChance = 20f;

    [Header("피격 효과 설정")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

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

            if (transform.position.x <= enterXPosition)
            {
                isAdvancing = false;
                isInvincible = false;

                SetNextWanderPosition();
                if (animator != null) animator.SetBool("isMoving", true);
                StartCoroutine(AttackRoutine());
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPos) < 0.1f) SetNextWanderPosition();
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
        if (isDead) return;

        if (other.CompareTag("Kunai"))
        {
            if (isInvincible)
            {
                Destroy(other.gameObject);
                return;
            }
            TakeDamage(1); // TakeHit 대신 통일된 TakeDamage 호출
            Destroy(other.gameObject);
        }
    }

    // 💡 [수정] 외부 연동을 위해 TakeDamage 함수를 명시적으로 추가했습니다.
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        hp -= damage;
        if (hp > 0) StartCoroutine(HitFlashRoutine());
        else Die();
    }

    public void TakeHit() { TakeDamage(1); }

    IEnumerator HitFlashRoutine()
    {
        if (sr != null) sr.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
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

        // 💡 [수정] 즉시 "펑!" 하고 터뜨립니다.
        if (deathMotionPrefab != null)
        {
            Instantiate(deathMotionPrefab, transform.position, Quaternion.identity);
        }

        
        Destroy(gameObject); // 본체 즉시 삭제
    }

    

    // 💡 [최종 수정] 보스 등장 시 매니저가 이 함수를 호출하면, 
    // 아이템을 드롭하지 않고 즉시 "펑!" 효과를 내며 조용히 사라집니다.
    public void Flee()
    {
        if (isDead) return;
        isDead = true; // 아이템 미드롭 처리를 위해 죽은 상태로 전환
        StopAllCoroutines();

        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        // 보스전 연출용 즉시 삭제
        Destroy(gameObject);
    }
}