using UnityEngine;

public class TopBarAnimation : MonoBehaviour
{
    private RectTransform rectTransform;
    public float startY = 200f;   // 화면 밖 위치
    public float targetY = -100f; // 화면 안 위치 (상단 바 자리)
    public float speed = 5f;

    private float currentDestinationY; // 현재 목표로 하는 Y값
    private bool isAnimating = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        currentDestinationY = startY; // 처음엔 화면 밖에 있도록 설정
    }

    // [GameManager에서 부르는 함수] 상단 바 들어오기
    public void PlaySlideIn()
    {
        currentDestinationY = targetY;
        isAnimating = true;
    }

    // ★ [새로 추가] 상단 바 나가기
    public void PlaySlideOut()
    {
        currentDestinationY = startY; // 다시 화면 밖(위쪽)으로 목표 설정
        isAnimating = true;
    }

    void Update()
    {
        if (isAnimating && rectTransform != null)
        {
            float currentY = rectTransform.anchoredPosition.y;
            float newY = Mathf.Lerp(currentY, currentDestinationY, Time.deltaTime * speed);
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);

            // 목표 지점에 거의 도착하면 애니메이션 끄기
            if (Mathf.Abs(newY - currentDestinationY) < 0.5f)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, currentDestinationY);
                isAnimating = false;

                // 만약 나가는 중이었다면 아예 오브젝트를 꺼버려도 됩니다 (선택사항)
                if (currentDestinationY == startY)
                {
                    // gameObject.SetActive(false); // 필요하면 주석 해제하세요!
                }
            }
        }
    }
}