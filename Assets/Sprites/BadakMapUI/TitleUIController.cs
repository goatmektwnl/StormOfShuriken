using UnityEngine;
using System.Collections;

public class TitleUIController : MonoBehaviour
{
    [Header("오프닝 UI 이미지")]
    [Tooltip("캔버스 이미지가 아닌 원본 로고 텍스처를 바로 넣으세요.")]
    public Texture2D logoTexture;

    [Header("텍스트 디자인 설정")]
    public GUIStyle menuButtonStyle;
    public GUIStyle countdownStyle;

    [Header("애니메이션 설정")]
    public float scrollDuration = 1.0f;

    private bool showButtons = true;
    private bool isCountingDown = false;
    private string countdownTextString = "";

    // 로고가 왼쪽으로 날아갈 때 쓸 X축 좌표 변수
    private float logoOffsetX = 0f;
    private bool isLogoVisible = true;

    void Start()
    {
        // 초기화
        showButtons = true;
        isCountingDown = false;
        logoOffsetX = 0f;
    }

    public void OnClickStart()
    {
        Debug.Log("🎯 [System] 시작 버튼 함수가 호출되었습니다!");
        showButtons = false;
        StartCoroutine(StartCountdownAndGame());
    }

    public void OnClickSettings()
    {
        showButtons = false;
        GameManager.instance.OpenSettingsFromTitle();
    }

    public void OnClickQuit()
    {
        showButtons = false;
        GameManager.instance.OpenQuitConfirmFromTitle();
    }

    // GameManager가 설정이나 종료 취소 후 다시 메뉴를 띄울 때 부르는 함수
    public void ShowButtons()
    {
        showButtons = true;
    }

    IEnumerator StartCountdownAndGame()
    {
        isCountingDown = true;

        for (int i = 3; i > 0; i--)
        {
            countdownTextString = i.ToString();
            // 💡 GameManager에서 Time.timeScale = 0 상태이므로 unscaledDeltaTime이나 WaitForSecondsRealtime을 써야 합니다!
            yield return new WaitForSecondsRealtime(1f);
        }

        countdownTextString = "START!";
        yield return new WaitForSecondsRealtime(0.5f);

        isCountingDown = false;

        // 1. 로고를 왼쪽으로 날려버립니다.
        StartCoroutine(ScrollLogoLeft());

        // 2. 게임매니저에게 본게임 시작을 알립니다.
        GameManager.instance.StartGameSequence();
    }

    IEnumerator ScrollLogoLeft()
    {
        float startX = 0f;
        // 화면 왼쪽 밖으로 아득히 멀어지는 목표 지점
        float targetX = -Screen.width * 1.5f;
        float elapsed = 0f;

        while (elapsed < scrollDuration)
        {
            elapsed += Time.unscaledDeltaTime; // 시간 정지 상태에서도 작동하도록 보장
            float progress = elapsed / scrollDuration;

            // progress * progress 를 쓰면 처음엔 서서히, 나중엔 확 빨라지는 쫀득한 연출이 됩니다.
            logoOffsetX = Mathf.Lerp(startX, targetX, progress * progress);
            yield return null;
        }

        logoOffsetX = targetX;
        isLogoVisible = false; // 화면 밖으로 나가면 렌더링 종료
    }

    // =======================================================
    // 🎨 UI 렌더링 시작 (버튼과 로고, 카운트다운을 그립니다)
    // =======================================================
    void OnGUI()
    {
        // 1. 로고 그리기
        // 💡 [수정] GameManager에 다른 팝업(설정, 종료 확인 등)이 안 떠 있을 때만 로고를 그립니다!
        if (isLogoVisible && logoTexture != null && GameManager.instance.currentMenu == GameManager.MenuState.None)
        {
            // 화면 비율에 맞춰 로고를 예쁘게 위쪽에 주차시킵니다.
            float logoW = Screen.width * 0.6f;
            float logoH = logoW * ((float)logoTexture.height / logoTexture.width);
            float baseLogoX = (Screen.width - logoW) / 2f;
            float baseLogoY = Screen.height * 0.15f;

            GUI.DrawTexture(new Rect(baseLogoX + logoOffsetX, baseLogoY, logoW, logoH), logoTexture);
        }

        // 2. 메인 버튼 3형제 그리기 (GameManager의 다른 팝업이 안 떠있을 때만!)
        if (showButtons && GameManager.instance.currentMenu == GameManager.MenuState.None)
        {
            float btnW = Screen.width * 0.35f;
            float btnH = Screen.height * 0.1f;
            float centerX = (Screen.width - btnW) / 2f;
            float startY = Screen.height * 0.55f; // 로고 아래쪽에 배치

            // OnGUI의 진가! 버튼을 그리는 동시에 눌렸는지 판정합니다.
            if (GUI.Button(new Rect(centerX, startY, btnW, btnH), "START", menuButtonStyle)) OnClickStart();
            if (GUI.Button(new Rect(centerX, startY + btnH * 1.2f, btnW, btnH), "SETTINGS", menuButtonStyle)) OnClickSettings();
            if (GUI.Button(new Rect(centerX, startY + btnH * 2.4f, btnW, btnH), "QUIT", menuButtonStyle)) OnClickQuit();
        }

        // 3. 카운트다운 텍스트 그리기
        if (isCountingDown)
        {
            DrawTextWithOutline(new Rect(0, 0, Screen.width, Screen.height), countdownTextString, countdownStyle, Color.black, 2f);
        }
    }

    // 아웃라인 렌더링 핵심 함수 (두께 2f)
    void DrawTextWithOutline(Rect rect, string text, GUIStyle style, Color outlineColor, float outlineWidth)
    {
        Color originalColor = style.normal.textColor;
        style.normal.textColor = outlineColor;

        GUI.Label(new Rect(rect.x - outlineWidth, rect.y, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x + outlineWidth, rect.y, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x, rect.y - outlineWidth, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x, rect.y + outlineWidth, rect.width, rect.height), text, style);

        GUI.Label(new Rect(rect.x - outlineWidth, rect.y - outlineWidth, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x + outlineWidth, rect.y - outlineWidth, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x - outlineWidth, rect.y + outlineWidth, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x + outlineWidth, rect.y + outlineWidth, rect.width, rect.height), text, style);

        style.normal.textColor = originalColor;
        GUI.Label(rect, text, style);
    }
}