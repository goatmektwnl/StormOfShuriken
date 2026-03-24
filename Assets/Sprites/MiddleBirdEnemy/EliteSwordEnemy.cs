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
    // 💡 여기에 분신 소멸 애니메이션 프리팹을 연결해주세요!
    public GameObject deathEffectPrefab;
    public Color hitColor = Color.red;

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

        if (spriteRenderer != null) spriteRenderer.color = Color.magenta;
        yield return new WaitForSeconds(aimReadyTime);

        if (spriteRenderer != null) spriteRenderer.color = Color.white;
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
        if (spriteRenderer != null) spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    

    // 💡 [핵심] 일반적인 사망 처리 (쿠나이에 맞았을 때)
    void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();

        // 물리 판정 끄고 즉시 "펑!"
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        if (GameManager.instance != null) GameManager.instance.AddScore(500);

        
        Destroy(gameObject); // 이펙트가 생겼으니 즉시 파괴
    }

    // 💡 [도망 연출] 보스 등장 시 호출되는 함수
    public void Flee()
    {
        if (isDead) return;
        isDead = true; // 아이템을 흘리지 않게 죽은 상태로 처리
        StopAllCoroutines();

        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        // 보스 매니저가 이펙트를 소환할 것이므로 본체만 조용히 삭제
        Destroy(gameObject);
    }
}