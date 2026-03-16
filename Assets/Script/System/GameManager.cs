using UnityEngine;
using UnityEngine.SceneManagement; // [추가] 씬 전환을 위해 필요

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGameStarted = false;
    public bool isGameOver = false; // [추가] 게임 오버 상태 체크
    public PlayerController player;    // 닌자 스크립트
    public TopBarAnimation topBar;   // 상단 바 스크립트
    public GameObject gameOverPanel;
    public static GameManager instance;
    private SpriteRenderer sr;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // 씬이 바뀌어도 파괴되지 않게 안전장치를 겁니다 (선택사항)
            // DontDestroyOnLoad(gameObject); 
            Debug.Log("GameManager: 인스턴스 등록 성공!");
        }
        else
        {
            // 혹시라도 GameManager가 두 개라면 하나를 지웁니다.

        }
    }

    void Update()
    {
        // 대기 상태에서 마우스 클릭 시 게임 시작
        if (!isGameStarted && Input.GetMouseButtonDown(0))
        {
            StartGame();
        }

        // [추가] 게임 오버 상태에서 클릭하면 씬 재시작
        if (isGameOver && Input.GetMouseButtonDown(0))
        {
            RestartGame();
        }
    }

    public void OnGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        // 1. 하이 스코어 체크 및 저장
        UIManager.Instance.CheckHighScore();

        Debug.Log("게임 오버! 다시 시작하려면 클릭하세요.");
    }

    void RestartGame()
    {
        // 현재 활성화된 씬을 다시 로드 (게임 초기화)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void StartGame()
    {
        isGameStarted = true;

        // 타이틀 연출(로고 퇴장, 글씨 제거) 실행
        FindObjectOfType<TitleUIController>().PlayStartEffect();

        // 상단 바 인트로 실행
        FindObjectOfType<TopBarAnimation>().PlaySlideIn();

        Debug.Log("전투 개시!");
    }

    public void OnPlayerDie()
    {
        // 1. 닌자에게 죽는 애니메이션 재생 명령 (지우지 않음!)
        if (player != null)
        {
            player.Die();
        }

        // 2. 상단 바 위로 슬라이드 아웃
        if (topBar != null)
        {
            topBar.PlaySlideOut();
        }

        // 3. 게임 오버 판넬 등장
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 4. 시간 느리게 (슬로우 모션)
        Time.timeScale = 0.2f;
    }
}