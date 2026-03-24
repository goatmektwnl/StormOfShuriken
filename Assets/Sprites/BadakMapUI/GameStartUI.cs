using System.Collections;
using UnityEngine;
using TMPro;

public class GameStartUI : MonoBehaviour
{
    [Header("오프닝 UI 요소들")]
    public GameObject logoBackgroundPanel; // 💡 1단계에서 만든 반투명 Panel을 넣을 곳!
    public GameObject logoImage;
    public GameObject pressSpaceText;
    public TextMeshProUGUI countdownText;

    private bool isWaitingForSpace = true;

    void Start()
    {
        // 게임 켜지자마자 모든 시간 정지!!
        Time.timeScale = 0f;

        // UI들 초기 세팅
        logoBackgroundPanel.SetActive(true); // 반투명 배경 켜기
        logoImage.SetActive(true);
        pressSpaceText.SetActive(true);
        countdownText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isWaitingForSpace && Input.GetKeyDown(KeyCode.Space))
        {
            isWaitingForSpace = false;
            StartCoroutine(StartCountdownRoutine()); // 카운트다운 및 로고 exit 연출 시작!
        }
    }

    IEnumerator StartCountdownRoutine()
    {
        // 1. "Press Space..." 글자는 즉시 치웁니다.
        pressSpaceText.SetActive(false);

        // 💡 [초특급 연출] 로고를 왼쪽으로 '슈우웅' 보내는 마법!!
        if (logoImage != null)
        {
            // 놈의 원래 위치를 기억합니다.
            Vector3 initialPos = logoImage.transform.position;
            // 놈을 보낼 목표 위치 (예: 화면 왼쪽 밖 2000 좌표)
            Vector3 targetPos = initialPos + Vector3.left * 2000f;

            float elapsedTime = 0f;
            float moveDuration = 0.5f; // 💡 슈우웅 지나가는 시간 (0.5초)

            // 0.5초 동안 로고를 targetPos로 잡아끕니다!!
            while (elapsedTime < moveDuration)
            {
                // [Lerp]라는 마법을 써서 initialPos와 targetPos 사이를 부드럽게 오갑니다!
                // 💡 Time.timeScale이 0일 때는 반드시 Time.unscaledDeltaTime을 써야 시간이 흐릅니다!!
                logoImage.transform.position = Vector3.Lerp(initialPos, targetPos, elapsedTime / moveDuration);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null; // 다음 프레임까지 대기
            }

            // 2. 슈우웅 지나가고 나면, 로고와 반투명 배경을 완전히 눈앞에서 치워버립니다.
            logoImage.SetActive(false);
            logoBackgroundPanel.SetActive(false); // 반투명 배경도 같이 숨기기
        }

        // 3. 이제 카운트다운 시작! (나머지는 이전과 동일)
        countdownText.gameObject.SetActive(true);
        int count = 3;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSecondsRealtime(1f);
            count--;
        }

        countdownText.text = "START!";
        yield return new WaitForSecondsRealtime(0.5f);
        countdownText.gameObject.SetActive(false);

        // [봉인 해제] 시간아 흘러라!! 진짜 전투 시작!!
        Time.timeScale = 1f;
    }
}
