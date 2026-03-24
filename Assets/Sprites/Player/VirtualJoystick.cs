using UnityEngine;
using UnityEngine.EventSystems;

// 💡 남의 에셋에 의존하지 않는, 100% 독립적인 터치 조이스틱 코드입니다!
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private RectTransform bgRect;
    private RectTransform handleRect;
    private Vector2 inputVector;

    void Start()
    {
        bgRect = GetComponent<RectTransform>();
        // 💡 이 스크립트가 붙은 오브젝트의 '첫 번째 자식'을 조이스틱 손잡이로 자동 인식합니다!
        handleRect = transform.GetChild(0).GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData ped) { OnDrag(ped); }

    public void OnDrag(PointerEventData ped)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgRect, ped.position, ped.pressEventCamera, out pos))
        {
            pos.x = (pos.x / bgRect.sizeDelta.x);
            pos.y = (pos.y / bgRect.sizeDelta.y);

            inputVector = new Vector2(pos.x * 2, pos.y * 2);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // 손잡이(Handle)가 배경(Bg) 밖으로 나가지 않게 가둬둡니다!
            handleRect.anchoredPosition = new Vector2(inputVector.x * (bgRect.sizeDelta.x / 2), inputVector.y * (bgRect.sizeDelta.y / 2));
        }
    }

    public void OnPointerUp(PointerEventData ped)
    {
        // 손가락을 떼면 손잡이가 정중앙으로 돌아가고, 움직임도 0이 됩니다!
        inputVector = Vector2.zero;
        handleRect.anchoredPosition = Vector2.zero;
    }

    // 💡 PlayerController가 방향을 물어볼 때 대답해주는 함수들!
    public float GetHorizontal() { return inputVector.x; }
    public float GetVertical() { return inputVector.y; }
}
