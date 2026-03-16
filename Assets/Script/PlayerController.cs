using UnityEngine;
using System.Collections; // Coroutine을 쓰기 위해 필요해요!

public class PlayerController : MonoBehaviour
{
    [Header("죽음 애니메이션 설정")]
    // ★ Sprite 하나가 아니라 '배열[]'로 만듭니다! ★
    public Sprite[] deathSprites;
    public float frameRate = 0.1f; // 다음 그림으로 넘어가는 속도

    private SpriteRenderer sr;
    private Animator anim;
    private bool isDead = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }


    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("닌자: 죽는 애니메이션 재생 시작!");

        // [중요] 충돌체만 끕니다. (지우지 않음!)
        // 이렇게 해야 바닥을 뚫고 내려가거나 다른 적과 또 부딪히지 않아요.
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        if (col != null) col.enabled = false;

        // 애니메이터 끄기
        if (anim != null) anim.enabled = false;

        // 코루틴 시작 (여기서 이미지를 한 장씩 바꿉니다)
        StartCoroutine(PlayDeathAnimation());
    }

    // 그림을 한 장씩 순서대로 보여주는 마법의 함수
    IEnumerator PlayDeathAnimation()
    {
        Debug.Log($"[검거] 애니메이션 시작! 총 {deathSprites.Length}장의 이미지를 준비했습니다.");

        for (int i = 0; i < deathSprites.Length; i++)
        {
            if (deathSprites[i] != null)
            {
                sr.sprite = deathSprites[i];
                Debug.Log($"[검거] 현재 {i}번째 이미지로 교체 완료: {deathSprites[i].name}");
            }
            else
            {
                Debug.LogError($"[검거] 에러: {i}번 칸에 이미지가 없습니다!");
            }

            yield return new WaitForSecondsRealtime(frameRate);
        }
    }

}
