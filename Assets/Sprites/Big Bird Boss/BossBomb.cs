using UnityEngine;

public class BossBomb : MonoBehaviour
{
    [Header("폭탄 설정")]
    public float fallSpeed = 8f; // 떨어지는 속도
    public int damage = 1;

    [Tooltip("터질 때 나올 이펙트 프리팹 (옵션)")]
    public GameObject explosionEffect;

    void Start()
    {
        // 땅에 안 닿고 화면 밖으로 한참 떨어지면 스스로 삭제 (메모리 정리)
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        // 아래로 일정하게 추락
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어에게 맞았을 때
        if (other.CompareTag("Player"))
        {
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Explode();
        }
        // 바닥(Ground 레이어나 태그)에 닿았을 때 터지게 하려면 아래 주석을 푸세요!
        /*
        else if (other.CompareTag("Ground"))
        {
            Explode();
        }
        */
    }

    void Explode()
    {
        // 폭발 이펙트 생성
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // 폭탄 자신은 파괴
        Destroy(gameObject);
    }
}