using UnityEngine;

public class BlinkEffect : MonoBehaviour
{
    [Header("반짝임 설정")]
    public float blinkSpeed = 5f; // 💡 깜빡이는 속도 (숫자가 클수록 호들갑스럽게 깜빡입니다!)
    public float minAlpha = 0.3f; // 💡 제일 투명해질 때의 정도 (0.0으로 하면 아예 안 보였다 나타납니다)
    public float maxAlpha = 1.0f; // 💡 제일 선명할 때의 정도

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // 내 몸에 붙어있는 이미지(Sprite Renderer)를 찾아옵니다.
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (spriteRenderer != null)
        {
            // 💡 PingPong 마법: 시간이 지남에 따라 숫자가 탁구공처럼 0과 1 사이를 오갑니다!
            float pingPongValue = Mathf.PingPong(Time.time * blinkSpeed, 1f);

            // 투명도(Alpha) 값을 최소~최대 사이에서 부드럽게 오르락내리락하게 만듭니다.
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, pingPongValue);

            // 이미지에 새로운 투명도를 적용!
            Color currentColor = spriteRenderer.color;
            currentColor.a = currentAlpha;
            spriteRenderer.color = currentColor;
        }
    }
}
