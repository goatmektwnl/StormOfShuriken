using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage2Boss : MonoBehaviour
{
    [Header("보스 능력치")]
    public int maxHp = 50;
    public int currentHp;
    public float entranceSpeed = 3f;

    [Header("기본 이동 및 분열탄 설정")]
    public float stopXPosition = 6f;
    public float moveSpeed = 2f;
    public float amplitude = 3f;
    public float frequency = 0.5f;
    public GameObject splitKunaiPrefab;
    public Transform firePoint;

    [Header("패턴 쿨타임 설정")]
    public float minAttackDelay = 2.5f;
    public float maxAttackDelay = 4.5f;

    [Header("공통: 분노 및 돌진 설정")]
    public float diveSpeed = 25f;
    public float returnSpeed = 12f;
    public int dashDamage = 1;
    public LineRenderer warningLine;
    public float warningTime = 0.5f;

    [Header("페이즈 2: 공중 폭격 설정")]
    public GameObject bombPrefab;
    public float bombingDuration = 4f;
    public float bombDropInterval = 0.3f;

    [Header("화면 가두기 설정")]
    public float screenPadding = 1.0f;

    [Header("사망 및 연출")]
    public GameObject deathEffectPrefab;
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    public enum BossState { Intro, Combat, Phase2Transition, Dead }
    public BossState currentState = BossState.Intro;

    private float baseYPosition;
    private float patternTimer;
    private bool isAttacking = false;
    private bool isPhase2 = false;
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

        patternTimer = Random.Range(minAttackDelay, maxAttackDelay);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (GameManager.instance != null) GameManager.instance.SetBossHp(currentHp, maxHp);
        if (warningLine != null) warningLine.enabled = false;

        BossAppearanceManager appearanceManager = FindAnyObjectByType<BossAppearanceManager>();
        if (appearanceManager != null) appearanceManager.MakeEnemiesFlee();
    }

    void Update()
    {
        if (currentState == BossState.Dead || currentState == BossState.Phase2Transition) return;

        if (currentState == BossState.Intro)
        {
            HandleEntrance();
        }
        else if (currentState == BossState.Combat)
        {
            if (!isAttacking)
            {
                HandleMovement();

                patternTimer -= Time.deltaTime;
                if (patternTimer <= 0)
                {
                    ChooseNextPattern();
                }
            }
        }
    }

    void LateUpdate()
    {
        if (currentState == BossState.Intro || currentState == BossState.Dead || !col.enabled) return;
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
            currentState = BossState.Combat;
        }
    }

    void HandleMovement()
    {
        float newY = baseYPosition + Mathf.Sin(Time.time * frequency * Mathf.PI * 2) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, 0f);
    }

    void ChooseNextPattern()
    {
        isAttacking = true;

        int patternChoice = isPhase2 ? Random.Range(0, 3) : Random.Range(0, 2);

        if (patternChoice == 0) StartCoroutine(SplitKunaiRoutine());
        else if (patternChoice == 1) StartCoroutine(ClawDiveRoutine());
        else StartCoroutine(BombingRoutine());
    }

    void EndPattern()
    {
        isAttacking = false;
        patternTimer = Random.Range(minAttackDelay, maxAttackDelay);

        if (anim != null)
        {
            anim.ResetTrigger("OnAttack");
            anim.ResetTrigger("OnAnticipation");
            anim.ResetTrigger("OnRecovery");
            anim.SetTrigger("OnIdle");
        }

        if (currentState != BossState.Dead) col.enabled = true;
    }

    IEnumerator SplitKunaiRoutine()
    {
        if (anim != null) anim.SetTrigger("OnAttack");

        yield return new WaitForSeconds(0.2f);

        if (splitKunaiPrefab != null && player != null)
        {
            Vector3 spawnPosition = (firePoint != null) ? firePoint.position : transform.position;
            Vector3 directionToPlayer = (player.position - spawnPosition).normalized;

            GameObject kunai = Instantiate(splitKunaiPrefab, spawnPosition, Quaternion.identity);
            BossKunaiSplit splitScript = kunai.GetComponent<BossKunaiSplit>();
            if (splitScript != null)
            {
                splitScript.SetDirection(directionToPlayer);
                splitScript.timeToSplit = Random.Range(0.8f, 1.4f);
            }
        }

        // 💡 [추가] 사진의 애니메이터 흐름에 맞춰 Recovery 트리거를 실행합니다.
        yield return new WaitForSeconds(0.5f);
        if (anim != null) anim.SetTrigger("OnRecovery");
        yield return new WaitForSeconds(0.5f);

        EndPattern();
    }

    IEnumerator ClawDiveRoutine()
    {
        Vector3 startPosition = new Vector3(stopXPosition, baseYPosition, 0f);
        Vector3 targetPos = (player != null) ? player.position : Vector3.zero;
        Vector3 dir = (targetPos - transform.position).normalized;
        Vector3 dashEndPos = targetPos + dir * 5f;
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

        float returnTimer = 0;
        while (Vector2.Distance(transform.position, startPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, returnSpeed * Time.deltaTime);
            returnTimer += Time.deltaTime;
            if (returnTimer > 3.0f) break;
            yield return null;
        }

        transform.position = startPosition;
        EndPattern();
    }

    IEnumerator BombingRoutine()
    {
        col.enabled = false;
        if (anim != null) anim.SetTrigger("OnAnticipation");

        // 보스가 화면 밖으로 완전히 사라지도록 높이 수정 (1.6f)
        float flyUpY = (mainCam != null) ? mainCam.ViewportToWorldPoint(new Vector2(0, 1.6f)).y : 15f;
        Vector3 flyUpTarget = new Vector3(transform.position.x, flyUpY, 0f);

        while (Vector3.Distance(transform.position, flyUpTarget) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, flyUpTarget, diveSpeed * Time.deltaTime);
            yield return null;
        }

        float bombTimer = 0;
        while (bombTimer < bombingDuration)
        {
            if (bombPrefab != null && mainCam != null)
            {
                Vector2 minB = mainCam.ViewportToWorldPoint(new Vector2(0.1f, 0.1f));
                Vector2 maxB = mainCam.ViewportToWorldPoint(new Vector2(0.9f, 0.9f));
                float randX = Random.Range(minB.x, maxB.x);
                float randY = Random.Range(minB.y, maxB.y);

                Vector3 spawnPos = new Vector3(randX, flyUpY, 0f);
                GameObject bombObj = Instantiate(bombPrefab, spawnPos, Quaternion.identity);
                Bomb bombScript = bombObj.GetComponent<Bomb>();
                if (bombScript != null) bombScript.groundYPosition = randY;
            }
            yield return new WaitForSeconds(bombDropInterval);
            bombTimer += bombDropInterval;
        }

        Vector3 returnPos = new Vector3(stopXPosition, baseYPosition, 0f);
        while (Vector3.Distance(transform.position, returnPos) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, returnPos, diveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = returnPos;

        // 💡 Recovery 단계를 명확히 거치도록 수정
        if (anim != null) anim.SetTrigger("OnRecovery");
        yield return new WaitForSeconds(0.5f);

        EndPattern();
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

            if (currentHp <= maxHp / 2 && !isPhase2)
            {
                isPhase2 = true;
                StartCoroutine(Phase2TransitionRoutine());
            }
        }
    }

    IEnumerator Phase2TransitionRoutine()
    {
        currentState = BossState.Phase2Transition;
        col.enabled = false;

        if (anim != null)
        {
            anim.ResetTrigger("OnAttack");
            anim.ResetTrigger("OnAnticipation");
            anim.SetTrigger("OnIdle");
        }

        for (int i = 0; i < 5; i++)
        {
            sr.color = Color.red; yield return new WaitForSeconds(0.1f);
            sr.color = Color.white; yield return new WaitForSeconds(0.1f);
        }

        col.enabled = true;
        currentState = BossState.Combat;
        EndPattern();
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
        StopAllCoroutines();
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