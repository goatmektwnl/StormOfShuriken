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

    // 💡 [신규 핵심 추가] 컷신 연출을 위해 플레이어의 위치를 제어할 연결 고리입니다!
    [Header("시네마틱 컷신 설정")]
    [Tooltip("Hierarchy에 있는 플레이어(Player) 오브젝트를 여기에 끌어다 넣으세요.")]
    public Transform playerTransform;
    public float playerAutoMoveSpeed = 7f; // 자동 이동 속도 (입맛에 맞게 조절)

    [Header("오프닝 연출")]
    public bool isLogoScreen = true;

    [Header("Canvas UI 애니메이션 컨트롤러")]
    public TitleUIController titleUI;
    public TopBarAnimation topBarUI;

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

    // 플레이어의 애니메이터가 멈추지 않도록 백업해두는 변수
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
        // 시작할 때 플레이어 애니메이터를 미리 찾아둡니다.
        if (playerTransform != null) playerAnimator = playerTransform.GetComponent<Animator>();

        if (isFirstStage)
        {
            isLogoScreen = true;
            Time.timeScale = 0f;
            fadeAlpha = 0f;
        }
        else
        {
            isLogoScreen = false;
            Time.timeScale = 0f;
            fadeAlpha = 1f; // 새까만 화면에서 시작
            // 💡 2스테이지부터는 페이드인(true)과 함께 왼쪽에서 등장!
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

    // 💡 [핵심 개조] 화면 밖에서 안으로 멋지게 달려오는 등장 컷신!
    private IEnumerator StageStartSequence(bool doFadeIn)
    {
        Vector3 targetPos = Vector3.zero;

        // 1. 시간이 멈춰도 다리가 움직이도록 애니메이터 설정 변경!
        if (playerAnimator != null) playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (playerTransform != null)
        {
            targetPos = playerTransform.position; // 에디터에 배치된 원래(목표) 위치
            // 플레이어를 화면 왼쪽 저 멀리 밖으로 강제 이동시켜 둡니다.
            playerTransform.position = targetPos + Vector3.left * 15f;
            if (playerAnimator != null) playerAnimator.SetBool("isMoving", true); // 달리기 애니메이션 On!
        }

        // 2. 페이드 인 (검은 화면 걷어내기)
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

        // 3. 주인공이 왼쪽에서 중앙(목표 위치)으로 맹렬하게 달려옵니다!
        if (playerTransform != null)
        {
            while (Vector3.Distance(playerTransform.position, targetPos) > 0.1f)
            {
                playerTransform.position = Vector3.MoveTowards(playerTransform.position, targetPos, playerAutoMoveSpeed * Time.unscaledDeltaTime);
                yield return null;
            }
            playerTransform.position = targetPos; // 제자리에 완벽히 주차
            if (playerAnimator != null) playerAnimator.SetBool("isMoving", false); // 제자리에 서기!
        }

        // 4. "STAGE X START" 카운트다운 텍스트 띄우기
        showStageStartText = true;
        yield return new WaitForSecondsRealtime(1.5f);
        showStageStartText = false;

        // 5. 전투 개시 (애니메이터 원상 복구)
        if (playerAnimator != null) playerAnimator.updateMode = AnimatorUpdateMode.Normal;
        if (topBarUI != null) topBarUI.PlaySlideIn();
        Time.timeScale = 1f;
        ChangeBGM(mainBgm);
    }

    // 💡 [핵심 개조] 클리어 문구가 사라진 뒤 화면 오른쪽으로 퇴장하는 컷신!
    private IEnumerator StageClearSequence()
    {
        // 시간이 멈췄으므로, 애니메이터를 컷신용(Unscaled)으로 변경
        if (playerAnimator != null) playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        // 1. "STAGE COMPLETE" 글씨 등장 (중앙으로)
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

        // 2. 2초 동안 멋지게 포즈!
        yield return new WaitForSecondsRealtime(2.0f);

        // 3. "STAGE COMPLETE" 글씨가 화면 왼쪽 밖으로 휙! 하고 사라집니다.
        elapsed = 0f;
        float textOutDuration = 0.8f;
        while (elapsed < textOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            stageClearTextX = Mathf.Lerp(0, -Screen.width, elapsed / textOutDuration);
            yield return null;
        }

        // 4. 주인공 오른쪽 밖으로 질주 퇴장!
        if (playerTransform != null)
        {
            if (playerAnimator != null) playerAnimator.SetBool("isMoving", true); // 다시 달리기!
            float targetExitX = playerTransform.position.x + 15f; // 화면 오른쪽 밖의 위치

            while (playerTransform.position.x < targetExitX)
            {
                playerTransform.position += Vector3.right * playerAutoMoveSpeed * Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // 5. 완벽히 퇴장 후 페이드 아웃
        float fadeDuration = 1.0f;
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeAlpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        fadeAlpha = 1f;

        // 6. 씬 이동!
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
        // 💡 1스테이지에서도 로고에서 'START'를 누르면 멋지게 왼쪽에서 뛰어 들어오도록 통일했습니다!
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
        Time.timeScale = 0f; // 적과 탄환 일시정지!
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
            GUIStyle ss = new GUIStyle();
            ss.fontSize = Screen.width / 12;
            ss.fontStyle = FontStyle.Bold;
            ss.alignment = TextAnchor.MiddleCenter;
            ss.normal.textColor = Color.yellow;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), currentStageName + " START", ss);
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
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bgTexture); GUIStyle titleStyle = new GUIStyle(); titleStyle.fontSize = Screen.width / 15; titleStyle.normal.textColor = Color.white; titleStyle.alignment = TextAnchor.MiddleCenter; titleStyle.fontStyle = FontStyle.Bold; GUIStyle btnTextStyle = new GUIStyle(GUI.skin.button); btnTextStyle.fontSize = Screen.width / 30; btnTextStyle.fontStyle = FontStyle.Bold; GUIStyle labelStyle = new GUIStyle(); labelStyle.fontSize = Screen.width / 25; labelStyle.normal.textColor = Color.yellow; labelStyle.alignment = TextAnchor.MiddleCenter; labelStyle.fontStyle = FontStyle.Bold; float btnW = Screen.width * 0.35f; float btnH = Screen.height * 0.1f; float centerX = (Screen.width - btnW) / 2f; if (currentMenu == MenuState.Main) { GUI.Label(new Rect(0, Screen.height * 0.1f, Screen.width, 150), "PAUSED", titleStyle); if (GUI.Button(new Rect(centerX, Screen.height * 0.35f, btnW, btnH), "CONTINUE", btnTextStyle)) TogglePause(); if (GUI.Button(new Rect(centerX, Screen.height * 0.47f, btnW, btnH), "SETTINGS", btnTextStyle)) currentMenu = MenuState.Settings; if (GUI.Button(new Rect(centerX, Screen.height * 0.59f, btnW, btnH), "LOBBY", btnTextStyle)) currentMenu = MenuState.ConfirmLobby; if (GUI.Button(new Rect(centerX, Screen.height * 0.71f, btnW, btnH), "QUIT", btnTextStyle)) currentMenu = MenuState.ConfirmQuit; }
        else if (currentMenu == MenuState.Settings) { GUI.Label(new Rect(0, Screen.height * 0.1f, Screen.width, 150), "SETTINGS", titleStyle); Resolution res = resolutions[selectedResolutionIndex]; string resText = $"{res.width} x {res.height}"; if (GUI.Button(new Rect(centerX - 80, Screen.height * 0.35f, 70, btnH), "<", btnTextStyle)) { selectedResolutionIndex--; if (selectedResolutionIndex < 0) selectedResolutionIndex = resolutions.Length - 1; } GUI.Label(new Rect(centerX, Screen.height * 0.35f, btnW, btnH), resText, labelStyle); if (GUI.Button(new Rect(centerX + btnW + 10, Screen.height * 0.35f, 70, btnH), ">", btnTextStyle)) { selectedResolutionIndex++; if (selectedResolutionIndex >= resolutions.Length) selectedResolutionIndex = 0; } string screenModeText = Screen.fullScreen ? "FULLSCREEN: ON" : "FULLSCREEN: OFF"; if (GUI.Button(new Rect(centerX, Screen.height * 0.48f, btnW, btnH), screenModeText, btnTextStyle)) { Screen.fullScreen = !Screen.fullScreen; } if (GUI.Button(new Rect(centerX, Screen.height * 0.65f, btnW, btnH), "APPLY", btnTextStyle)) { Resolution selectedRes = resolutions[selectedResolutionIndex]; Screen.SetResolution(selectedRes.width, selectedRes.height, Screen.fullScreen); } if (GUI.Button(new Rect(centerX, Screen.height * 0.77f, btnW, btnH), "BACK", btnTextStyle)) { if (isLogoScreen) { currentMenu = MenuState.None; if (titleUI != null) titleUI.ShowButtons(); } else { currentMenu = MenuState.Main; } } }
        else if (currentMenu == MenuState.ConfirmLobby)
        {
            GUI.Label(new Rect(0, Screen.height * 0.25f, Screen.width, 150), "초기 화면으로 돌아가시겠습니까?", labelStyle);
            if (GUI.Button(new Rect(centerX - btnW * 0.55f, Screen.height * 0.5f, btnW, btnH), "예", btnTextStyle)) SceneManager.LoadScene(0);
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