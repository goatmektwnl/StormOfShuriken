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

    [Header("페이즈 1: 공격 설정 (조준 사격)")]
    public GameObject giantSwordAuraPrefab;
    public float attackCooldown = 2f;
    public float attackWaitTime = 0.5f;
    public float fireDelay = 0.2f;

    [Header("페이즈 2: 발악 패턴 (차원 단절 - 백귀야행)")]
    public GameObject phase2BulletPrefab;
    public float phase2BulletSpeedMin = 3f;
    public float phase2BulletSpeedMax = 7f;

    public float phase2SpawnRate = 0.05f;
    public float phase2RiftSize = 6f;

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

    private bool isPhase2 = false;
    private bool isTransitioning = false;

    private float currentAttackTimer;

    private SpriteRenderer sr;
    private Collider2D bossCollider;
    private Animator animator;

    private Coroutine currentPhase1Attack;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        currentAttackTimer = attackCooldown;

        maxHp = hp;
        if (GameManager.instance != null) GameManager.instance.SetBossHp(hp, maxHp);

        // 💡 [최신 문법 적용] FindObjectOfType 대신 가장 빠른 FindAnyObjectByType을 사용합니다!
        BossAppearanceManager appearanceManager = FindAnyObjectByType<BossAppearanceManager>();
        if (appearanceManager != null)
        {
            appearanceManager.MakeEnemiesFlee();
        }
    }

    void Update()
    {
        if (isDead || isTransitioning) return;

        if (isAdvancing)
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
            if (transform.position.x <= enterXPosition) isAdvancing = false;
            return;
        }

        if (!isPhase2 && hp <= maxHp / 2)
        {
            StartCoroutine(Phase2TransitionRoutine());
            return;
        }

        if (!isPhase2 && !isAttacking)
        {
            MoveVertical();
            currentAttackTimer -= Time.deltaTime;
            if (currentAttackTimer <= 0f) currentPhase1Attack = StartCoroutine(TargetedAttackRoutine());
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

    IEnumerator Phase2TransitionRoutine()
    {
        isTransitioning = true;
        if (currentPhase1Attack != null) StopCoroutine(currentPhase1Attack);
        isAttacking = false;

        Vector3 targetPos = new Vector3(enterXPosition, 0f, 0f);
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (animator != null) animator.SetTrigger("doPhase2");
        yield return new WaitForSeconds(1.5f);

        isPhase2 = true;
        isTransitioning = false;
        StartCoroutine(HyakkiYagyoRoutine());
    }

    IEnumerator HyakkiYagyoRoutine()
    {
        isAttacking = true;

        float animationSwingRate = 0.5f;
        float currentAnimTimer = 0f;

        while (!isDead)
        {
            if (phase2BulletPrefab != null)
            {
                float riftSize = phase2RiftSize;
                float xOffset = -3f;
                Vector3 riftCenter = transform.position + new Vector3(xOffset, 0, 0);

                float t = Random.Range(-riftSize, riftSize);
                bool isLineA = Random.value > 0.5f;

                Vector3 spawnPoint = isLineA
                    ? riftCenter + new Vector3(t, t, 0)
                    : riftCenter + new Vector3(t, -t, 0);

                GameObject bullet = Instantiate(phase2BulletPrefab, spawnPoint, Quaternion.identity);
                float randomSpeed = Random.Range(phase2BulletSpeedMin, phase2BulletSpeedMax);

                SwordWave2 waveScript = bullet.GetComponent<SwordWave2>();
                if (waveScript != null)
                {
                    waveScript.speed = randomSpeed;
                    waveScript.SetDirection(Vector3.left);
                }

                currentAnimTimer += phase2SpawnRate;
                if (currentAnimTimer >= animationSwingRate)
                {
                    if (animator != null) animator.SetTrigger("doAttack");
                    currentAnimTimer = 0f;
                }
            }
            yield return new WaitForSeconds(phase2SpawnRate);
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
                if (waveScript != null) waveScript.SetDirection(direction);
            }
        }

        float remainingWait = Mathf.Max(0f, attackWaitTime - fireDelay);
        yield return new WaitForSeconds(remainingWait);
        isAttacking = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isAdvancing || isDead) return;

        if (other.CompareTag("Player"))
        {
            other.SendMessage("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
        }

        if (other.CompareTag("Kunai"))
        {
            TakeDamage(1);
            if (!other.gameObject.name.Contains("Explosion")) Destroy(other.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isAdvancing || isDead) return;
        hp -= damage;
        if (GameManager.instance != null) GameManager.instance.SetBossHp(hp, maxHp);
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
            GameManager.instance.HideBossHp();
            GameManager.instance.AddScore(scoreValue);
            GameManager.instance.AddKill();
            GameManager.instance.TriggerBossDeathSequence();
        }

        StartCoroutine(HitAndDieRoutine());
    }

    IEnumerator HitAndDieRoutine()
    {
        if (bossCollider != null) bossCollider.enabled = false;
        if (sr != null) sr.color = hitColor;

        float timer = 0f;
        float duration = 2.5f;

        while (timer < duration)
        {
            if (deathMotionPrefab != null)
            {
                Vector3 randomOffset = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0);
                Instantiate(deathMotionPrefab, transform.position + randomOffset, Quaternion.identity);
            }
            transform.position += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
            yield return new WaitForSecondsRealtime(0.2f);
            timer += 0.2f;
        }

        Destroy(gameObject);
    }
}