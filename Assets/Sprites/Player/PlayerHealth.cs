using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;



public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;
    private bool isGameOver = false;

    [Header("Invincibility (피격 무적)")]
    public float invincibilityDuration = 1.5f;
    public float blinkInterval = 0.15f;
    private bool isInvincible = false;


    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentLives = maxLives;
        spriteRenderer = GetComponent<SpriteRenderer>();
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
            // 💡 1. 내 몸(PlayerBuff)에 쉴드가 켜져 있는지 확인합니다!
            PlayerBuff myBuff = GetComponent<PlayerBuff>();

            if (myBuff != null && myBuff.hasShield == true)
            {
                // 💡 2. 쉴드가 있다면 쉴드를 깨부수고 방어에 성공합니다!
                myBuff.BreakShield();

                // 나를 때린 총알이나 검기는 소멸시킵니다.
                if (other.CompareTag("EnemyBullet") || other.CompareTag("BossAttack"))
                {
                    Destroy(other.gameObject);
                }

                return; // 💡 [핵심] 여기서 함수를 끝내버리므로 아래쪽의 '사망(데미지)' 코드가 실행되지 않습니다!!
            }

            currentLives--;
            Destroy(other.gameObject);

            if (currentLives <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(BlinkAndInvincibility());
            }
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
        // 플레이어가 죽는 함수 내부 어딘가에 아래 한 줄을 넣으십시오!
        if (GameManager.instance != null)
        {
            GameManager.instance.GameOver(); // 💡 매니저에게 "나 죽었어! 시간 멈춰!" 라고 보고!
        }
    }

    // 💡 에디터 설정 없이 화면에 UI를 직접 그리는 함수
    void OnGUI()
    {
        // 1. 살아있을 때는 화면 좌측 상단에 남은 목숨(하트)을 그립니다!
        if (!isGameOver)
        {
            GUIStyle heartStyle = new GUIStyle();
            heartStyle.fontSize = 60;                // 하트 크기
            heartStyle.normal.textColor = Color.red; // 핏빛 빨간색!

            // 남은 목숨 개수만큼 하트 문자(♥)를 이어 붙입니다.
            string heartText = "";
            for (int i = 0; i < currentLives; i++)
            {
                heartText += "♥ ";
            }

            // 화면 왼쪽 위 (X: 20, Y: 20) 위치에 하트를 고정 출력!
            GUI.Label(new Rect(20, 20, 300, 100), heartText, heartStyle);
        }
        
    }
}
