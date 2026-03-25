using UnityEngine;

public class CrowMonster : MonoBehaviour
{
    [Header("이동 설정 (친구분 로직 적용)")]
    public float moveSpeed = 5f;
    public float sineFrequency = 3f;
    public float amplitude = 3.25f;

    [Header("능력치 세팅")]
    public int hp = 3;
    public int contactDamage = 1;

    private float aliveTime = 0f;
    private float baseY;

    // 💡 0.01초 안에 두 번 때리는 '더블 히트'를 막는 자물쇠!
    private bool isDead = false;

    void Start()
    {
        baseY = transform.position.y;
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        aliveTime += Time.deltaTime;
        float nextX = transform.position.x - (moveSpeed * Time.deltaTime);
        float nextY = baseY + (Mathf.Sin(aliveTime * sineFrequency) * amplitude);
        transform.position = new Vector3(nextX, nextY, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 죽은 판정이 난 까마귀는 타격 불가
        if (isDead) return;

        // 💡 [수정] 에러를 내던 Shield 태그 검사를 지웠습니다. Player만 검사합니다!
        if (other.CompareTag("Player"))
        {
            isDead = true; // 데미지를 주는 순간 자물쇠 잠금!
            other.SendMessage("TakeDamage", contactDamage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }

        if (other.CompareTag("Kunai"))
        {
            hp--;
            Destroy(other.gameObject);

            if (hp <= 0)
            {
                isDead = true;
                Destroy(gameObject);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        hp -= damage;
        if (hp <= 0)
        {
            isDead = true;
            Destroy(gameObject);
        }
    }
}