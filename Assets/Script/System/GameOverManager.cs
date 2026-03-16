using System.Collections;
using TMPro;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TextMeshProUGUI restartText; // TextMeshPro를 쓰실 경우
    private bool isGameOver = false;
    public TopBarAnimation topBar;

    public void ShowGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        gameOverPanel.SetActive(true);
        Time.timeScale = 0.2f; // 슬로우 모션

        if (restartText != null)
        {
            // [체크] 함수 이름과 정확히 일치해야 합니다!
            StartCoroutine(BlinkRestartText());
        }

        // 1. 상단 바에게 나가라고 명령!
        if (topBar != null)
        {
            topBar.PlaySlideOut();
        }

        // 2. 원래 있던 게임 오버 로직 실행
        gameOverPanel.SetActive(true);
        Time.timeScale = 0.2f;
        // ... (나머지 코드)

    }

    // [체크] 앞부분이 꼭 IEnumerator 여야 합니다!
    IEnumerator BlinkRestartText()
    {
        while (isGameOver)
        {
            if (restartText != null)
            {
                // 텍스트 컴포넌트를 껐다 켰다 해서 깜빡이게 함
                restartText.enabled = !restartText.enabled;
            }
            // 슬로우 모션 중에도 정상 속도로 깜빡이게 Realtime 사용
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("G키 눌림! 강제로 화면 띄우기 시도!");
            ShowGameOver();
        }

        if (isGameOver && Input.GetMouseButtonDown(0))
        {
            RestartGame();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}