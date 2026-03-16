using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int hp = 3;
    private bool isInvincible = false;
    public static GameManager instance;
    private SpriteRenderer sr;
    public GameManager gameManager; // GameManager를 담을 칸 만들기
    public int lives = 3;

    void Awake()
    {
      
        sr = GetComponent<SpriteRenderer>();
    }


    // [중요!] 외부(적 총알 등)나 내부에서 호출하는 피격 함수
    public void TakeDamage()
    {
        // 무적 상태라면 데미지를 입지 않음
        if (isInvincible) return;

        hp--;

        // UI 매니저에게 하트 하나 깨뜨리라고 신호 보냄
        if (UIManager.Instance != null)
        {
            UIManager.Instance.TakeDamage();
        }

        if (hp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }

        lives--;

        if (lives <= 0)
        {
            // ★ "GameManager의 instance 안에 있는 OnPlayerDie를 실행해!"
            GameManager.instance.OnPlayerDie();
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // 적 총알에 닿았을 때 실행
        if (col.CompareTag("EnemyBullet"))
        {
            TakeDamage();
        }
    }

    void Die()
    {
        Debug.Log("닌자 사망...");
        if (GameManager.instance != null)
        {
            GameManager.instance.OnPlayerDie();
        }
        else
        {
            Debug.LogError("🚨 비상! Hierarchy 창에 GameManager 오브젝트가 없습니다!");
        }

        // 애니메이션 실행 (PlayerController가 있다면 그걸 부름)
        GetComponent<PlayerController>().Die();


        Animator anim = GetComponent<Animator>();

        // 2. 애니메이터를 비활성화(OFF) 합니다.
        if (anim != null)
        {
            anim.enabled = false;
        }



    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 1.5f;
        while (timer > 0)
        {
            sr.enabled = !sr.enabled; // 깜빡임 효과
            yield return new WaitForSeconds(0.1f);
            timer -= 0.1f;
        }
        sr.enabled = true;
        isInvincible = false;
    }
}