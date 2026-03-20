using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    // 쓰러지는 모션이 유지될 시간 (애니메이션 길이에 맞춰 에디터에서 조절하세요)
    public float destroyTime = 1f;

    void Start()
    {
        // 생성된 후 설정한 시간이 지나면 자동으로 파괴됩니다.
        Destroy(gameObject, destroyTime);
    }
}
