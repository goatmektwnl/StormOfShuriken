using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("스테이지 기본 설정")]
    public bool isFirstStage = true;
    public string currentStageName = "STAGE 1";
    public string nextStageName = "Stage2";

    [Header("시네마틱 컷신 설정")]
    [Tooltip("Hierarchy에 있는 플레이어(Player) 오브젝트를 여기에 끌어다 넣으세요.")]
    public Transform playerTransform;
    public float playerAutoMoveSpeed = 7f;

    [Header("오프닝 연출")]
    public bool isLogoScreen = true;

    [Header("Canvas UI 애니메이션 컨트롤러")]
    public TitleUIController titleUI;
    public TopBarAnimation topBarUI;

    [Header("텍스트 디자인 설정")]
    public GUIStyle menuTitleStyle;
    public GUIStyle menuButtonStyle;
    public GUIStyle menuLabelStyle;
    public GUIStyle hudScoreStyle;
    public GUIStyle hudKillStyle;
    public GUIStyle centerAlertStyle;
    public GUIStyle restartTextStyle;

    [Header("BGM 설정")]
    public AudioSource bgmSource;
    public AudioClip mainBgm;
    public AudioClip bossBgm;
    public AudioClip gameOverBgm;

    [Header("일시정지 및 메뉴 설정")]
    private bool isPaused = false;
    public enum MenuState { None, Main, ConfirmLobby, ConfirmQuit, Settings }
    public MenuState currentMenu = MenuState.None;

    [Header("해상도 설정 데이터")]
    private Resolution[] resolutions;
    private int selectedResolutionIndex = 0;

    [Header("점수 및 킬 카운터")]
    public int score = 0;
    public int totalKills = 0;
    public int killsToSpawnBoss = 100;

    [Header("보스 스포너 연결")]
    public BossSpawner bossSpawner;
    private bool bossSpawned = false;
    private bool isStageClear = false;
    private bool isGameOver = false;

    private bool showBossHp = false;
    private int currentBossHp = 0;
    private int maxBossHp = 0;
    private Texture2D bgTexture;
    private Texture2D fgTexture;
    private Texture2D blackTexture;

    [Header("결과 화면 연출")]
    public Texture2D stageClearImage;
    private float stageClearTextX = 0f;
    private float fadeAlpha = 0f;
    private bool showStageStartText = false;

    private Animator playerAnimator;

    void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(gameObject); return; }

        resolutions = Screen.resolutions;

        bgTexture = new Texture2D(1, 1);
        bgTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.95f));
        bgTexture.Apply();

        fgTexture = new Texture2D(1, 1);
        fgTexture.SetPixel(0, 0, new Color(0.8f, 0f, 0f, 1f));
        fgTexture.Apply();

        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
    }

    void Start()
    {
        if (playerTransform != null) playerAnimator = playerTransform.GetComponent<Animator>();

        if (isFirstStage)
        {
            isLogoScreen = true;
            Time.timeScale = 0f;
            fadeAlpha = 0f;

            PlayerPrefs.SetInt("SavedScore", 0);
            score = 0;

            // 💡 [추가] 로비 화면에서는 플레이어와 목숨(TopBar) UI를 완전히 숨깁니다!
            if (playerTransform != null) playerTransform.gameObject.SetActive(false);
            if (topBarUI != null) topBarUI.gameObject.SetActive(false);
        }
        else
        {
            isLogoScreen = false;
            Time.timeScale = 0f;
            fadeAlpha = 1f;

            score = PlayerPrefs.GetInt("SavedScore", 0);

            // 💡 2스테이지부터는 시작하자마자 플레이어와 UI가 켜져 있어야 합니다.
            if (playerTransform != null) playerTransform.gameObject.SetActive(true);
            if (topBarUI != null) topBarUI.gameObject.SetActive(true);

            StartCoroutine(StageStartSequence(true));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isLogoScreen && !isGameOver && !isStageClear && !showStageStartText) TogglePause();
        }

        if (isGameOver && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private IEnumerator StageStartSequence(bool doFadeIn)
    {
        Vector3 targetPos = Vector3.zero;

        // 💡 [추가] 컷신 시작과 함께 숨겨뒀던 플레이어를 켭니다!
        if (playerTransform != null)
        {
            playerTransform.gameObject.SetActive(true); // 이제 나타남!

            if (playerAnimator != null) playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

            targetPos = playerTransform.position;
            playerTransform.position = targetPos + Vector3.left * 15f;

            if (playerAnimator != null) playerAnimator.SetBool("isMoving", true);
        }

        if (doFadeIn)
        {
            float duration = 1.0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeAlpha = 1f - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            fadeAlpha = 0f;
        }

        if (playerTransform != null)
        {
            while (Vector3.Distance(playerTransform.position, targetPos) > 0.1f)
            {
                playerTransform.position = Vector3.MoveTowards(playerTransform.position, targetPos, playerAutoMoveSpeed * Time.unscaledDeltaTime);
                yield return null;
            }
            playerTransform.position = targetPos;
            if (playerAnimator != null) playerAnimator.SetBool("isMoving", false);
        }

        showStageStartText = true;
        yield return new WaitForSecondsRealtime(1.5f);
        showStageStartText = false;

        if (playerAnimator != null) playerAnimator.updateMode = AnimatorUpdateMode.Normal;

        // 💡 [추가] 컷신이 끝나고 진짜 전투가 시작될 때 목숨 UI(TopBar)를 짠! 하고 나타나게 합니다.
        if (topBarUI != null)
        {
            topBarUI.gameObject.SetActive(true);
            topBarUI.PlaySlideIn();
        }

        Time.timeScale = 1f;
        ChangeBGM(mainBgm);
    }

    private IEnumerator StageClearSequence()
    {
        if (playerAnimator != null) playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        float slideDuration = 2.0f;
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / slideDuration;
            stageClearTextX = Mathf.Lerp(Screen.width, 0, 1f - Mathf.Pow(1f - t, 3f));
            yield return null;
        }
        stageClearTextX = 0f;

        yield return new WaitForSecondsRealtime(2.0f);

        elapsed = 0f;
        float textOutDuration = 0.8f;
        while (elapsed < textOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            stageClearTextX = Mathf.Lerp(0, -Screen.width, elapsed / textOutDuration);
            yield return null;
        }

        if (playerTransform != null)
        {
            if (playerAnimator != null) playerAnimator.SetBool("isMoving", true);
            float targetExitX = playerTransform.position.x + 15f;

            while (playerTransform.position.x < targetExitX)
            {
                playerTransform.position += Vector3.right * playerAutoMoveSpeed * Time.unscaledDeltaTime;
                yield return null;
            }
        }

        float fadeDuration = 1.0f;
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeAlpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        fadeAlpha = 1f;

        GoToNextStage();
    }

    public void ChangeBGM(AudioClip newClip)
    {
        if (bgmSource == null || newClip == null) return;
        if (bgmSource.clip == newClip && bgmSource.isPlaying) return;
        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();
    }

    public void OpenSettingsFromTitle() { currentMenu = MenuState.Settings; }
    public void OpenQuitConfirmFromTitle() { currentMenu = MenuState.ConfirmQuit; }

    public void StartGameSequence()
    {
        isLogoScreen = false;
        StartCoroutine(StageStartSequence(false));
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused) { Time.timeScale = 0f; currentMenu = MenuState.Main; }
        else { Time.timeScale = 1f; currentMenu = MenuState.None; }
    }

    public void AddScore(int amount) { if (isStageClear || isGameOver || isLogoScreen || isPaused) return; score += amount; }

    public void AddKill()
    {
        if (bossSpawned || isStageClear || isGameOver || isLogoScreen || isPaused) return;
        totalKills++;
        if (totalKills >= killsToSpawnBoss && !bossSpawned)
        {
            bossSpawned = true;
            TriggerBossEvent();
        }
    }

    void TriggerBossEvent()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies) { Destroy(enemy); }
        if (bossSpawner != null) bossSpawner.SpawnBoss();
        ChangeBGM(bossBgm);
    }

    public void SetBossHp(int current, int max) { showBossHp = true; currentBossHp = current; maxBossHp = max; }
    public void HideBossHp() { showBossHp = false; }

    public void ShowStageClear()
    {
        isStageClear = true;
        Time.timeScale = 0f;
        stageClearTextX = Screen.width;
        StartCoroutine(StageClearSequence());
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        ChangeBGM(gameOverBgm);
    }

    void GoToNextStage()
    {
        PlayerPrefs.SetInt("SavedScore", score);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        if (string.IsNullOrEmpty(nextStageName)) SceneManager.LoadScene(0);
        else SceneManager.LoadScene(nextStageName);
    }

    void OnGUI()
    {
        if (Event.current.type == EventType.Layout) return;
        if (isLogoScreen)
        {
            if (currentMenu != MenuState.None) DrawPauseMenu();
        }
        else
        {
            DrawGamePlayUI();
            if (isPaused) DrawPauseMenu();
            if (isStageClear || isGameOver) DrawResultUI();
        }

        if (showStageStartText)
        {
            DrawTextWithOutline(new Rect(0, 0, Screen.width, Screen.height), currentStageName + " START", centerAlertStyle, Color.black, 2f);
        }

        if (fadeAlpha > 0f)
        {
            Color oldColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, fadeAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
            GUI.color = oldColor;
        }
    }

    void DrawPauseMenu()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bgTexture);
        float btnW = Screen.width * 0.35f; float btnH = Screen.height * 0.1f; float centerX = (Screen.width - btnW) / 2f;

        if (currentMenu == MenuState.Main)
        {
            DrawTextWithOutline(new Rect(0, Screen.height * 0.1f, Screen.width, 150), "PAUSED", menuTitleStyle, Color.black, 2f);
            if (GUI.Button(new Rect(centerX, Screen.height * 0.35f, btnW, btnH), "CONTINUE", menuButtonStyle)) TogglePause();
            if (GUI.Button(new Rect(centerX, Screen.height * 0.47f, btnW, btnH), "SETTINGS", menuButtonStyle)) currentMenu = MenuState.Settings;
            if (GUI.Button(new Rect(centerX, Screen.height * 0.59f, btnW, btnH), "LOBBY", menuButtonStyle)) currentMenu = MenuState.ConfirmLobby;
            if (GUI.Button(new Rect(centerX, Screen.height * 0.71f, btnW, btnH), "QUIT", menuButtonStyle)) currentMenu = MenuState.ConfirmQuit;
        }
        else if (currentMenu == MenuState.Settings)
        {
            DrawTextWithOutline(new Rect(0, Screen.height * 0.1f, Screen.width, 150), "SETTINGS", menuTitleStyle, Color.black, 2f);
            Resolution res = resolutions[selectedResolutionIndex];
            string resText = $"{res.width} x {res.height}";

            if (GUI.Button(new Rect(centerX - 80, Screen.height * 0.35f, 70, btnH), "<", menuButtonStyle)) { selectedResolutionIndex--; if (selectedResolutionIndex < 0) selectedResolutionIndex = resolutions.Length - 1; }
            DrawTextWithOutline(new Rect(centerX, Screen.height * 0.35f, btnW, btnH), resText, menuLabelStyle, Color.black, 2f);
            if (GUI.Button(new Rect(centerX + btnW + 10, Screen.height * 0.35f, 70, btnH), ">", menuButtonStyle)) { selectedResolutionIndex++; if (selectedResolutionIndex >= resolutions.Length) selectedResolutionIndex = 0; }

            string screenModeText = Screen.fullScreen ? "FULLSCREEN: ON" : "FULLSCREEN: OFF";
            if (GUI.Button(new Rect(centerX, Screen.height * 0.48f, btnW, btnH), screenModeText, menuButtonStyle)) { Screen.fullScreen = !Screen.fullScreen; }
            if (GUI.Button(new Rect(centerX, Screen.height * 0.65f, btnW, btnH), "APPLY", menuButtonStyle)) { Resolution selectedRes = resolutions[selectedResolutionIndex]; Screen.SetResolution(selectedRes.width, selectedRes.height, Screen.fullScreen); }
            if (GUI.Button(new Rect(centerX, Screen.height * 0.77f, btnW, btnH), "BACK", menuButtonStyle)) { if (isLogoScreen) { currentMenu = MenuState.None; if (titleUI != null) titleUI.ShowButtons(); } else { currentMenu = MenuState.Main; } }
        }
        else if (currentMenu == MenuState.ConfirmLobby)
        {
            DrawTextWithOutline(new Rect(0, Screen.height * 0.25f, Screen.width, 150), "초기 화면으로 돌아가시겠습니까?", menuLabelStyle, Color.black, 2f);
            if (GUI.Button(new Rect(centerX - btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "예", menuButtonStyle)) SceneManager.LoadScene(0);
            if (GUI.Button(new Rect(centerX + btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "아니오", menuButtonStyle)) { if (isLogoScreen) { currentMenu = MenuState.None; if (titleUI != null) titleUI.ShowButtons(); } else { currentMenu = MenuState.Main; } }
        }
        else if (currentMenu == MenuState.ConfirmQuit)
        {
            DrawTextWithOutline(new Rect(0, Screen.height * 0.25f, Screen.width, 150), "게임을 종료하시겠습니까?", menuLabelStyle, Color.black, 2f);
            if (GUI.Button(new Rect(centerX - btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "예", menuButtonStyle)) Application.Quit();
            if (GUI.Button(new Rect(centerX + btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "아니오", menuButtonStyle)) { if (isLogoScreen) { currentMenu = MenuState.None; if (titleUI != null) titleUI.ShowButtons(); } else { currentMenu = MenuState.Main; } }
        }
    }

    void DrawGamePlayUI()
    {
        float p = Screen.width * 0.02f;
        float lw = Screen.width * 0.4f;
        int scoreFontSize = hudScoreStyle.fontSize == 0 ? 30 : hudScoreStyle.fontSize;

        DrawTextWithOutline(new Rect(Screen.width - lw - p, p, lw, scoreFontSize * 1.5f), "SCORE : " + score.ToString("N0"), hudScoreStyle, Color.black, 2f);
        DrawTextWithOutline(new Rect(Screen.width - lw - p, p + (scoreFontSize * 1.5f), lw, scoreFontSize * 1.5f), "KILLS : " + totalKills + " / " + killsToSpawnBoss, hudKillStyle, Color.black, 2f);

        if (showBossHp && !isStageClear && !isGameOver)
        {
            float bw = Screen.width * 0.4f;
            float bh = Screen.height * 0.03f;
            float topMargin = Screen.height * 0.08f;
            float startX = (Screen.width - bw) / 2f;

            GUI.DrawTexture(new Rect(startX, topMargin, bw, bh), bgTexture);
            float hp = (float)currentBossHp / maxBossHp;
            if (hp > 0) GUI.DrawTexture(new Rect(startX, topMargin, bw * hp, bh), fgTexture);
        }
    }

    void DrawResultUI()
    {
        if (isStageClear)
        {
            if (stageClearImage != null)
            {
                float imgWidth = Screen.width * 0.6f;
                float imgHeight = imgWidth * ((float)stageClearImage.height / stageClearImage.width);
                float drawX = stageClearTextX + (Screen.width - imgWidth) / 2f;
                float drawY = (Screen.height - imgHeight) / 2f;

                GUI.DrawTexture(new Rect(drawX, drawY, imgWidth, imgHeight), stageClearImage);
            }
            else
            {
                DrawTextWithOutline(new Rect(stageClearTextX, 0, Screen.width, Screen.height), "STAGE COMPLETE", centerAlertStyle, Color.black, 2f);
            }
        }
        else
        {
            DrawTextWithOutline(new Rect(0, -50, Screen.width, Screen.height), "GAME OVER", centerAlertStyle, Color.black, 2f);
            DrawTextWithOutline(new Rect(0, Screen.height * 0.1f, Screen.width, Screen.height), "Press Space to Restart", restartTextStyle, Color.black, 2f);
        }
    }

    public void TriggerBossDeathSequence()
    {
        StartCoroutine(BossClearCinematic());
    }

    private System.Collections.IEnumerator BossClearCinematic()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies) { Destroy(enemy); }

        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(2.5f);

        Time.timeScale = 0f;
        ShowStageClear();
    }

    // =======================================================
    // 🎨 테두리를 그려주는 핵심 함수 (두께 조절 가능)
    // =======================================================
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