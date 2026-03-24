using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("생명력 설정")]
    public int maxLives = 3;
    private int currentLives;
    private bool isGameOver = false;

    [Header("UI 목숨 설정")]
    public Transform heartContainer;
    public GameObject heartPrefab;

    private List<HeartController> hearts = new List<HeartController>();

    [Header("Invincibility (피격 무적)")]
    public float invincibilityDuration = 1.5f;
    public float blinkInterval = 0.15f;
    private bool isInvincible = false;

    private SpriteRenderer spriteRenderer;
    private PlayerBuffController buffController;

    void Start()
    {
        currentLives = maxLives;
        spriteRenderer = GetComponent<SpriteRenderer>();
        buffController = GetComponent<PlayerBuffController>();

        InitializeHearts();
    }

    void InitializeHearts()
    {
        foreach (Transform child in heartContainer) { Destroy(child.gameObject); }
        hearts.Clear();

        for (int i = 0; i < maxLives; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartContainer);
            HeartController hc = newHeart.GetComponent<HeartController>();
            if (hc != null) hearts.Add(hc);
        }
    }

    void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // 💡 [핵심 1] 엘리트 몹(또는 보스)이 "데미지 받아라!" 하고 신호를 보낼 때 작동하는 전용 수신기입니다!
    // 💡 수정된 TakeDamage 함수
    public void TakeDamage(int damage)
    {
        if (isGameOver || isInvincible) return;

        PlayerBuff myBuff = GetComponent<PlayerBuff>();

        // 🛡️ 1순위 방어: 쉴드
        if (myBuff != null && myBuff.hasShield == true)
        {
            myBuff.BreakShield();
            StartCoroutine(BlinkAndInvincibility()); // 🚨 필수 추가: 쉴드 깨져도 무적 부여!
            return;
        }

        // 👥 2순위 방어: 분신 대타 출동
        if (buffController != null && buffController.SacrificeClone())
        {
            Debug.Log("대타출동! 분신이 플레이어 대신 희생했습니다.");
            StartCoroutine(BlinkAndInvincibility());
            return;
        }

        // 💔 3순위: 본체 피격
        int previousLives = currentLives;
        currentLives -= damage;

        for (int i = previousLives - 1; i >= currentLives; i--)
        {
            if (i >= 0 && i < hearts.Count)
            {
                hearts[i].BreakHeart();
            }
        }

        if (currentLives <= 0) Die();
        else StartCoroutine(BlinkAndInvincibility());
    }

    // 💡 수정된 OnTriggerEnter2D 함수
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver || isInvincible) return;

        // 🚨 대문자 "Boss"와 폭탄("Bomb" 등) 태그 정확히 추가!
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet") || other.CompareTag("Boss") || other.CompareTag("Bomb"))
        {
            // 엘리트 몹은 돌진 공격 중 자체 스크립트에서 데미지를 주므로 여기선 무시
            if (other.GetComponent<EliteSwordEnemy>() != null) return;

            PlayerBuff myBuff = GetComponent<PlayerBuff>();

            // 🛡️ 1순위 방어: 쉴드 버프
            if (myBuff != null && myBuff.hasShield == true)
            {
                myBuff.BreakShield(); // 쉴드 파괴

                // 보스나 탄막에 맞았을 때 관통되지 않도록 투사체 파괴
                if (other.CompareTag("EnemyBullet")) Destroy(other.gameObject);

                StartCoroutine(BlinkAndInvincibility()); // 🚨 핵심: 쉴드 깨져도 무조건 무적 시간 부여!
                return;
            }

            // 💡 대문자 "Boss" 태그로 정확히 식별
            bool isBoss = (other.GetComponent<Stage2Boss>() != null) || other.CompareTag("Boss");

            // 👥 2순위 방어: 분신 희생
            if (buffController != null && buffController.SacrificeClone())
            {
                Debug.Log("대타출동! 분신이 플레이어 대신 희생했습니다.");
                if (!isBoss) Destroy(other.gameObject); // 보스가 아닐 때만 파괴
                StartCoroutine(BlinkAndInvincibility());
                return;
            }

            // 💔 3순위: 본체 피격
            currentLives--;

            if (currentLives >= 0 && currentLives < hearts.Count)
            {
                hearts[currentLives].BreakHeart();
            }

            if (!isBoss) Destroy(other.gameObject); // 보스가 아닐 때만 파괴

            if (currentLives <= 0) Die();
            else StartCoroutine(BlinkAndInvincibility());
        }
    }

    IEnumerator BlinkAndInvincibility()
    {
        isInvincible = true;
        float timer = 0f;
        while (timer < invincibilityDuration)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.3f);
            yield return new WaitForSeconds(blinkInterval);
            spriteRenderer.color = new Color(1, 1, 1, 1f);
            yield return new WaitForSeconds(blinkInterval);
            timer += (blinkInterval * 2f);
        }
        spriteRenderer.color = new Color(1, 1, 1, 1f);
        isInvincible = false;
    }

    void Die()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        if (GameManager.instance != null) GameManager.instance.GameOver();
    }
}