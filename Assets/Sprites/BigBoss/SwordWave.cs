using UnityEngine;

public class SwordWave : MonoBehaviour
{
    public float speed = 12f;
    private Vector3 moveDirection = Vector3.left; // 기본 방향은 왼쪽

    // 💡 보스가 이 함수를 불러서 파괴신님의 방향을 주입합니다!
    public void SetDirection(Vector3 newDirection)
    {
        moveDirection = newDirection;
    }

    void Update()
    {
        // 주입받은 방향으로 맹렬하게 전진!
        transform.position += moveDirection * speed * Time.deltaTime;

        // 조준 사격이므로 상하좌우 어디로든 화면을 벗어나면 삭제되게 안전망 구축
        if (transform.position.x < -20f || transform.position.x > 20f ||
            transform.position.y < -15f || transform.position.y > 15f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 💡 파괴신님의 태그가 "Player"여야 적중합니다!
        if (other.CompareTag("Player"))
        {
            // 플레이어 피격 로직이 있다면 여기에 추가
            Destroy(gameObject);
        }
    }
}
