using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;      // 현재 점수
    public TextMeshProUGUI highScoreText; // 최고 점수

    public HeartController[] hearts;

    private int currentScore = 0;
    private int highScore = 0;
    private int displayedScore = 0;
    private int currentHP;

    void Awake()
    {
        Instance = this;

        // 하이 스코어 로드
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateHighScoreUI();

        if (hearts != null)
        {
            currentHP = hearts.Length;
        }
    }

    void Start()
    {
        // 시작 시 현재 점수를 000000으로 표시
        scoreText.text = 0.ToString("D6");
    }

    public void TakeDamage()
    {
        
        if (currentHP > 0)
        {
            // 1. 현재 HP를 하나 줄임 (3 -> 2)
            currentHP--;

            // 2. 줄어든 숫자를 번호(인덱스)로 사용
            if (hearts != null && hearts.Length > currentHP)
            {
                hearts[currentHP].BreakHeart();
            }
        }

        if (currentHP <= 0)
        {
            Invoke("TriggerGameOver", 1.0f); // 하트 깨지는 거 보고 1초 뒤에 띄우기
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        StopAllCoroutines();
        StartCoroutine(RollScore(currentScore));
    }

    // [하나만 남겨둔 RollScore!] 6자리 포맷 로직 포함
    IEnumerator RollScore(int target)
    {
        float duration = 0.4f;
        float elapsed = 0f;
        int start = displayedScore;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            displayedScore = (int)Mathf.Lerp(start, target, elapsed / duration);
            scoreText.text = displayedScore.ToString("D6");
            yield return null;
        }

        displayedScore = target;
        scoreText.text = target.ToString("D6");
    }

    public void CheckHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
            UpdateHighScoreUI();
        }
    }

    void UpdateHighScoreUI()
    {
        // 최고 점수도 깔끔하게 6자리로 표시
        highScoreText.text = highScore.ToString("D6");
    }

    public GameOverManager gameOverManager; // 인스펙터에서 GameOverManager 연결

   

    void TriggerGameOver()
    {
        gameOverManager.ShowGameOver();
    }
}