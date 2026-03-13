using UnityEngine;

public class MapScroller : MonoBehaviour
{
    public float scrollSpeed = 5f; // 맵이 다가오는 속도
    public float mapWidth = 20f;   // 타일맵 하나의 가로 길이 (에디터에서 수정)

    void Update()
    {
        // 1. 매 프레임 왼쪽으로 이동
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // 2. 맵이 왼쪽으로 자신의 가로 길이만큼 완전히 이동했다면?
        if (transform.position.x <= -mapWidth)
        {
            // 현재 위치에서 맵 2개 길이만큼 다시 오른쪽으로 순간이동시킵니다.
            transform.position += new Vector3(mapWidth * 2f, 0, 0);
        }
    }
}
