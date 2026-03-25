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
    public void TakeDamage(int damage)
    {
        if (isGameOver || isInvincible) return;

        PlayerBuff myBuff = GetComponent<PlayerBuff>();

        // 🛡️ 1순위 방어: 쉴드
        if (myBuff != null && myBuff.hasShield == true)
        {
            myBuff.BreakShield();
            return;
        }

        // 👥 2순위 방어: 분신 대타 출동
        if (buffController != null && buffController.SacrificeClone())
        {
            Debug.Log("대타출동! 분신이 플레이어 대신 희생했습니다.");
            StartCoroutine(BlinkAndInvincibility());
            return;
        }

        // 💔 3순위: 본체 피격 (요청받은 데미지만큼 체력을 깎습니다)
        int previousLives = currentLives;
        currentLives -= damage;

        // 💡 10년 차의 디테일: 만약 데미지가 2 이상 들어올 경우를 대비해 깎인 만큼 하트를 깹니다.
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver || isInvincible) return;

        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
        {
            // 💡 [핵심 2] 엘리트 몹은 그냥 스치기만 해서는 데미지를 주지 않고, 터지지도 않습니다!
            // (엘리트 몹은 '돌진 애니메이션' 중일 때만 위쪽의 TakeDamage()를 알아서 호출할 것입니다.)
            if (other.GetComponent<EliteSwordEnemy>() != null) return;

            PlayerBuff myBuff = GetComponent<PlayerBuff>();

            // 🛡️ 1순위 방어: 쉴드 버프
            if (myBuff != null && myBuff.hasShield == true)
            {
                myBuff.BreakShield();
                if (other.CompareTag("EnemyBullet")) Destroy(other.gameObject);
                return;
            }

            // 💡 2스테이지 보스 확인
            bool isBoss = (other.GetComponent<Stage2Boss>() != null);

            // 👥 2순위 방어: 분신 희생
            if (buffController != null && buffController.SacrificeClone())
            {
                Debug.Log("대타출동! 분신이 플레이어 대신 희생했습니다.");

                // 부딪힌 녀석이 보스가 '아닐 때만' 파괴
                if (!isBoss) Destroy(other.gameObject);

                StartCoroutine(BlinkAndInvincibility());
                return;
            }

            // 💔 3순위: 본체 피격
            currentLives--;

            if (currentLives >= 0 && currentLives < hearts.Count)
            {
                hearts[currentLives].BreakHeart();
            }

            // 부딪힌 녀석이 보스가 '아닐 때만' 파괴
            if (!isBoss) Destroy(other.gameObject);

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