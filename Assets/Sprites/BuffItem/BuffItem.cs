using UnityEngine;

public class BuffItem : MonoBehaviour
{
    [Header("이동 설정")]
    // 💡 아이템이 생성된 후 왼쪽으로 천천히 움직이게 합니다 (아케이드 느낌)
    public float scrollSpeed = 2f;

    void Update()
    {
        // 왼쪽으로 이동
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // 화면 밖으로 나가면 자동 삭제 (메모리 관리)
        if (transform.position.x < -10f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 💡 오직 "Player" 태그를 가진 오브젝트와 충돌했을 때만 작동합니다.
        if (other.CompareTag("Player"))
        {
            // 💡 1. 플레이어에게 분신 버프를 활성화하라고 명령합니다!
            PlayerBuffController playerBuff = other.GetComponent<PlayerBuffController>();
            if (playerBuff != null)
            {
                playerBuff.ActivateCloneBuff();
            }

            // 💡 2. 아이템 자신을 파괴합니다 (먹었으므로 사라짐)
            Destroy(gameObject);
        }
    }
}