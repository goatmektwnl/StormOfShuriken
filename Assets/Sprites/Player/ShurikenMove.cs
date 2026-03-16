using UnityEngine;

public class ShurikenMove : MonoBehaviour
{
    public float speed = 10f;       // 앞으로 날아가는 속도
    public float spinSpeed = 800f;  // 💡 칼날이 뱅글뱅글 도는 속도! (숫자가 클수록 위협적입니다)

    void Update()
    {
        // 1. 앞으로 전진 (오른쪽)
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);

        // 2. 4방향 칼날 회전 효과 (Z축을 기준으로 빙글빙글 돕니다)
        transform.Rotate(0, 0, -spinSpeed * Time.deltaTime);

        // 3. 화면 밖으로 나가면 삭제
        if (transform.position.x > 20f)
        {
            Destroy(gameObject);
        }
    }
}
