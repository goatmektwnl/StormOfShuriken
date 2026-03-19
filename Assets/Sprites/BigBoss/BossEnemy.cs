using System.Collections;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [Header("능력치")]
    public int hp = 1000;
    private int maxHp;

    public float moveSpeed = 4f;

    [Header("이동 및 배회 구역")]
    public float enterXPosition = 7f;
    public float minY = -4f;
    public float maxY = 4f;

    [Header("공격 설정 (플레이어 조준 사격!)")]
    public GameObject giantSwordAuraPrefab;
    public float attackCooldown = 2f;
    public float attackWaitTime = 0.5f;
    public float fireDelay = 0.2f;

    [Header("사망 및 전리품")]
    public GameObject deathMotionPrefab;
    public int scoreValue = 10000;

    [Header("피격 효과 설정")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    private bool isAdvancing = true;
    private bool movingUp = true;
    private bool isAttacking = false;
    private bool isDead = false;

    private float currentAttackTimer;

    private SpriteRenderer sr;
    private Collider2D bossCollider;
    private Animator animator;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        currentAttackTimer = attackCooldown;

        maxHp = hp;
        if (GameManager.instance != null)
        {
            GameManager.instance.SetBossHp(hp, maxHp);
        }
    }

    void Update()
    {
        if (isDead) return;

        if (isAdvancing)
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
            if (transform.position.x <= enterXPosition)
            {
                isAdvancing = false;
            }
            return;
        }

        if (!isAttacking)
        {
            MoveVertical();

            currentAttackTimer -= Time.deltaTime;
            if (currentAttackTimer <= 0f)
            {
                StartCoroutine(TargetedAttackRoutine());
            }
        }
    }

    void MoveVertical()
    {
        if (movingUp)
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
            if (transform.position.y >= maxY)
            {
                transform.position = new Vector3(transform.position.x, maxY, 0);
                movingUp = false;
            }
        }
        else
        {
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
            if (transform.position.y <= minY)
            {
                transform.position = new Vector3(transform.position.x, minY, 0);
                movingUp = true;
            }
        }
    }

    IEnumerator TargetedAttackRoutine()
    {
        isAttacking = true;
        currentAttackTimer = attackCooldown;

        if (animator != null) animator.SetTrigger("doAttack");

        yield return new WaitForSeconds(fireDelay);

        if (giantSwordAuraPrefab != null)
        {
            GameObject aura = Instantiate(giantSwordAuraPrefab, transform.position, Quaternion.identity);

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 direction = (player.transform.position - transform.position).normalized;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                aura.transform.rotation = Quaternion.Euler(0, 0, angle + 180f);

                SwordWave waveScript = aura.GetComponent<SwordWave>();
                if (waveScript != null)
                {
                    waveScript.SetDirection(direction);
                }
            }
        }

        float remainingWait = Mathf.Max(0f, attackWaitTime - fireDelay);
        yield return new WaitForSeconds(remainingWait);

        isAttacking = false;
    }

    public void ShootGiantSwordAura() { }

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
        if (isAdvancing || isDead) return;

        hp--;

        if (GameManager.instance != null)
        {
            GameManager.instance.SetBossHp(hp, maxHp);
        }

        if (hp > 0) StartCoroutine(HitFlashRoutine());
        else Die();
    }

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
            GameManager.instance.HideBossHp();
            GameManager.instance.AddScore(scoreValue);
            GameManager.instance.AddKill();

            // 💡 1. GameManager에게 시네마틱 연출(잡몹 소멸 + 슬로우 모션) 시작을 명령합니다!
            GameManager.instance.TriggerBossDeathSequence();
        }

        StartCoroutine(HitAndDieRoutine());
    }

    IEnumerator HitAndDieRoutine()
    {
        if (bossCollider != null) bossCollider.enabled = false;
        if (sr != null) sr.color = hitColor;

        float timer = 0f;
        float duration = 2.5f; // GameManager의 대기 시간(2.5초)과 동일하게 맞춥니다.

        while (timer < duration)
        {
            if (deathMotionPrefab != null)
            {
                Vector3 randomOffset = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0);
                Instantiate(deathMotionPrefab, transform.position + randomOffset, Quaternion.identity);
            }
            transform.position += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);

            // 💡 2. TimeScale이 0.1f로 느려져도 폭발 연출은 현실 시간 속도로 화려하게 터지도록 보정합니다.
            yield return new WaitForSecondsRealtime(0.2f);
            timer += 0.2f;
        }

        // 💡 3. GameManager 쪽에서 StageClear를 띄우기로 했으므로 자신만 파괴하고 퇴장합니다.
        Destroy(gameObject);
    }
}