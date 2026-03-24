using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 능력치")]
    public float moveSpeed = 7f;

    [Header("이동 제한 (화면 밖 방지)")]
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    [Header("공격 설정 (완전 자동 발사!)")]
    public GameObject kunaiPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f; // 💡 0.2초마다 자동으로 나갑니다.

    private float currentFireTimer;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        currentFireTimer = fireRate;
    }

    void FixedUpdate()
    {
        // 💡 1. 방향키로 이동 (키보드 조작)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 direction = new Vector2(horizontal, vertical).normalized;
        Vector2 nextPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;

        nextPosition.x = Mathf.Clamp(nextPosition.x, minX, maxX);
        nextPosition.y = Mathf.Clamp(nextPosition.y, minY, maxY);

        rb.MovePosition(nextPosition);
    }

    void Update()
    {
        // 💡 2. 전장 진입 시 자동으로 쿠나이 발사! (스페이스 바는 건드리지 않습니다)
        AutoFireRoutine();
    }

    void AutoFireRoutine()
    {
        // GameManager가 게임 중일 때만 발사하도록 안전장치를 걸어두는 것이 좋습니다.
        currentFireTimer -= Time.deltaTime;

        if (currentFireTimer <= 0f)
        {
            if (kunaiPrefab != null && firePoint != null)
            {
                Instantiate(kunaiPrefab, firePoint.position, Quaternion.identity);
            }
            currentFireTimer = fireRate;
        }
    }
}