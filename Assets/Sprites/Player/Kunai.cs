using UnityEngine;

public class Kunai : MonoBehaviour
{
    public float speed = 15f; // 쿠나이 날아가는 속도
    public float lifeTime = 3f; // 3초 뒤 자동 소멸 (메모리 관리)

    void Start()
    {
        // 생성된 후 lifeTime이 지나면 파괴됩니다.
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 오른쪽 방향으로 계속 이동합니다.
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
}
