using UnityEngine;

public class AutoDestroyAnimation : MonoBehaviour
{
    // 💡 애니메이션이 끝날 때쯤 스스로 파괴되는 간단한 기능입니다.
    // 횡스크롤 게임의 메모리 관리에 필수입니다.

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            // 현재 애니메이션 클립의 길이를 계산합니다.
            float animLength = animator.GetCurrentAnimatorStateInfo(0).length;

            // 애니메이션이 끝나면 자신을 파괴하라고 명령합니다.
            Destroy(gameObject, animLength);
        }
        else
        {
            // Animator가 없다면 1초 뒤에 파괴 (안전장치)
            Destroy(gameObject, 1f);
        }
    }
}