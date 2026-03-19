using System.Collections;
using System.Collections.Generic; // 💡 List 사용을 위해 필수!
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

    // 💡 [신규] 분신(대타) 컨트롤러를 조종할 리모컨 변수입니다!
    private PlayerBuffController buffController;

    void Start()
    {
        currentLives = maxLives;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 💡 게임이 시작될 때 자신에게 붙어있는 PlayerBuffController를 자동으로 찾아옵니다.
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver || isInvincible) return;

        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
        {
            PlayerBuff myBuff = GetComponent<PlayerBuff>();

            // 🛡️ 1순위 방어: 쉴드 버프가 있는지 가장 먼저 확인!
            if (myBuff != null && myBuff.hasShield == true)
            {
                myBuff.BreakShield();
                // 💡 [수정됨] BossAttack을 지우고, EnemyBullet 하나만 파괴하도록 단순화했습니다!
                if (other.CompareTag("EnemyBullet")) Destroy(other.gameObject);
                return;
            }

            // 👥 2순위 방어: [핵심 신규 로직] 쉴드가 깨졌다면, 희생할 분신이 있는지 확인!
            if (buffController != null && buffController.SacrificeClone())
            {
                Debug.Log("대타출동! 분신이 플레이어 대신 희생했습니다.");

                // 분신이 터졌으므로, 부딪힌 적이나 총알도 함께 파괴해 줍니다. (원래 로직 유지)
                Destroy(other.gameObject);

                // 분신이 터지는 동안 본체가 연속으로 맞는 것을 막기 위해 짧은 무적(깜빡임)을 줍니다.
                StartCoroutine(BlinkAndInvincibility());

                return; // 💡 여기서 함수를 즉시 종료시켜 본체의 하트가 깎이는 것을 완벽히 차단합니다!
            }

            // 💔 3순위: 쉴드도 없고 분신도 없다면... 결국 본체가 맞습니다.
            currentLives--;

            if (currentLives >= 0 && currentLives < hearts.Count)
            {
                hearts[currentLives].BreakHeart();
            }

            Destroy(other.gameObject);

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