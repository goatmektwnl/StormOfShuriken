using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public TextMeshProUGUI scoreText;
    public HeartController[] hearts;

    private int currentScore = 0;
    private int displayedScore = 0;
    private int currentHP;

    void Awake()
    {
        Instance = this;
        currentHP = hearts.Length;
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        StopAllCoroutines(); // ±‚¡∏ ∑—∏µ ∏ÿ√„
        StartCoroutine(RollScore(currentScore));
    }

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
        scoreText.text = target.ToString("D6");
    }

    public void TakeDamage()
    {
        if (currentHP > 0)
        {
            currentHP--;
            hearts[currentHP].BreakHeart(); // «œ∆Æ ±˙¡¸ æ÷¥œ∏ﬁ¿Ãº«
            StartCoroutine(CameraShake.Instance.Shake(0.2f, 0.15f)); // »≠∏È »ÁµÈ∏≤
        }
    }
}