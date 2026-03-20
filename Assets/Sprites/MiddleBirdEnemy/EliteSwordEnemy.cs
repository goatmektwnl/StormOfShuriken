using System.Collections;
using UnityEngine;

public class EliteSwordEnemy : MonoBehaviour
{
    [Header("능력치 세팅")]
    public int maxHp = 5;
    private int currentHp;
    public int contactDamage = 1;

    [Header("이동 속도 세팅")]
    public float entranceSpeed = 3f;
    public float dashSpeed = 20f;
    public float returnSpeed = 12f;

    [Header("패턴 타이밍 (초)")]
    public float stopAfterEntranceTime = 1.0f;
    public float aimReadyTime = 0.5f;
    public float dashPostDelay = 1.0f;
    public float attackCooldown = 3.0f;

    [Header("거리 및 위치 설정")]
    public float stopXPosition = 8.0f;

    [Header("그리기 및 연출")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public GameObject deathEffectPrefab;
    public Color hitColor = Color.red;

    // 💡 [핵심 추가] 엘리트 몹의 전리품 설정 칸입니다! (EliteEnemy 참고)
    [Header("전리품(아이템) 설정")]
    public GameObject[] buffItemPrefabs;
    public GameObject shieldItemPrefab;
    [Range(0, 100)] public float shieldDropChance = 20f;

    private enum EnemyState { Spawning, ReadyToDash, Dashing, PostDashDelay, Returning, Dead }
    [SerializeField] private EnemyState currentState = EnemyState.Spawning;

    private Transform player;
    private Vector2 startPos;
    private bool isDead = false;
    private bool isDashingDamageActive = false;

    void Start()
    {
        currentHp = maxHp;
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        startPos = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        StartCoroutine(EliteEnemyBrain());
    }

    IEnumerator EliteEnemyBrain()
    {
        yield return StartCoroutine(EntranceRoutine());

        while (!isDead && player != null)
        {
            yield return StartCoroutine(AimReadyRoutine());
            yield return StartCoroutine(DashRoutine());
            yield return StartCoroutine(PostDashRoutine());
            yield return StartCoroutine(ReturnRoutine());
            yield return new WaitForSeconds(attackCooldown);
        }
    }

    IEnumerator EntranceRoutine()
    {
        currentState = EnemyState.Spawning;
        SetAnimation("isWalking", true);

        while (transform.position.x > stopXPosition)
        {
            if (isDead) yield break;
            transform.Translate(Vector3.left * entranceSpeed * Time.deltaTime);
            yield return null;
        }

        SetAnimation("isWalking", false);
        yield return new WaitForSeconds(stopAfterEntranceTime);
    }

    IEnumerator AimReadyRoutine()
    {
        currentState = EnemyState.ReadyToDash;
        SetAnimation("isAiming", true);

        spriteRenderer.color = Color.magenta;
        yield return new WaitForSeconds(aimReadyTime);

        spriteRenderer.color = Color.white;
        SetAnimation("isAiming", false);
    }

    IEnumerator DashRoutine()
    {
        currentState = EnemyState.Dashing;
        SetAnimation("isAttacking", true);

        isDashingDamageActive = true;

        Vector3 targetPos = player.position;
        Vector3 dir = (targetPos - transform.position).normalized;
        Vector3 dashEndPos = targetPos + dir * 3.0f;
        dashEndPos.z = 0;

        float timer = 0;

        while (timer < 1.0f)
        {
            if (isDead) yield break;
            transform.position = Vector3.MoveTowards(transform.position, dashEndPos, dashSpeed * Time.deltaTime);
            timer += Time.deltaTime;

            if (Vector2.Distance(transform.position, dashEndPos) < 0.1f) break;
            yield return null;
        }

        isDashingDamageActive = false;
        SetAnimation("isAttacking", false);
    }

    IEnumerator PostDashRoutine()
    {
        currentState = EnemyState.PostDashDelay;
        yield return new WaitForSeconds(dashPostDelay);
    }

    IEnumerator ReturnRoutine()
    {
        currentState = EnemyState.Returning;
        SetAnimation("isWalking", true);

        Vector3 returnTarget = new Vector3(stopXPosition, startPos.y, 0f);

        while (Vector2.Distance(transform.position, returnTarget) > 0.1f)
        {
            if (isDead) yield break;
            transform.position = Vector3.MoveTowards(transform.position, returnTarget, returnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = returnTarget;
        SetAnimation("isWalking", false);
    }

    void SetAnimation(string paramName, bool value)
    {
        if (animator == null) return;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                animator.SetBool(paramName, value);
                return;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerCollision(other);

        if (other.CompareTag("Kunai"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        HandlePlayerCollision(other);
    }

    void HandlePlayerCollision(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Player") && isDashingDamageActive)
        {
            isDashingDamageActive = false;
            other.SendMessage("TakeDamage", contactDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHp -= damage;
        StartCoroutine(HitFlashRoutine());

        if (currentHp <= 0) Die();
    }

    IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    // 💡 [핵심 추가] 기존 스크립트의 DropItem 로직을 완벽 이식했습니다!
    void DropItem()
    {
        // 1. 버프 아이템 배열 검사 (각각 10% 확률로 개별 드롭 판정)
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

        // 2. 쉴드 아이템 드롭 판정 (인스펙터에서 설정한 확률 기반)
        if (shieldItemPrefab != null && Random.Range(0f, 100f) <= shieldDropChance)
        {
            Instantiate(shieldItemPrefab, transform.position, Quaternion.identity);
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();

        GetComponent<Collider2D>().enabled = false;
        if (deathEffectPrefab != null) Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        if (GameManager.instance != null) GameManager.instance.AddScore(500);

        // 💡 사망 직전에 전리품을 바닥에 뿌립니다!
        DropItem();

        Destroy(gameObject, 0.3f);
    }
}