using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("오프닝 연출 (로고 이미지)")]
    public Texture2D logoTexture;
    private bool isLogoScreen = true;
    private bool isCountdown = false;
    private float countdownTimer = 3f;

    [Header("일시정지 및 메뉴 설정")]
    private bool isPaused = false;
    private enum MenuState { None, Main, ConfirmLobby, ConfirmQuit, Settings }
    private MenuState currentMenu = MenuState.None;

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

    void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(gameObject); return; }

        resolutions = Screen.resolutions;

        bgTexture = new Texture2D(1, 1);
        bgTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.95f)); // 더 어둡고 진하게
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
            if (!isLogoScreen && !isGameOver && !isStageClear && !isCountdown)
            {
                TogglePause();
            }
        }

        if (isPaused) return;

        if (isCountdown)
        {
            countdownTimer -= Time.unscaledDeltaTime;
            if (countdownTimer <= 0f)
            {
                isCountdown = false;
                Time.timeScale = 1f;
            }
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (Input.GetMouseButtonDown(0) && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (isLogoScreen)
            {
                isLogoScreen = false;
                isCountdown = true;
                countdownTimer = 3f;
            }
            else if (isGameOver)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Time.timeScale = 0f;
            currentMenu = MenuState.Main;
        }
        else
        {
            Time.timeScale = 1f;
            currentMenu = MenuState.None;
        }
    }

    public void AddScore(int amount) { if (isStageClear || isGameOver || isLogoScreen || isCountdown || isPaused) return; score += amount; }
    public void AddKill() { if (bossSpawned || isStageClear || isGameOver || isLogoScreen || isCountdown || isPaused) return; totalKills++; if (totalKills >= killsToSpawnBoss && !bossSpawned) { bossSpawned = true; TriggerBossEvent(); } }
    void TriggerBossEvent() { GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); foreach (GameObject enemy in enemies) { Destroy(enemy); } if (bossSpawner != null) bossSpawner.SpawnBoss(); }
    public void SetBossHp(int current, int max) { showBossHp = true; currentBossHp = current; maxBossHp = max; }
    public void HideBossHp() { showBossHp = false; }
    public void ShowStageClear() { isStageClear = true; Time.timeScale = 0f; }
    public void GameOver() { isGameOver = true; Time.timeScale = 0f; }

    void OnGUI()
    {
        // 💡 Layout 이벤트를 무시해야 클릭이 정상적으로 인식됩니다.
        if (Event.current.type == EventType.Layout) return;

        if (isLogoScreen || isCountdown)
        {
            DrawOpeningUI();
            return;
        }

        DrawGamePlayUI();

        if (isPaused)
        {
            DrawPauseMenu();
        }

        if (isStageClear || isGameOver) DrawResultUI();
    }

    void DrawPauseMenu()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bgTexture);

        // 💡 폰트 스타일 설정
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = Screen.width / 15; // 대왕 타이틀
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;

        GUIStyle btnTextStyle = new GUIStyle(GUI.skin.button);
        btnTextStyle.fontSize = Screen.width / 30; // 큼직한 버튼 글꼴
        btnTextStyle.fontStyle = FontStyle.Bold;

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = Screen.width / 25; // 노란색 안내 문구용
        labelStyle.normal.textColor = Color.yellow;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontStyle = FontStyle.Bold;

        // 버튼 규격 확대
        float btnW = Screen.width * 0.35f;
        float btnH = Screen.height * 0.1f;
        float centerX = (Screen.width - btnW) / 2f;

        if (currentMenu == MenuState.Main)
        {
            GUI.Label(new Rect(0, Screen.height * 0.1f, Screen.width, 150), "PAUSED", titleStyle);

            if (GUI.Button(new Rect(centerX, Screen.height * 0.35f, btnW, btnH), "CONTINUE", btnTextStyle)) TogglePause();
            if (GUI.Button(new Rect(centerX, Screen.height * 0.47f, btnW, btnH), "SETTINGS", btnTextStyle)) currentMenu = MenuState.Settings;
            if (GUI.Button(new Rect(centerX, Screen.height * 0.59f, btnW, btnH), "LOBBY", btnTextStyle)) currentMenu = MenuState.ConfirmLobby;
            if (GUI.Button(new Rect(centerX, Screen.height * 0.71f, btnW, btnH), "QUIT", btnTextStyle)) currentMenu = MenuState.ConfirmQuit;
        }
        else if (currentMenu == MenuState.Settings)
        {
            GUI.Label(new Rect(0, Screen.height * 0.1f, Screen.width, 150), "SETTINGS", titleStyle);

            Resolution res = resolutions[selectedResolutionIndex];
            string resText = $"{res.width} x {res.height}";

            if (GUI.Button(new Rect(centerX - 80, Screen.height * 0.35f, 70, btnH), "<", btnTextStyle))
            {
                selectedResolutionIndex--; if (selectedResolutionIndex < 0) selectedResolutionIndex = resolutions.Length - 1;
            }
            GUI.Label(new Rect(centerX, Screen.height * 0.35f, btnW, btnH), resText, labelStyle);
            if (GUI.Button(new Rect(centerX + btnW + 10, Screen.height * 0.35f, 70, btnH), ">", btnTextStyle))
            {
                selectedResolutionIndex++; if (selectedResolutionIndex >= resolutions.Length) selectedResolutionIndex = 0;
            }

            string screenModeText = Screen.fullScreen ? "FULLSCREEN: ON" : "FULLSCREEN: OFF";
            if (GUI.Button(new Rect(centerX, Screen.height * 0.48f, btnW, btnH), screenModeText, btnTextStyle))
            {
                Screen.fullScreen = !Screen.fullScreen;
            }

            if (GUI.Button(new Rect(centerX, Screen.height * 0.65f, btnW, btnH), "APPLY", btnTextStyle))
            {
                Resolution selectedRes = resolutions[selectedResolutionIndex];
                Screen.SetResolution(selectedRes.width, selectedRes.height, Screen.fullScreen);
            }
            if (GUI.Button(new Rect(centerX, Screen.height * 0.77f, btnW, btnH), "BACK", btnTextStyle)) currentMenu = MenuState.Main;
        }
        else if (currentMenu == MenuState.ConfirmLobby)
        {
            GUI.Label(new Rect(0, Screen.height * 0.25f, Screen.width, 150), "로비로 돌아가시겠습니까?", labelStyle);
            if (GUI.Button(new Rect(centerX - btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "예", btnTextStyle)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            if (GUI.Button(new Rect(centerX + btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "아니오", btnTextStyle)) currentMenu = MenuState.Main;
        }
        else if (currentMenu == MenuState.ConfirmQuit)
        {
            GUI.Label(new Rect(0, Screen.height * 0.25f, Screen.width, 150), "게임을 종료하시겠습니까?", labelStyle);
            if (GUI.Button(new Rect(centerX - btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "예", btnTextStyle)) Application.Quit();
            if (GUI.Button(new Rect(centerX + btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "아니오", btnTextStyle)) currentMenu = MenuState.Main;
        }
    }

    void DrawOpeningUI() { if (isLogoScreen) { if (logoTexture != null) { float logoWidth = Screen.width * 0.7f; float logoHeight = logoWidth * ((float)logoTexture.height / logoTexture.width); GUI.DrawTexture(new Rect((Screen.width - logoWidth) / 2f, (Screen.height - logoHeight) / 2f, logoWidth, logoHeight), logoTexture); } GUIStyle s = new GUIStyle(); s.fontSize = Screen.width / 25; s.normal.textColor = Color.white; s.alignment = TextAnchor.MiddleCenter; GUI.Label(new Rect(0, Screen.height * 0.8f, Screen.width, 50), "- Press Space or Touch to Start -", s); } if (isCountdown) { int t = Mathf.CeilToInt(countdownTimer); GUIStyle s = new GUIStyle(); s.fontSize = Screen.width / 10; s.normal.textColor = Color.red; s.alignment = TextAnchor.MiddleCenter; GUI.Label(new Rect(0, 0, Screen.width, Screen.height), t > 0 ? t.ToString() : "START!", s); } }
    void DrawGamePlayUI() { int fs = Screen.width / 55; float p = Screen.width * 0.02f; float lw = Screen.width * 0.4f; GUIStyle s = new GUIStyle(); s.fontSize = fs; s.normal.textColor = Color.white; s.fontStyle = FontStyle.Bold; s.alignment = TextAnchor.UpperRight; GUI.Label(new Rect(Screen.width - lw - p, p, lw, fs), "SCORE : " + score.ToString("N0"), s); s.normal.textColor = new Color(1f, 0.3f, 0.3f); GUI.Label(new Rect(Screen.width - lw - p, p + fs * 1.3f, lw, fs), "KILLS : " + totalKills + " / " + killsToSpawnBoss, s); if (showBossHp && !isStageClear && !isGameOver) { float bw = Screen.width * 0.6f; float bh = Screen.height * 0.04f; GUI.DrawTexture(new Rect((Screen.width - bw) / 2f, Screen.height * 0.05f, bw, bh), bgTexture); float hp = (float)currentBossHp / maxBossHp; if (hp > 0) GUI.DrawTexture(new Rect((Screen.width - bw) / 2f, Screen.height * 0.05f, bw * hp, bh), fgTexture); } }
    void DrawResultUI() { GUIStyle s = new GUIStyle(); s.fontSize = Screen.width / 15; s.fontStyle = FontStyle.Bold; s.alignment = TextAnchor.MiddleCenter; if (isStageClear) { s.normal.textColor = Color.yellow; GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "STAGE COMPLETE", s); } else { s.normal.textColor = Color.red; GUI.Label(new Rect(0, -50, Screen.width, Screen.height), "GAME OVER", s); GUIStyle rs = new GUIStyle(); rs.fontSize = Screen.width / 30; rs.normal.textColor = Color.white; rs.alignment = TextAnchor.MiddleCenter; GUI.Label(new Rect(0, Screen.height * 0.1f, Screen.width, Screen.height), "Press Space to Restart", rs); } }
}