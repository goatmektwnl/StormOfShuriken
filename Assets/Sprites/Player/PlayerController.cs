using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;

    [Header("Weapon")]
    public GameObject kunaiPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f; // 0.5초마다 자동 발사
    private float nextFireTime;

    [Header("Movement Bounds")]
    public float minX = -7f;
    public float maxX = -2f;
    public float minY = -4f;
    public float maxY = 4f;

    void Update()
    {
        // 1. 이동 처리 (PC: WASD / 모바일 적용 시 조이스틱 연동)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(moveX, moveY, 0f).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // 2. 화면 밖으로 나가지 못하게 제한
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);

        // 3. ✨ 자동 공격 로직 (수정됨!) ✨
        // 이전처럼 Input.GetMouseButton(0)을 검사하지 않습니다.
        // 오직 시간이 nextFireTime을 지났는지만 확인하여 자동으로 발사합니다.
        if (Time.time >= nextFireTime)
        {
            Instantiate(kunaiPrefab, firePoint.position, firePoint.rotation);
            nextFireTime = Time.time + fireRate; // 다음 발사 시간 갱신
        }
    }
}