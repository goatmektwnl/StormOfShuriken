using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int hp = 3;
    private bool isInvincible = false;
    private SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("EnemyBullet") && !isInvincible)
        {
            TakeDamage();
        }
    }

    void TakeDamage()
    {
        hp--;
        UIManager.Instance.TakeDamage();
        if (hp > 0) StartCoroutine(InvincibilityRoutine());
        else Debug.Log("Game Over");
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 1.5f;
        while (timer > 0)
        {
            sr.enabled = !sr.enabled; // ±ôºıÀÓ ¿¬Ãâ
            yield return new WaitForSeconds(0.1f);
            timer -= 0.1f;
        }
        sr.enabled = true;
        isInvincible = false;
    }
}