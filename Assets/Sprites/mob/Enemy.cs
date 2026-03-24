using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("이동 및 공격 설정")]
    public float moveSpeed = 3f;
    public float stopXPosition = 6f;
    public GameObject enemyBulletPrefab;
    public float fireRate = 0.5f;

    [Header("사망 및 연출")]
    public GameObject deathMotionPrefab;
    // 💡 아이템 관련 변수 삭제

    [Header("피격 판정 제어")]
    private bool isInvincible = true;
    private bool isAdvancing = true;
    private bool hasAttacked = false;
    private bool isMovingLeftAgain = false;
    private bool isDead = false;

    private SpriteRenderer sr;

    void Start() { sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        if (isDead) return;
        if (isAdvancing && !hasAttacked && !isMovingLeftAgain)
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
            if (transform.position.x <= stopXPosition)
            {
                isAdvancing = false;
                isInvincible = false;
                StartCoroutine(FireThreeBullets());
            }
        }
        else if (isMovingLeftAgain) transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x < -15f) Destroy(gameObject);
    }

    IEnumerator FireThreeBullets()
    {
        hasAttacked = true;
        for (int i = 0; i < 3; i++)
        {
            if (enemyBulletPrefab != null) Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(fireRate);
        }
        isMovingLeftAgain = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (other.CompareTag("Kunai"))
        {
            if (isInvincible) { Destroy(other.gameObject); return; }
            TakeHit();
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(int damage) { TakeHit(); }

    public void TakeHit()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(100);
            GameManager.instance.AddKill();
        }

        if (deathMotionPrefab != null) Instantiate(deathMotionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    // 💡 DropItem() 함수 삭제됨
}