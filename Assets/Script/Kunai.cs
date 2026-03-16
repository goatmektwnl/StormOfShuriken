using UnityEngine;

public class Kunai : MonoBehaviour
{
    // 스포너가 값을 넣어줄 수 있도록 public으로 둡니다.
    public float speed;

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null) player.TakeDamage();
            Destroy(gameObject);
        }
    }
}