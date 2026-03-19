using UnityEngine;
using TMPro; // 💡 [수정] TextMeshPro 시스템을 사용하기 위한 필수 선언
using System.Collections;

public class TitleUIController : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    public RectTransform logoRect;
    public GameObject buttonGroup;

    // 💡 [수정] 구형 Text 대신 최신 TextMeshProUGUI 타입으로 변경
    public TextMeshProUGUI countdownText;

    [Header("애니메이션 설정")]
    public float scrollDuration = 1.0f;

    void Start()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (buttonGroup != null) buttonGroup.SetActive(true);
    }

    public void OnClickStart()
    {
        Debug.Log("🎯 [System] 시작 버튼 함수가 호출되었습니다!");
        if (buttonGroup != null) buttonGroup.SetActive(false);
        StartCoroutine(StartCountdownAndGame());
    }

    public void OnClickSettings()
    {
        if (buttonGroup != null) buttonGroup.SetActive(false);
        GameManager.instance.OpenSettingsFromTitle();
    }

    public void OnClickQuit()
    {
        if (buttonGroup != null) buttonGroup.SetActive(false);
        GameManager.instance.OpenQuitConfirmFromTitle();
    }

    public void ShowButtons()
    {
        if (buttonGroup != null) buttonGroup.SetActive(true);
    }

    IEnumerator StartCountdownAndGame()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            for (int i = 3; i > 0; i--)
            {
                countdownText.text = i.ToString();
                yield return new WaitForSecondsRealtime(1f);
            }
            countdownText.text = "START!";
            yield return new WaitForSecondsRealtime(0.5f);
            countdownText.gameObject.SetActive(false);
        }

        StartCoroutine(ScrollLogoLeft());
        GameManager.instance.StartGameSequence();
    }

    IEnumerator ScrollLogoLeft()
    {
        Vector2 startPos = logoRect.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x - 2500f, startPos.y);
        float elapsed = 0f;

        while (elapsed < scrollDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / scrollDuration;
            logoRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress * progress);
            yield return null;
        }

        logoRect.anchoredPosition = targetPos;
        logoRect.gameObject.SetActive(false);
    }
}