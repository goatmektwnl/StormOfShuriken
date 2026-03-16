using UnityEngine;

public class ShieldItem : MonoBehaviour
{
    [Header("아이템 설정")]
    public float moveSpeed = 3f; // 💡 왼쪽으로 날아가는 속도

    void Update()
    {
        // 무조건 왼쪽으로 날아갑니다!
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 파괴신님과 닿았을 때!
        if (other.CompareTag("Player"))
        {
            // 💡 [핵심] 다른 엉뚱한 곳이 아니라 'PlayerBuff' 스크립트를 정확히 찾아냅니다!
            PlayerBuff buffScript = other.GetComponent<PlayerBuff>();

            if (buffScript != null)
            {
                buffScript.ActivateShield(); // 버프 스크립트에 쉴드 주입!
                Destroy(gameObject);         // 쉴드 아이템 파괴
            }
        }
    }
}
