using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TopBarAnimation : MonoBehaviour
{
    [Header("위치 설정")]
    public float startY = 200f;   // 화면 밖 대기 위치 (위쪽)
    public float targetY = -50f;  // 화면 안쪽 목표 위치 (상단 바 자리)

    [Header("애니메이션 설정")]
    public float speed = 5f;      // 슬라이드 이동 속도

    private RectTransform rectTransform;
    private float currentDestinationY;
    private bool isAnimating = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // 시작 시 화면 밖으로 즉시 이동시켜 대기 상태로 만듭니다.
        currentDestinationY = startY;
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, startY);
    }

    // GameManager에서 게임 시작(Play) 시 호출하는 함수
    public void PlaySlideIn()
    {
        gameObject.SetActive(true); // 비활성화되어 있을 경우를 대비해 켭니다.
        currentDestinationY = targetY;
        isAnimating = true;
    }

    // 게임 종료나 로비 복귀 시 연출을 위해 사용할 수 있는 함수
    public void PlaySlideOut()
    {
        currentDestinationY = startY;
        isAnimating = true;
    }

    void Update()
    {
        if (!isAnimating || rectTransform == null) return;

        float currentY = rectTransform.anchoredPosition.y;

        // 💡 Time.unscaledDeltaTime을 사용하여 Time.timeScale = 0 상태에서도 UI가 부드럽게 내려오도록 처리합니다.
        float newY = Mathf.Lerp(currentY, currentDestinationY, Time.unscaledDeltaTime * speed);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);

        // 목표 지점에 오차 범위(0.5f) 이내로 도달했을 때 위치를 고정하고 애니메이션을 종료합니다.
        if (Mathf.Abs(newY - currentDestinationY) < 0.5f)
        {
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, currentDestinationY);
            isAnimating = false;

            // 화면 밖으로 나가는 연출이 끝났다면 오브젝트를 비활성화하여 최적화합니다.
            if (currentDestinationY == startY)
            {
                gameObject.SetActive(false);
            }
        }
    }
}