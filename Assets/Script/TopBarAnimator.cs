using UnityEngine;
using System.Collections;

public class TopBarAnimator : MonoBehaviour
{
    private RectTransform rectTransform;
    public float slideDuration = 0.8f;
    public float startYOffset = 300f;
    // 인스펙터에서 끝부분이 1.2까지 올라갔다 1.0으로 오는 커브를 그려주세요!
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    void Awake() => rectTransform = GetComponent<RectTransform>();

    void Start()
    {
        // 시작 시 화면 밖으로 숨김
        rectTransform.anchoredPosition += new Vector2(0, startYOffset);
    }

    public void PlaySlideIn() => StartCoroutine(SlideDown());

    IEnumerator SlideDown()
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 targetPos = startPos - new Vector2(0, startYOffset);
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float curveValue = slideCurve.Evaluate(elapsed / slideDuration);
            // LerpUnclamped를 써야 커브의 바운스(1.0 초과값)가 적용됩니다.
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, curveValue);
            yield return null;
        }
    }
}