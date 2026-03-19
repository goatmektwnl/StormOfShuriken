using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3f;

    [Header("아이템 설정")]
    public GameObject itemPrefab;
    public GameObject destructionEffectPrefab;

    // 💡 화면 진입 여부를 체크할 변수
    private bool isVisibleOnScreen = false;

    void Update()
    {
        // 매 프레임 왼쪽으로 이동
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        // 화면 왼쪽 밖으로 완전히 나가버리면 삭제
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    // 💡 카메라 화면에 들어오면 true
    void OnBecameVisible()
    {
        isVisibleOnScreen = true;
    }

    // 💡 카메라 화면에서 나가면 false
    void OnBecameInvisible()
    {
        isVisibleOnScreen = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 💡 [핵심] 화면 안에 들어오지 않았다면 피격 판정을 무시합니다!
        if (!isVisibleOnScreen) return;

        if (other.CompareTag("Kunai"))
        {
            Destroy(other.gameObject);
            BreakBox();
        }
    }

    void BreakBox()
    {
        if (itemPrefab != null)
        {
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }

        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}