using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float stopXPosition = 6f;
    private bool isStopped = false;

    [Header("Attack (탄막)")]
    public GameObject enemyBulletPrefab;
    public float fireRate = 1.5f;
    public float initialDelay = 1f;
    private float nextFireTime;

    void Update()
    {
        // 1. 목표 위치까지 이동
        if (!isStopped)
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

            if (transform.position.x <= stopXPosition)
            {
                isStopped = true;
                nextFireTime = Time.time + initialDelay; // 도착 후 초기 대기
            }
        }
        else // 2. 멈춘 상태라면 사격 시작
        {
            if (Time.time >= nextFireTime)
            {
                Shoot(); // 💡 수정한 발사 함수 실행!
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    // 💡 3갈래에서 1발로 심플해진 사격 로직
    void Shoot()
    {
        // 각도 계산 없이, 적의 현재 위치에서 정면으로 딱 1개만 생성합니다.
        Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
    }

    // 충돌 처리 (플레이어의 쿠나이에 맞았을 때)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Kunai"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
