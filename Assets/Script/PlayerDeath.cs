using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public SpriteRenderer playerRenderer; // 플레이어의 몸체(SpriteRenderer)
    public Sprite deathSprite;            // 준비하신 죽는 이미지

    private bool isDead = false;

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. 이미지를 죽는 이미지로 교체!
        if (playerRenderer != null && deathSprite != null)
        {
            playerRenderer.sprite = deathSprite;
        }

        // 2. 애니메이터가 있다면 꺼줘야 합니다 (중요!)
        // 애니메이터가 켜져 있으면 이미지를 바꿔도 다시 원래대로 돌아오거든요.
        Animator anim = playerRenderer.GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = false;
        }

        // 3. (선택) 죽었을 때 플레이어를 살짝 회전시키거나 튕기게 하면 더 리얼합니다.
        transform.Rotate(0, 0, 90f);
    }
}