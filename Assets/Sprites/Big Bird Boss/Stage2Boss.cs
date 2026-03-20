using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage2Boss : MonoBehaviour
{
    [Header("보스 능력치")]
    public int maxHp = 50;
    public int currentHp;
    public float entranceSpeed = 3f;

    [Header("페이즈 1: 이동 및 탄막 설정")]
    public float stopXPosition = 6f;
    public float moveSpeed = 2f;
    public float amplitude = 3f;
    public float frequency = 0.5f;
    public GameObject bossKunaiPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    [Range(3, 15)] public int bulletCount = 5;
    [Range(10f, 90f)] public float spreadAngle = 60f;

    // 💡 [신규 추가] 까마귀 요괴 소환 설정
    [Header("페이즈 1: 까마귀 요괴 소환 패턴")]
    public GameObject crowMonsterPrefab;  // 까마귀 요괴 프리팹을 넣을 칸
    private int kunaiFireCount = 0;       // 현재 쿠나이를 몇 번 쐈는지 기억하는 카운터
    private int nextCrowSummonCount;      // 다음 요괴 소환까지 필요한 쿠나이 발사 횟수 (3~5)

    [Header("페이즈 2: 분노 및 돌진 설정")]
    public float diveSpeed = 25f;
    public float returnSpeed = 12f;
    public float minAttackDelay = 3f;
    public float maxAttackDelay = 6f;
    public int dashDamage = 1;

    [Header("페이즈 2: 경고 레이저 설정")]
    public LineRenderer warningLine;
    public float warningTime = 0.5f;

    [Header("화면 가두기 설정")]
    public float screenPadding = 1.0f;

    [Header("사망 및 연출")]
    public GameObject deathEffectPrefab;
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    public enum BossState { Intro, Phase1, Phase2Transition, Phase2Hover, Phase2Dive, Dead }
    public BossState currentState = BossState.Intro;

    private float baseYPosition;
    private float fireTimer;
    private float phase2Timer;
    private bool isDashingDamageActive = false;

    private Transform player;
    private SpriteRenderer sr;
    private Collider2D col;
    private Animator anim;
    private Camera mainCam;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();

        mainCam = Camera.main;
        if (mainCam == null) mainCam = FindAnyObjectByType<Camera>();

        currentHp = maxHp;
        baseYPosition = transform.position.y;
        fireTimer = fireRate;

        // 💡 게임 시작 시 첫 번째 까마귀 소환 타이밍을 3~5 사이로 뽑습니다.
        nextCrowSummonCount = Random.Range(3, 6);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (GameManager.instance != null) GameManager.instance.SetBossHp(currentHp, maxHp);
        if (warningLine != null) warningLine.enabled = false;
    }

    void Update()
    {
        if (currentState == BossState.Dead || currentState == BossState.Phase2Transition) return;

        if (currentState == BossState.Intro) HandleEntrance();
        else if (currentState == BossState.Phase1)
        {
            HandleMovement();
            HandleAttack();
        }
        else if (currentState == BossState.Phase2Hover)
        {
            HandleMovement();
            HandlePhase2Wait();
        }
    }

    void LateUpdate()
    {
        if (currentState == BossState.Intro || currentState == BossState.Dead || currentState == BossState.Phase2Dive) return;
        ClampToScreen();
    }

    void ClampToScreen()
    {
        if (mainCam == null) return;
        Vector2 minBounds = mainCam.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 maxBounds = mainCam.ViewportToWorldPoint(new Vector2(1, 1));

        float clampedX = Mathf.Clamp(transform.position.x, minBounds.x + screenPadding, maxBounds.x - screenPadding);
        float clampedY = Mathf.Clamp(transform.position.y, minBounds.y + screenPadding, maxBounds.y - screenPadding);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    void HandleEntrance()
    {
        transform.position += Vector3.left * entranceSpeed * Time.deltaTime;
        if (transform.position.x <= stopXPosition)
        {
            transform.position = new Vector3(stopXPosition, transform.position.y, 0f);
            currentState = BossState.Phase1;
        }
    }

    void HandleMovement()
    {
        float newY = baseYPosition + Mathf.Sin(Time.time * frequency * Mathf.PI * 2) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, 0f);
    }

    // 💡 [핵심 수정] 3~5회 탄막을 쏘면 까마귀를 소환하도록 뇌를 개조했습니다!
    void HandleAttack()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            if (kunaiFireCount >= nextCrowSummonCount)
            {
                SummonCrowMonsters();
                kunaiFireCount = 0; // 카운터 초기화
                nextCrowSummonCount = Random.Range(3, 6); // 다음 소환 타이밍 재설정 (3, 4, 5회 중 하나)

                // 까마귀 소환 후에는 딜레이를 살짝 길게 주어 유저가 대처할 시간을 줍니다.
                fireTimer = fireRate * 1.5f;
            }
            else
            {
                FireSpreadKunai();
                kunaiFireCount++; // 쐈으니 1 증가
                fireTimer = fireRate;
            }
        }
    }

    // 💡 [신규] 까마귀 요괴 위아래 동시 소환 함수
    void SummonCrowMonsters()
    {
        if (crowMonsterPrefab == null) return;

        // 1. 위쪽 극점 좌표 (기본 높이 + 진폭)
        Vector3 topSpawnPos = new Vector3(transform.position.x, baseYPosition + amplitude, 0f);
        // 2. 아래쪽 극점 좌표 (기본 높이 - 진폭)
        Vector3 bottomSpawnPos = new Vector3(transform.position.x, baseYPosition - amplitude, 0f);

        // 생성!
        Instantiate(crowMonsterPrefab, topSpawnPos, Quaternion.identity);
        Instantiate(crowMonsterPrefab, bottomSpawnPos, Quaternion.identity);

        if (anim != null) anim.SetTrigger("OnAttack"); // 소환 모션이 있다면 사용
    }

    void FireSpreadKunai()
    {
        if (bossKunaiPrefab == null) return;
        Vector3 spawnPosition = (firePoint != null) ? firePoint.position : transform.position;

        if (bulletCount <= 1)
        {
            Instantiate(bossKunaiPrefab, spawnPosition, Quaternion.identity);
            return;
        }

        float centerAngle = 180f;
        float startAngle = centerAngle - (spreadAngle / 2f);
        float angleStep = spreadAngle / (bulletCount - 1);

        for (int i = 0; i < bulletCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            float dirX = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
            Vector3 fireDirection = new Vector3(dirX, dirY, 0).normalized;

            GameObject kunai = Instantiate(bossKunaiPrefab, spawnPosition, Quaternion.identity);
            BossKunai kunaiScript = kunai.GetComponent<BossKunai>();
            if (kunaiScript != null) kunaiScript.SetDirection(fireDirection);
            kunai.transform.rotation = Quaternion.Euler(0, 0, currentAngle + 180f);
        }
    }

    void HandlePhase2Wait()
    {
        phase2Timer -= Time.deltaTime;
        if (phase2Timer <= 0 && player != null)
        {
            StartCoroutine(ClawDiveRoutine());
        }
    }

    IEnumerator ClawDiveRoutine()
    {
        currentState = BossState.Phase2Dive;

        Vector3 startPosition = new Vector3(stopXPosition, baseYPosition, 0f);
        Vector3 targetPos = new Vector3(player.position.x, player.position.y, 0f);
        Vector3 dir = (targetPos - transform.position).normalized;

        Vector3 dashEndPos = targetPos + dir * 5f;

        if (mainCam != null)
        {
            Vector2 minBounds = mainCam.ViewportToWorldPoint(new Vector2(0, 0));
            Vector2 maxBounds = mainCam.ViewportToWorldPoint(new Vector2(1, 1));
            dashEndPos.x = Mathf.Clamp(dashEndPos.x, minBounds.x + screenPadding, maxBounds.x - screenPadding);
            dashEndPos.y = Mathf.Clamp(dashEndPos.y, minBounds.y + screenPadding, maxBounds.y - screenPadding);
        }
        dashEndPos.z = 0f;

        if (anim != null) anim.SetTrigger("OnAnticipation");

        if (warningLine != null)
        {
            warningLine.enabled = true;
            warningLine.SetPosition(0, transform.position);
            warningLine.SetPosition(1, dashEndPos);
        }

        yield return new WaitForSeconds(warningTime);

        if (warningLine != null) warningLine.enabled = false;
        if (anim != null) anim.SetTrigger("OnAttack");
        isDashingDamageActive = true;

        float timer = 0;

        while (timer < 1.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, dashEndPos, diveSpeed * Time.deltaTime);
            timer += Time.deltaTime;

            if (Vector2.Distance(transform.position, dashEndPos) < 0.1f) break;

            yield return null;
        }

        isDashingDamageActive = false;
        if (anim != null) anim.SetTrigger("OnRecovery");
        yield return new WaitForSeconds(1.0f);

        if (anim != null) anim.SetTrigger("OnIdle");

        float returnTimer = 0;

        while (Vector2.Distance(transform.position, startPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, returnSpeed * Time.deltaTime);
            returnTimer += Time.deltaTime;
            if (returnTimer > 3.0f) break;
            yield return null;
        }

        transform.position = startPosition;
        phase2Timer = Random.Range(minAttackDelay, maxAttackDelay);
        currentState = BossState.Phase2Hover;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == BossState.Intro || currentState == BossState.Dead) return;

        if (other.CompareTag("Kunai"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Player") && isDashingDamageActive)
        {
            other.SendMessage("TakeDamage", dashDamage, SendMessageOptions.DontRequireReceiver);
            isDashingDamageActive = false;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        if (GameManager.instance != null) GameManager.instance.SetBossHp(currentHp, maxHp);

        if (currentHp <= 0) Die();
        else
        {
            StartCoroutine(HitFlashRoutine());
            if (currentHp <= maxHp / 2 && currentState == BossState.Phase1)
            {
                StartCoroutine(Phase2TransitionRoutine());
            }
        }
    }

    IEnumerator Phase2TransitionRoutine()
    {
        currentState = BossState.Phase2Transition;
        if (warningLine != null) warningLine.enabled = false;

        for (int i = 0; i < 5; i++)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);
        phase2Timer = 1f;
        currentState = BossState.Phase2Hover;
    }

    IEnumerator HitFlashRoutine()
    {
        if (sr != null) sr.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        if (sr != null) sr.color = Color.white;
    }

    void Die()
    {
        currentState = BossState.Dead;
        col.enabled = false;
        if (warningLine != null) warningLine.enabled = false;

        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(5000);
            GameManager.instance.HideBossHp();
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        if (deathEffectPrefab != null) Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        if (sr != null) sr.color = hitColor;

        yield return new WaitForSecondsRealtime(2.0f);

        if (GameManager.instance != null) GameManager.instance.ShowStageClear();

        Destroy(gameObject);
    }
}