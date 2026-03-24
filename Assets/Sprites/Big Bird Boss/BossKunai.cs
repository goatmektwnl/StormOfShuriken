using UnityEngine;

public class BossKunai : MonoBehaviour
{
    [Header("이동 및 사정거리 설정")]
    public float speed = 5f;

    // 💡 [핵심 추가] 기획자님이 마음대로 깎으실 수 있는 '사정거리' 변수입니다!
    public float maxDistance = 15f;

    private Vector3 moveDirection = Vector3.left;
    private Vector2 startPos; // 쿠나이가 태어난 위치를 기억할 변수

    void Start()
    {
        // 태어난 순간의 좌표를 기억해 둡니다.
        startPos = transform.position;
    }

    // 보스가 이 함수를 불러서 방향(dir)을 꽂아줍니다.
    public void SetDirection(Vector3 dir)
    {
        moveDirection = dir.normalized;
    }

    void Update()
    {
        // 💡 보스가 정해준 방향(moveDirection)으로 날아갑니다.
        transform.position += moveDirection * speed * Time.deltaTime;

        // 💡 [새로운 로직] 시작 위치부터 현재 위치까지의 거리를 자로 잽니다.
        // 그 거리가 기획자님이 설정한 사정거리(maxDistance)보다 멀어지면 스스로 파괴됩니다!
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);

            // (💡 기획 팁: 공중에서 사라질 때 펑! 하는 연기 이펙트를 넣고 싶으시면 
            // 나중에 여기에 Instantiate(연기프리팹) 한 줄만 추가하시면 됩니다.)
        }

        // (기존 2중 안전장치) 화면 밖으로 아주 멀리 나가면 강제 파괴
        if (transform.position.x < -20f || transform.position.y > 15f || transform.position.y < -15f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 타격 로직 (최근에 만든 PlayerHealth.cs의 TakeDamage와 완벽 호환!)
        if (other.CompareTag("Player"))
        {
            other.SendMessage("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}