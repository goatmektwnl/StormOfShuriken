using UnityEngine;

public class MovingItem : MonoBehaviour
{
    public float moveSpeed = 3f;

    // 💡 에디터에서 마음대로 조절할 수 있는 '사라지는 X 좌표' (기본값 -15f)
    public float destroyX = -15f;

    void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        // 💡 내 X 좌표가 destroyX 값보다 더 왼쪽으로 가면 파괴!
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}
