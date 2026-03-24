using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("낙하 설정")]
    public float fallSpeed = 7f;
    public float groundYPosition = -4.5f; // 💡 보스가 이 값을 랜덤하게 덮어씌웁니다.

    [Header("폭발 및 데미지 설정")]
    public int damage = 1;
    public float explosionRadius = 1.5f; // 💡 폭발 반경
    public float effectDuration = 0.5f;

    [Header("연출 및 프리팹 연결")]
    // 💡 [신규] 아까 만든 '반투명 빨간 조준점 프리팹'을 여기에 넣습니다.
    public GameObject warningIndicatorPrefab;
    public GameObject explosionEffectPrefab;

    private GameObject spawnedWarning; // 생성된 경고 마크를 기억해둘 변수
    private bool hasExploded = false;

    // 💡 Start는 보스가 groundYPosition 값을 세팅해준 '직후'에 실행됩니다.
    void Start()
    {
        SpawnWarningIndicator();
    }

    void Update()
    {
        if (hasExploded) return;

        // 아래로 수직 낙하
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // 목표 Y 좌표에 도달하면 폭발!
        if (transform.position.y <= groundYPosition)
        {
            Explode();
        }
    }

    // 💡 [신규] 폭탄이 생성되자마자 바닥에 경고 마크를 소환합니다.
    void SpawnWarningIndicator()
    {
        if (warningIndicatorPrefab != null)
        {
            // 위치는 폭탄의 X좌표, 터질 높이인 groundYPosition, Z는 0
            Vector3 warningPos = new Vector3(transform.position.x, groundYPosition, 0f);

            // 경고 마크 소환!
            spawnedWarning = Instantiate(warningIndicatorPrefab, warningPos, Quaternion.identity);

            // 💡 [베테랑의 디테일] 경고 마크의 크기를 실제 폭발 반경(explosionRadius)과 똑같이 맞춥니다!
            // 이미지가 반지름 0.5unit짜리 원이라면, 지름(Scale)을 Radius * 2로 해야 딱 맞습니다.
            float diameter = explosionRadius * 2f;
            spawnedWarning.transform.localScale = new Vector3(diameter, diameter, 1f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;

        // 떨어지는 도중 플레이어와 직접 부딪혀도 즉시 폭발합니다.
        if (other.CompareTag("Player"))
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;

        // 1. 💥 광역(스플래시) 데미지 판정!
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hitObjects)
        {
            if (hit.CompareTag("Player"))
            {
                hit.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }

        // 2. ✨ 폭발 이펙트 생성 및 '자동 삭제' 예약
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }

        // 3. 🛑 [신규] 터졌으니 바닥에 있던 경고 마크도 지워줍니다.
        if (spawnedWarning != null)
        {
            Destroy(spawnedWarning);
        }

        // 4. 폭탄 본체 파괴
        Destroy(gameObject);
    }

    // 유니티 씬(Scene) 화면에서 폭발 범위를 빨간 원으로 보여주는 기능 (개발용)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, groundYPosition, 0f), explosionRadius);
    }
}