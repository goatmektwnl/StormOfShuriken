using UnityEngine;

public class SwordWave2 : MonoBehaviour
{
    [Header("검기 전투 설정")]
    public float speed = 12f;       // 검기가 날아가는 속도 (꽤 빨라야 위협적입니다!)
    public int damage = 1;          // 플레이어에게 입히는 데미지
    public float lifeTime = 5f;     // 5초 뒤 자동 소멸 (화면 밖으로 날아간 쓰레기 청소)

    private Vector3 moveDirection;
    private bool isFired = false;

    void Start()
    {
        // 태어나자마자 5초 뒤에 죽는 타이머를 켭니다. (메모리 최적화)
        Destroy(gameObject, lifeTime);
    }

    // 💡 [핵심] 보스(BossEnemy.cs)가 조준을 마치고 이 함수로 방향을 꽂아줍니다!
    public void SetDirection(Vector3 dir)
    {
        moveDirection = dir.normalized;
        isFired = true;
    }

    void Update()
    {
        // 방향을 전달받았다면, 그 방향으로 쉬지 않고 날아갑니다.
        if (isFired)
        {
            transform.position += moveDirection * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 플레이어 피격 처리
        if (other.CompareTag("Player"))
        {
            // 플레이어의 체력을 깎습니다.
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            // 💡 [기획 선택] 거대 검기라서 유저를 관통하며 날아가게 하고 싶다면 아래 줄을 지우세요!
            // 하지만 보통은 맞으면 데미지를 주고 사라지는 게 깔끔하므로 켜두었습니다.
            Destroy(gameObject);
        }

        // 2. 맵 밖의 벽(Wall) 등에 부딪혔을 때 파괴하고 싶다면 여기에 추가
        /*
        if (other.CompareTag("Wall")) 
        {
            Destroy(gameObject);
        }
        */
    }
}