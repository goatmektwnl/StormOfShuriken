using UnityEngine;

public class ShurikenMove : MonoBehaviour
{
    [Header("이동 및 회전 속도")]
    public float speed = 10f;       // 앞으로 날아가는 속도
    public float spinSpeed = 800f;  // 칼날이 뱅글뱅글 도는 속도!

    void Update()
    {
        // 1. 앞으로 전진 (오른쪽)
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);

        // 2. 4방향 칼날 회전 효과 (Z축을 기준으로 빙글빙글 돕니다)
        transform.Rotate(0, 0, -spinSpeed * Time.deltaTime);

        // 💡 [수정] 자체적인 파괴 로직(X > 20f 조건문)을 제거했습니다.
        // 이제 이 수리검의 수명은 'SpreadShooter' 스크립트에서 전적으로 관리합니다.
    }
}