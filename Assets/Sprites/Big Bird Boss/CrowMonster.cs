using UnityEngine;

public class CrowMonster : MonoBehaviour
{
    [Header("이동 설정 (친구분 로직 적용)")]
    public float moveSpeed = 5f;        // 친구분 코드의 moveSpeed 방식
    public float sineFrequency = 3f;    // 친구분 코드의 sineFrequency (꿀렁이는 속도)
    public float amplitude = 3.25f;     // 친구분 코드에 있던 3.25f 진폭 (위아래 폭)

    [Header("능력치 세팅")]
    public int hp = 3;
    public int contactDamage = 1;

    private float aliveTime = 0f;       // 친구분 코드의 시간 누적 변수
    private float baseY;

    void Start()
    {
        // 소환된 순간의 높이(극점)를 기준점으로 삼습니다.
        baseY = transform.position.y;

        // 화면 밖으로 멀리 날아가면 알아서 소멸 (메모리 최적화)
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        // 💡 1. 친구분 코드처럼 살아있는 시간을 계속 누적합니다.
        aliveTime += Time.deltaTime;

        // 💡 2. 친구분 코드의 X축 전진 방식 (현재 X - 속도)
        float nextX = transform.position.x - (moveSpeed * Time.deltaTime);

        // 💡 3. 친구분 코드의 Y축 Sine 곡선 공식 완벽 적용!
        // (Mathf.Sin(aliveTime * sineFrequency) * amplitude)
        float nextY = baseY + (Mathf.Sin(aliveTime * sineFrequency) * amplitude);

        // 위치 적용!
        transform.position = new Vector3(nextX, nextY, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어와 부딪혔을 때 처리
        if (other.CompareTag("Player"))
        {
            other.SendMessage("TakeDamage", contactDamage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }

        // 주인공의 표창(Kunai)에 맞았을 때 피격 처리
        if (other.CompareTag("Kunai"))
        {
            hp--;
            Destroy(other.gameObject);

            if (hp <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            // 사망 시 파괴
            Destroy(gameObject);
        }
    }
}