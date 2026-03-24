using UnityEngine;
using System.Collections;

public class TitleUIController : MonoBehaviour
{
    [Header("오프닝 UI 이미지")]
    [Tooltip("캔버스 이미지가 아닌 원본 로고 텍스처를 바로 넣으세요.")]
    public Texture2D logoTexture;

    // 💡 [핵심 추가] GameManager와 동일하게 UI를 고정시킬 기준 해상도입니다.
    [Header("UI 기준 해상도 (절대 안 깨짐!)")]
    public float refWidth = 1920f;
    public float refHeight = 1080f;

    [Header("텍스트 디자인 설정")]
    public GUIStyle menuButtonStyle;
    public GUIStyle countdownStyle;

    [Header("버튼 크기 설정 (화면 비율)")]
    [Tooltip("버튼의 가로 길이 (0.35 = 화면의 35%)")]
    public float buttonWidthRatio = 0.35f;
    [Tooltip("버튼의 세로 길이 (0.1 = 화면의 10%)")]
    public float buttonHeightRatio = 0.1f;
    [Tooltip("버튼 사이의 위아래 간격 (기본 1.2)")]
    public float buttonSpacing = 1.2f;

    [Header("버튼 색상 및 테두리 설정")]
    public Color buttonNormalColor = Color.white;
    public Color buttonHoverColor = Color.yellow;
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
        // 💡 [수정] 날아가는 목표 지점도 Screen.width가 아닌 refWidth 기준으로 맞춥니다.
        float targetX = -refWidth * 1.5f;
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
        if (Event.current.type == EventType.Layout) return;

        // 💡 [핵심 마법] 화면 해상도가 달라져도 UI가 깨지지 않게 비율을 고정합니다!
        Vector3 scale = new Vector3(Screen.width / refWidth, Screen.height / refHeight, 1f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

        // 1. 로고 그리기
        if (isLogoVisible && logoTexture != null && GameManager.instance.currentMenu == GameManager.MenuState.None)
        {
            // 💡 [수정] 이제 모든 Screen.width / Screen.height를 refWidth / refHeight로 교체!
            float logoW = refWidth * 0.6f;
            float logoH = logoW * ((float)logoTexture.height / logoTexture.width);
            float baseLogoX = (refWidth - logoW) / 2f;
            float baseLogoY = refHeight * 0.15f;

            GUI.DrawTexture(new Rect(baseLogoX + logoOffsetX, baseLogoY, logoW, logoH), logoTexture);
        }

        // 2. 메인 버튼 3형제 그리기
        if (showButtons && GameManager.instance.currentMenu == GameManager.MenuState.None)
        {
            float btnW = refWidth * buttonWidthRatio;
            float btnH = refHeight * buttonHeightRatio;
            float centerX = (refWidth - btnW) / 2f;
            float startY = refHeight * 0.55f;

            if (DrawCustomButton(new Rect(centerX, startY, btnW, btnH), "START", menuButtonStyle)) OnClickStart();
            if (DrawCustomButton(new Rect(centerX, startY + btnH * buttonSpacing, btnW, btnH), "SETTINGS", menuButtonStyle)) OnClickSettings();
            if (DrawCustomButton(new Rect(centerX, startY + btnH * buttonSpacing * 2f, btnW, btnH), "QUIT", menuButtonStyle)) OnClickQuit();
        }

        // 3. 카운트다운 텍스트 그리기
        if (isCountingDown)
        {
            DrawTextWithOutline(new Rect(0, 0, refWidth, refHeight), countdownTextString, countdownStyle, Color.black, 2f);
        }
    }

    // =======================================================
    // 🌟 테두리와 호버 기능이 포함된 '수제 버튼' 제작 함수
    // =======================================================
    bool DrawCustomButton(Rect rect, string text, GUIStyle style)
    {
        bool isClicked = GUI.Button(rect, "", style);
        bool isHover = rect.Contains(Event.current.mousePosition);

        style.normal.textColor = isHover ? buttonHoverColor : buttonNormalColor;
        DrawTextWithOutline(rect, text, style, buttonOutlineColor, buttonOutlineWidth);

        return isClicked;
    }

    // 아웃라인 렌더링 핵심 함수
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