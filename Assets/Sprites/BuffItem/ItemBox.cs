using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3f;

    [Header("아이템 목록 설정")]
    // 💡 [수정] 쉴드, 공격력, 속도 등 모든 아이템 프리팹을 이 하나의 배열에 다 집어넣습니다!
    public GameObject[] itemPrefabs;

    [Header("드롭 확률 설정 (0~100)")]
    // 💡 꽝 확률만 남겨두어 간결하게 관리합니다.
    [Range(0, 100)] public float emptyChance = 10f;

    [Header("연출 설정")]
    public GameObject destructionEffectPrefab;

    private bool isVisibleOnScreen = false;

    void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        // 화면 밖으로 완전히 나가면 삭제
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    void OnBecameVisible() { isVisibleOnScreen = true; }
    void OnBecameInvisible() { isVisibleOnScreen = false; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isVisibleOnScreen) return;

        // 일반 쿠나이와 직접 부딪혔을 때
        if (other.CompareTag("Kunai"))
        {
            BreakBox();
            if (other.gameObject != null) Destroy(other.gameObject);
        }
    }

    // 💡 [신규 추가] 폭발 쿠나이의 스플래시 데미지(SendMessage)를 받아줄 함수입니다!
    public void TakeDamage(int damage)
    {
        // 폭발 데미지를 받으면 상자를 부숩니다.
        BreakBox();
    }

    void BreakBox()
    {
        // 1. 파괴 이펙트 생성
        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. 아이템 생성 로직 (랜덤)
        SpawnRandomItem();

        // 3. 상자 본체 삭제
        Destroy(gameObject);
    }

    void SpawnRandomItem()
    {
        // 1. 꽝 판정 (꽝이면 아무것도 안 나옴)
        if (Random.Range(0f, 100f) < emptyChance)
        {
            Debug.Log("📦 상자가 비어있었습니다! (꽝)");
            return;
        }

        // 2. 단일 배열(itemPrefabs)에서 무작위 아이템 추출
        // 가방 안에 아이템이 하나라도 있는지 확인
        if (itemPrefabs != null && itemPrefabs.Length > 0)
        {
            // 배열 크기 안에서 무작위 인덱스 하나 추출
            int randomIndex = Random.Range(0, itemPrefabs.Length);

            // 추출된 인덱스 위치의 아이템 프리팹 생성
            if (itemPrefabs[randomIndex] != null)
            {
                Instantiate(itemPrefabs[randomIndex], transform.position, Quaternion.identity);
            }
        }
    }
}