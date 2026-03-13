using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGameStarted = false;

    void Awake() => Instance = this;

    void Update()
    {
        // 대기 상태에서 마우스 클릭 시 게임 시작
        if (!isGameStarted && Input.GetMouseButtonDown(0))
        {
            StartGame();
        }
    }

    void StartGame()
    {
        isGameStarted = true;

        // 타이틀 연출(로고 퇴장, 글씨 제거) 실행
        FindObjectOfType<TitleUIController>().PlayStartEffect();

        // 상단 바 인트로 실행
        FindObjectOfType<TopBarAnimator>().PlaySlideIn();

        Debug.Log("전투 개시!");
    }
}