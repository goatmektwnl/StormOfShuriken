using UnityEngine;

public class BossKunaiSplit : MonoBehaviour
{
    public float speed = 5f;            // 처음 날아가는 속도
    public float splitSpeed = 7f;       // 분열 후 날아가는 속도
    public float timeToSplit = 1.2f;    // 발사 후 몇 초 뒤에 분열할 것인가? (무작위 지점 느낌용)
    public GameObject subKunaiPrefab;   // 분열되어 나갈 쿠나이 프리팹 (자기 자신을 넣어도 됩니다)

    private Vector3 moveDirection;
    private bool hasSplit = false;

    public void SetDirection(Vector3 dir)
    {
        moveDirection = dir.normalized;
        // 날아가는 방향을 바라보게 회전
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 180f);
    }

    void Update()
    {
        if (!hasSplit)
        {
            transform.position += moveDirection * speed * Time.deltaTime;

            // 타이머 체크
            timeToSplit -= Time.deltaTime;
            if (timeToSplit <= 0)
            {
                Split();
            }
        }
    }

    void Split()
    {
        hasSplit = true;

        // 8방향(45도 간격)으로 분열
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            float dirX = Mathf.Cos(angle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector3 splitDir = new Vector3(dirX, dirY, 0).normalized;

            if (subKunaiPrefab != null)
            {
                GameObject kunai = Instantiate(subKunaiPrefab, transform.position, Quaternion.identity);
                // 기존의 BossKunai 스크립트를 사용한다고 가정합니다.
                BossKunai script = kunai.GetComponent<BossKunai>();
                if (script != null)
                {
                    script.SetDirection(splitDir);
                    // 방향에 맞춰 회전
                    float rotAngle = Mathf.Atan2(dirY, dirX) * Mathf.Rad2Deg;
                    kunai.transform.rotation = Quaternion.Euler(0, 0, rotAngle + 180f);
                }
            }
        }

        // 분열 후 본체는 삭제
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.SendMessage("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}