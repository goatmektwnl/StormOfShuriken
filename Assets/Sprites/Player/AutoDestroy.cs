using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float destroyTime = 0.5f; // 💡 0.5초 뒤에 삭제 (원하시면 0.3이나 1.0으로 조절 가능!)

    void Start()
    {
        // 태어나자마자 파괴 타이머 작동!
        Destroy(gameObject, destroyTime);
    }
}
