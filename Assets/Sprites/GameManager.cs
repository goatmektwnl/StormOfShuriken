using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("오프닝 연출")]
    public bool isLogoScreen = true;

    [Header("Canvas UI 애니메이션 컨트롤러")]
    public TitleUIController titleUI;
    public TopBarAnimation topBarUI;

    [Header("BGM 설정")]
    public AudioSource bgmSource;   // BGM을 재생할 오디오 소스
    public AudioClip mainBgm;       // 일반 스테이지 BGM
    public AudioClip bossBgm;       // 보스 등장 BGM
    public AudioClip gameOverBgm;   // 게임 오버 BGM

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

    // 💡 불필요해진 lobbySceneName 변수는 삭제했습니다!

    private bool showBossHp = false;
    private int currentBossHp = 0;
    private int maxBossHp = 0;
    private Texture2D bgTexture;
    private Texture2D fgTexture;

    [Header("결과 화면 연출")]
    public Texture2D stageClearImage;
    private float stageClearTextX = 0f;

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
    }

    void Start()
    {
        isLogoScreen = true;
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isLogoScreen && !isGameOver && !isStageClear) TogglePause();
        }

        // 게임 오버 시 재시작 로직
        if (isGameOver && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // 💡 스테이지 클리어 상태에서 스페이스바를 누르면 초기 화면으로 복귀!
        if (isStageClear && Input.GetKeyDown(KeyCode.Space))
        {
            GoToLobby();
        }
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
        if (topBarUI != null) topBarUI.PlaySlideIn();
        Time.timeScale = 1f;
        ChangeBGM(mainBgm);
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
        StartCoroutine(SlideInStageClearText());
    }

    private System.Collections.IEnumerator SlideInStageClearText()
    {
        float duration = 2.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float easeOutT = 1f - Mathf.Pow(1f - t, 3f);
            stageClearTextX = Mathf.Lerp(Screen.width, 0, easeOutT);
            yield return null;
        }

        stageClearTextX = 0f;
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        ChangeBGM(gameOverBgm);
    }

    // 💡 [핵심 수정] 씬 이동 없이, 현재 씬을 다시 로드하여 초기 상태(오프닝 화면)로 만듭니다.
    void GoToLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnGUI() { if (Event.current.type == EventType.Layout) return; if (isLogoScreen) { if (currentMenu != MenuState.None) DrawPauseMenu(); return; } DrawGamePlayUI(); if (isPaused) DrawPauseMenu(); if (isStageClear || isGameOver) DrawResultUI(); }

    void DrawPauseMenu()
    { /* 기존 내용 유지 */
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bgTexture); GUIStyle titleStyle = new GUIStyle(); titleStyle.fontSize = Screen.width / 15; titleStyle.normal.textColor = Color.white; titleStyle.alignment = TextAnchor.MiddleCenter; titleStyle.fontStyle = FontStyle.Bold; GUIStyle btnTextStyle = new GUIStyle(GUI.skin.button); btnTextStyle.fontSize = Screen.width / 30; btnTextStyle.fontStyle = FontStyle.Bold; GUIStyle labelStyle = new GUIStyle(); labelStyle.fontSize = Screen.width / 25; labelStyle.normal.textColor = Color.yellow; labelStyle.alignment = TextAnchor.MiddleCenter; labelStyle.fontStyle = FontStyle.Bold; float btnW = Screen.width * 0.35f; float btnH = Screen.height * 0.1f; float centerX = (Screen.width - btnW) / 2f; if (currentMenu == MenuState.Main) { GUI.Label(new Rect(0, Screen.height * 0.1f, Screen.width, 150), "PAUSED", titleStyle); if (GUI.Button(new Rect(centerX, Screen.height * 0.35f, btnW, btnH), "CONTINUE", btnTextStyle)) TogglePause(); if (GUI.Button(new Rect(centerX, Screen.height * 0.47f, btnW, btnH), "SETTINGS", btnTextStyle)) currentMenu = MenuState.Settings; if (GUI.Button(new Rect(centerX, Screen.height * 0.59f, btnW, btnH), "LOBBY", btnTextStyle)) currentMenu = MenuState.ConfirmLobby; if (GUI.Button(new Rect(centerX, Screen.height * 0.71f, btnW, btnH), "QUIT", btnTextStyle)) currentMenu = MenuState.ConfirmQuit; }
        else if (currentMenu == MenuState.Settings) { GUI.Label(new Rect(0, Screen.height * 0.1f, Screen.width, 150), "SETTINGS", titleStyle); Resolution res = resolutions[selectedResolutionIndex]; string resText = $"{res.width} x {res.height}"; if (GUI.Button(new Rect(centerX - 80, Screen.height * 0.35f, 70, btnH), "<", btnTextStyle)) { selectedResolutionIndex--; if (selectedResolutionIndex < 0) selectedResolutionIndex = resolutions.Length - 1; } GUI.Label(new Rect(centerX, Screen.height * 0.35f, btnW, btnH), resText, labelStyle); if (GUI.Button(new Rect(centerX + btnW + 10, Screen.height * 0.35f, 70, btnH), ">", btnTextStyle)) { selectedResolutionIndex++; if (selectedResolutionIndex >= resolutions.Length) selectedResolutionIndex = 0; } string screenModeText = Screen.fullScreen ? "FULLSCREEN: ON" : "FULLSCREEN: OFF"; if (GUI.Button(new Rect(centerX, Screen.height * 0.48f, btnW, btnH), screenModeText, btnTextStyle)) { Screen.fullScreen = !Screen.fullScreen; } if (GUI.Button(new Rect(centerX, Screen.height * 0.65f, btnW, btnH), "APPLY", btnTextStyle)) { Resolution selectedRes = resolutions[selectedResolutionIndex]; Screen.SetResolution(selectedRes.width, selectedRes.height, Screen.fullScreen); } if (GUI.Button(new Rect(centerX, Screen.height * 0.77f, btnW, btnH), "BACK", btnTextStyle)) { if (isLogoScreen) { currentMenu = MenuState.None; if (titleUI != null) titleUI.ShowButtons(); } else { currentMenu = MenuState.Main; } } }
        else if (currentMenu == MenuState.ConfirmLobby)
        {
            GUI.Label(new Rect(0, Screen.height * 0.25f, Screen.width, 150), "초기 화면으로 돌아가시겠습니까?", labelStyle);
            // 💡 일시정지 창에서 로비(초기화면)로 갈 때도 현재 씬을 다시 부르도록 수정했습니다.
            if (GUI.Button(new Rect(centerX - btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "예", btnTextStyle)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            if (GUI.Button(new Rect(centerX + btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "아니오", btnTextStyle)) { if (isLogoScreen) { currentMenu = MenuState.None; if (titleUI != null) titleUI.ShowButtons(); } else { currentMenu = MenuState.Main; } }
        }
        else if (currentMenu == MenuState.ConfirmQuit) { GUI.Label(new Rect(0, Screen.height * 0.25f, Screen.width, 150), "게임을 종료하시겠습니까?", labelStyle); if (GUI.Button(new Rect(centerX - btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "예", btnTextStyle)) Application.Quit(); if (GUI.Button(new Rect(centerX + btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "아니오", btnTextStyle)) { if (isLogoScreen) { currentMenu = MenuState.None; if (titleUI != null) titleUI.ShowButtons(); } else { currentMenu = MenuState.Main; } } }
    }

    void DrawGamePlayUI() { int fs = Screen.width / 55; float p = Screen.width * 0.02f; float lw = Screen.width * 0.4f; GUIStyle s = new GUIStyle(); s.fontSize = fs; s.normal.textColor = Color.white; s.fontStyle = FontStyle.Bold; s.alignment = TextAnchor.UpperRight; GUI.Label(new Rect(Screen.width - lw - p, p, lw, fs), "SCORE : " + score.ToString("N0"), s); s.normal.textColor = new Color(1f, 0.3f, 0.3f); GUI.Label(new Rect(Screen.width - lw - p, p + fs * 1.3f, lw, fs), "KILLS : " + totalKills + " / " + killsToSpawnBoss, s); if (showBossHp && !isStageClear && !isGameOver) { float bw = Screen.width * 0.6f; float bh = Screen.height * 0.04f; GUI.DrawTexture(new Rect((Screen.width - bw) / 2f, Screen.height * 0.05f, bw, bh), bgTexture); float hp = (float)currentBossHp / maxBossHp; if (hp > 0) GUI.DrawTexture(new Rect((Screen.width - bw) / 2f, Screen.height * 0.05f, bw * hp, bh), fgTexture); } }

    void DrawResultUI()
    {
        GUIStyle s = new GUIStyle();
        s.fontSize = Screen.width / 15;
        s.fontStyle = FontStyle.Bold;
        s.alignment = TextAnchor.MiddleCenter;

        GUIStyle rs = new GUIStyle();
        rs.fontSize = Screen.width / 30;
        rs.normal.textColor = Color.white;
        rs.alignment = TextAnchor.MiddleCenter;

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
                s.normal.textColor = Color.yellow;
                GUI.Label(new Rect(stageClearTextX, 0, Screen.width, Screen.height), "STAGE COMPLETE", s);
            }

            // 💡 [텍스트 수정] "Lobby" 대신 "Title"로 문구를 변경했습니다.
            GUI.Label(new Rect(stageClearTextX, Screen.height * 0.25f, Screen.width, Screen.height), "Press Space to Return Title", rs);
        }
        else
        {
            s.normal.textColor = Color.red;
            GUI.Label(new Rect(0, -50, Screen.width, Screen.height), "GAME OVER", s);
            GUI.Label(new Rect(0, Screen.height * 0.1f, Screen.width, Screen.height), "Press Space to Restart", rs);
        }
    }

    public void TriggerBossDeathSequence()
    {
        StartCoroutine(BossClearCinematic());
    }

    private System.Collections.IEnumerator BossClearCinematic()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        Time.timeScale = 0.1f;

        yield return new WaitForSecondsRealtime(2.5f);

        Time.timeScale = 0f;
        ShowStageClear();
    }
}