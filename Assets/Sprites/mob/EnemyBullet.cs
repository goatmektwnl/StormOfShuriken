using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 5f;
    public float lifeTime = 8f;

    private Vector3 targetDirection; // 날아갈 방향을 저장할 공간

    void Start()
    {
        // 1. 수명이 다하면 파괴
        Destroy(gameObject, lifeTime);

        // 2. 방금 태그를 달아준 "Player"를 씬에서 찾습니다.
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // 3. 방향 계산 공식: (목표 위치 - 내 위치).normalized
            targetDirection = (player.transform.position - transform.position).normalized;

            // 4. (선택) 날아가는 방향에 맞춰 쿠나이 이미지 회전시키기
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

            // 💡 만약 적의 쿠나이 이미지가 기본적으로 '왼쪽'을 보고 있다면 (angle + 180f)
            // 💡 '오른쪽'을 보고 있다면 그냥 (angle)을 입력하세요.
            transform.rotation = Quaternion.Euler(0, 0, angle + 180f);
        }
        else
        {
            // 만약 플레이어가 파괴되어서 없다면, 그냥 왼쪽으로 날아갑니다.
            targetDirection = Vector3.left;
        }
    }

    void Update()
    {
        // 유도탄처럼 계속 따라가는 게 아니라, 쏠 때 계산해 둔 방향으로 직진합니다!
        transform.position += targetDirection * speed * Time.deltaTime;
    }
}