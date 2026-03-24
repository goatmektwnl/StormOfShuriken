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

    // 💡 [신규 추가] 인스펙터에서 버튼 크기와 간격을 마음대로 조절하세요!
    [Header("버튼 크기 설정 (화면 비율)")]
    [Tooltip("버튼의 가로 길이 (0.35 = 화면의 35%)")]
    public float buttonWidthRatio = 0.35f;
    [Tooltip("버튼의 세로 길이 (0.1 = 화면의 10%)")]
    public float buttonHeightRatio = 0.1f;
    [Tooltip("버튼 사이의 위아래 간격 (기본 1.2)")]
    public float buttonSpacing = 1.2f;

    // 💡 [신규 추가] 그라데이션 대신 마우스 호버 색상 변화로 생동감을 줍니다!
    [Header("버튼 색상 및 테두리 설정")]
    public Color buttonNormalColor = Color.white;
    public Color buttonHoverColor = Color.yellow; // 마우스를 올렸을 때 변할 색상
    public Color buttonOutlineColor = Color.black;
    public float buttonOutlineWidth = 2f;

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
        float targetX = -Screen.width * 1.5f;
        float elapsed = 0f;

        while (elapsed < scrollDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / scrollDuration;

            logoOffsetX = Mathf.Lerp(startX, targetX, progress * progress);
            yield return null;
        }

        logoOffsetX = targetX;
        isLogoVisible = false;
    }

    // =======================================================
    // 🎨 UI 렌더링 
    // =======================================================
    void OnGUI()
    {
        // 1. 로고 그리기
        if (isLogoVisible && logoTexture != null && GameManager.instance.currentMenu == GameManager.MenuState.None)
        {
            float logoW = Screen.width * 0.6f;
            float logoH = logoW * ((float)logoTexture.height / logoTexture.width);
            float baseLogoX = (Screen.width - logoW) / 2f;
            float baseLogoY = Screen.height * 0.15f;

            GUI.DrawTexture(new Rect(baseLogoX + logoOffsetX, baseLogoY, logoW, logoH), logoTexture);
        }

        // 2. 메인 버튼 3형제 그리기
        if (showButtons && GameManager.instance.currentMenu == GameManager.MenuState.None)
        {
            // 💡 [수정] 인스펙터에서 설정한 변수를 바탕으로 크기를 계산합니다.
            float btnW = Screen.width * buttonWidthRatio;
            float btnH = Screen.height * buttonHeightRatio;
            float centerX = (Screen.width - btnW) / 2f;
            float startY = Screen.height * 0.55f;

            // 💡 [수정] 기본 GUI.Button 대신 아래에서 직접 만든 DrawCustomButton을 사용합니다!
            if (DrawCustomButton(new Rect(centerX, startY, btnW, btnH), "START", menuButtonStyle)) OnClickStart();
            if (DrawCustomButton(new Rect(centerX, startY + btnH * buttonSpacing, btnW, btnH), "SETTINGS", menuButtonStyle)) OnClickSettings();
            if (DrawCustomButton(new Rect(centerX, startY + btnH * buttonSpacing * 2f, btnW, btnH), "QUIT", menuButtonStyle)) OnClickQuit();
        }

        // 3. 카운트다운 텍스트 그리기
        if (isCountingDown)
        {
            DrawTextWithOutline(new Rect(0, 0, Screen.width, Screen.height), countdownTextString, countdownStyle, Color.black, 2f);
        }
    }

    // =======================================================
    // 🌟 테두리와 호버 기능이 포함된 '수제 버튼' 제작 함수
    // =======================================================
    bool DrawCustomButton(Rect rect, string text, GUIStyle style)
    {
        // 1. 실제 클릭 판정을 받는 투명한 버튼을 생성합니다. (글씨는 따로 그림)
        bool isClicked = GUI.Button(rect, "", style);

        // 2. 마우스가 현재 버튼 위에 올라와 있는지(Hover) 체크합니다.
        bool isHover = rect.Contains(Event.current.mousePosition);

        // 3. 마우스가 올라와 있으면 HoverColor, 아니면 NormalColor를 적용합니다.
        style.normal.textColor = isHover ? buttonHoverColor : buttonNormalColor;

        // 4. 결정된 색상으로 테두리가 있는 글씨를 버튼 중앙에 덧그려줍니다.
        DrawTextWithOutline(rect, text, style, buttonOutlineColor, buttonOutlineWidth);

        return isClicked;
    }

    // 아웃라인 렌더링 핵심 함수 (기존과 동일)
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