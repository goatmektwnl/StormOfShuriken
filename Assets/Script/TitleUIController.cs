using UnityEngine;
using System.Collections;

public class TitleUIController : MonoBehaviour
{
    public RectTransform logoRect;
    public CanvasGroup startTextCanvasGroup;
    public float scrollDuration = 1.0f;
    public float blinkSpeed = 2.0f;

    private Coroutine blinkCoroutine;

    void Start() => blinkCoroutine = StartCoroutine(BlinkText());

    IEnumerator BlinkText()
    {
        while (true)
        {
            // Sin 함수를 이용해 0~1 사이를 부드럽게 왕복 (깜빡임)
            startTextCanvasGroup.alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
            yield return null;
        }
    }

    public void PlayStartEffect()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        startTextCanvasGroup.gameObject.SetActive(false);
        StartCoroutine(ScrollLogoLeft());
    }

    IEnumerator ScrollLogoLeft()
    {
        Vector2 startPos = logoRect.anchoredPosition;
        // 무조건 현재 위치에서 왼쪽으로 2500픽셀만큼 날려버림!
        Vector2 targetPos = new Vector2(startPos.x - 2500f, startPos.y);
        float elapsed = 0f;

        while (elapsed < scrollDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / scrollDuration;

            // 애니메이션 커브가 없다면 그냥 progress만 써도 됩니다.
            logoRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress * progress); // 가속도 추가
            yield return null;
        }

        logoRect.anchoredPosition = targetPos;
        logoRect.gameObject.SetActive(false); // 완전히 나가면 꺼버리기
    }
}